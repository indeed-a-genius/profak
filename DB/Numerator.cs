using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ProFak.DB;

public class Numerator : Rekord<Numerator>
{
	public PrzeznaczenieNumeratora Przeznaczenie { get; set; } = PrzeznaczenieNumeratora.Faktura;
	public string Format { get; set; } = "[Numer]";
	public string? Grupa { get; set; }

	public string PrzeznaczenieFmt => Format(Przeznaczenie);

	public List<StanNumeratora> Stany { get; set; } = default!;

	public override bool CzyPasuje(string fraza)
		=> base.CzyPasuje(fraza)
		|| CzyPasuje(PrzeznaczenieFmt, fraza)
		|| CzyPasuje(Format, fraza)
		|| CzyPasuje(Grupa, fraza);

	public static string NadajNumer(Baza baza, PrzeznaczenieNumeratora przeznaczenie, Func<string, IFormattable?> podstawienie, bool zwiekszLicznik = true)
	{
		baza.Zablokuj<Numerator>();
		var numerator = baza.Numeratory.FirstOrDefault(numerator => numerator.Przeznaczenie == przeznaczenie);
		if (numerator == null) throw new ApplicationException($"Brak definicji numeratora \"{przeznaczenie}\" - dodaj pozycję w spisie \"Serwisowe\" - \"Numeracja\".");
		return numerator.NadajNumer(baza, podstawienie, zwiekszLicznik);
	}

	public string GenerujGrupe(Func<string, IFormattable?> podstawienie) => Podstaw(String.IsNullOrEmpty(Grupa) ? Format : Grupa, podstawienie, default);
	public string GenerujNumer(Func<string, IFormattable?> podstawienie, int licznik) => Podstaw(Format, podstawienie, licznik);

	public string NadajNumer(Baza baza, Func<string, IFormattable?> podstawienie, bool zwiekszLicznik = true)
	{
		var parametry = GenerujGrupe(podstawienie);

		var stanNumeratora = baza.StanyNumeratorow.FirstOrDefault(stan => stan.NumeratorId == Id && stan.Parametry == parametry);
		if (stanNumeratora == null) stanNumeratora = new StanNumeratora { NumeratorRef = this, Parametry = parametry, OstatniaWartosc = 0 };

		var licznik = Math.Max(stanNumeratora.OstatniaWartosc, NajwyzszyNadanyNumer(baza, podstawienie)) + 1;
		var numer = GenerujNumer(podstawienie, licznik);
		if (zwiekszLicznik)
		{
			stanNumeratora.OstatniaWartosc = licznik;
			baza.Zapisz(stanNumeratora);
		}

		return numer;
	}

	/// <summary>
	/// Zwraca najwyższy numer kolejny wynikający z numerów dokumentów sprzedaży zapisanych już w bazie,
	/// pasujących do formatu numeratora z podstawionymi datami bieżącej faktury. Dzięki temu numeracja
	/// uwzględnia także dokumenty, które nie przeszły przez licznik - np. faktury sprzedaży zaimportowane
	/// z KSeF - i kolejny numer jest nadawany zawsze za najwyższym faktycznie użytym w danej grupie.
	/// </summary>
	private int NajwyzszyNadanyNumer(Baza baza, Func<string, IFormattable?> podstawienie)
	{
		var wzorzec = new StringBuilder("^");
		var zawieraNumer = false;
		var pozycja = 0;
		foreach (Match token in Regex.Matches(Format, @"\[(?<nazwa>[^:\]]+)(:(?<format>[^\]]+))?\]"))
		{
			wzorzec.Append(Regex.Escape(Format.Substring(pozycja, token.Index - pozycja)));
			pozycja = token.Index + token.Length;
			var nazwa = token.Groups["nazwa"].Value;
			if (String.Equals(nazwa, "numer", StringComparison.CurrentCultureIgnoreCase))
			{
				wzorzec.Append(@"(?<numer>\d+)");
				zawieraNumer = true;
			}
			else
			{
				var wartosc = podstawienie(nazwa);
				if (wartosc == null) return 0;
				var format = token.Groups["format"].Success ? token.Groups["format"].Value : null;
				var tekst = String.IsNullOrWhiteSpace(format) ? wartosc.ToString() ?? "" : wartosc.ToString(format, CultureInfo.CurrentCulture);
				wzorzec.Append(Regex.Escape(tekst));
			}
		}
		wzorzec.Append(Regex.Escape(Format.Substring(pozycja)));
		wzorzec.Append('$');
		if (!zawieraNumer) return 0;

		var wyrazenie = new Regex(wzorzec.ToString(), RegexOptions.IgnoreCase);
		var najwyzszy = 0;
		var numery = baza.Faktury
			.Where(faktura => faktura.Rodzaj != RodzajFaktury.Zakup && faktura.Rodzaj != RodzajFaktury.KorektaZakupu && faktura.Rodzaj != RodzajFaktury.Usunięta)
			.Select(faktura => faktura.Numer);
		foreach (var numer in numery)
		{
			if (String.IsNullOrEmpty(numer)) continue;
			var dopasowanie = wyrazenie.Match(numer);
			if (dopasowanie.Success && Int32.TryParse(dopasowanie.Groups["numer"].Value, out var wartoscNumeru) && wartoscNumeru > najwyzszy) najwyzszy = wartoscNumeru;
		}
		return najwyzszy;
	}

	private static string Podstaw(string format, Func<string, IFormattable?> podstawienie, int? numer)
	{
		return Regex.Replace(format, @"\[(?<nazwa>[^:\]]+)(:(?<format>[^\]]+))?\]", fragment =>
		{
			var nazwa = fragment.Groups["nazwa"].Value;
			var format = fragment.Groups["format"]?.Value;
			if (String.Equals(nazwa, "numer", StringComparison.CurrentCultureIgnoreCase)) return numer is null ? "" : numer.Value.ToString(format);
			var wartosc = podstawienie(nazwa);
			if (wartosc == null) throw new ApplicationException($"Nieznane wyrażenie numeratora \"{nazwa}\".");
			var tekst = String.IsNullOrWhiteSpace(format) ? wartosc.ToString() ?? "" : wartosc.ToString(format, CultureInfo.CurrentCulture);
			return tekst;
		});
	}
}

public enum PrzeznaczenieNumeratora
{
	Faktura,
	Proforma,
	KorektaSprzedaży,
	DowódWewnętrzny,
	VatMarża,
	KorektaVatMarży,
	Rachunek,
	KorektaRachunku,
	Zaliczka,
	KorektaZaliczki,
}
