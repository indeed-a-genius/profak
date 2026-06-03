namespace ProFak.DB;

public class Kontrahent : Rekord<Kontrahent>
{
	public string Nazwa { get; set; } = "";
	public string PelnaNazwa { get; set; } = "";
	public string NIP { get; set; } = "";
	public string AdresRejestrowy { get; set; } = "";
	public string AdresKorespondencyjny { get; set; } = "";
	public string RachunekBankowy { get; set; } = "";
	public string NazwaBanku { get; set; } = "";
	/// <summary>
	/// Dodatkowe rachunki bankowe w formacie JSON: [{"Rachunek":"...","NazwaBanku":"...","WalutaSkrot":"..."}]
	/// </summary>
	public string DodatkoweRachunki { get; set; } = "";
	public string Telefon { get; set; } = "";
	public string EMail { get; set; } = "";
	public string UwagiWewnetrzne { get; set; } = "";
	public string UwagiPubliczne { get; set; } = "";
	public bool CzyArchiwalny { get; set; }
	public bool CzyPodmiot { get; set; }
	public bool CzyTP { get; set; }
	public bool CzyImportKSeF { get; set; }
	/// <summary>Czy kontrahent jest z zagranicy (brak polskiego NIP)</summary>
	public bool CzyZagraniczny { get; set; }
	/// <summary>Kraj kontrahenta zagranicznego, np. "DE", "FR"</summary>
	public string KrajKontrahenta { get; set; } = "";
	/// <summary>Zagraniczny numer VAT, np. DE123456789</summary>
	public string ZagranicznyNumerVAT { get; set; } = "";
	public int? SposobPlatnosciId { get; set; }
	public int? DomyslnaWalutaId { get; set; }

	public string KodUrzedu { get; set; } = "";
	public string OsobaFizycznaImie { get; set; } = "";
	public string OsobaFizycznaNazwisko { get; set; } = "";
	public DateTime? OsobaFizycznaDataUrodzenia { get; set; }
	public FormaOpodatkowania? FormaOpodatkowania { get; set; }
	public string TokenKSeF { get; set; } = "";
	public SrodowiskoKSeF SrodowiskoKSeF { get; set; }

	public Ref<SposobPlatnosci> SposobPlatnosciRef { get => SposobPlatnosciId; set => SposobPlatnosciId = value; }
	public Ref<Waluta> DomyslnaWalutaRef { get => DomyslnaWalutaId; set => DomyslnaWalutaId = value; }

	public SposobPlatnosci? SposobPlatnosci { get; set; }
	public Waluta? DomyslnaWaluta { get; set; }

	public string AdresRejestrowyFmt => AdresRejestrowy.JakoJednaLinia();
	public string PelnaNazwaLubNazwa => String.IsNullOrEmpty(PelnaNazwa) ? Nazwa : PelnaNazwa;

	/// <summary>
	/// Zwraca numer rachunku bankowego odpowiedni dla podanego skrótu waluty.
	/// Najpierw szuka wśród dodatkowych rachunków powiązanych z daną walutą,
	/// potem domyślny rachunek.
	/// </summary>
	public string RachunekBankowyDlaWaluty(string? walutaSkrot)
	{
		if (!String.IsNullOrEmpty(walutaSkrot) && !String.IsNullOrEmpty(DodatkoweRachunki))
		{
			try
			{
				var rachunki = System.Text.Json.JsonSerializer.Deserialize<List<DodatkowyRachunekBankowy>>(DodatkoweRachunki);
				var pasujacy = rachunki?.FirstOrDefault(r => String.Equals(r.WalutaSkrot, walutaSkrot, StringComparison.OrdinalIgnoreCase));
				if (pasujacy != null && !String.IsNullOrEmpty(pasujacy.Rachunek))
					return pasujacy.Rachunek;
			}
			catch { }
		}
		return RachunekBankowy;
	}

	/// <summary>
	/// Zwraca nazwę banku dla podanego skrótu waluty.
	/// </summary>
	public string NazwaBankuDlaWaluty(string? walutaSkrot)
	{
		if (!String.IsNullOrEmpty(walutaSkrot) && !String.IsNullOrEmpty(DodatkoweRachunki))
		{
			try
			{
				var rachunki = System.Text.Json.JsonSerializer.Deserialize<List<DodatkowyRachunekBankowy>>(DodatkoweRachunki);
				var pasujacy = rachunki?.FirstOrDefault(r => String.Equals(r.WalutaSkrot, walutaSkrot, StringComparison.OrdinalIgnoreCase));
				if (pasujacy != null && !String.IsNullOrEmpty(pasujacy.NazwaBanku))
					return pasujacy.NazwaBanku;
			}
			catch { }
		}
		return NazwaBanku;
	}

	public override bool CzyPasuje(string fraza)
		=> base.CzyPasuje(fraza)
		|| CzyPasuje(Nazwa, fraza)
		|| CzyPasuje(PelnaNazwa, fraza)
		|| CzyPasuje(NIP, fraza)
		|| CzyPasuje(ZagranicznyNumerVAT, fraza)
		|| CzyPasuje(KrajKontrahenta, fraza)
		|| CzyPasuje(AdresRejestrowy, fraza)
		|| CzyPasuje(AdresKorespondencyjny, fraza)
		|| CzyPasuje(RachunekBankowy, fraza)
		|| CzyPasuje(Telefon, fraza)
		|| CzyPasuje(EMail, fraza)
		|| CzyPasuje(UwagiWewnetrzne, fraza)
		|| CzyPasuje(CzyArchiwalny ? "Archiwalny" : "", fraza)
		|| CzyPasuje(CzyImportKSeF ? "KSeF" : "", fraza)
		|| CzyPasuje(CzyPodmiot ? "Podmiot" : "", fraza)
		|| CzyPasuje(CzyZagraniczny ? "Zagraniczny" : "", fraza);
}

/// <summary>Pomocnicza klasa do serializacji dodatkowych rachunków bankowych.</summary>
public class DodatkowyRachunekBankowy
{
	public string Rachunek { get; set; } = "";
	public string NazwaBanku { get; set; } = "";
	/// <summary>Skrót waluty powiązanej z rachunkiem, np. "EUR", "USD". Puste = dla wszystkich.</summary>
	public string WalutaSkrot { get; set; } = "";
}

public enum FormaOpodatkowania
{
	Liniowy,
	Skala,
	Ryczałt,
	DziałalnośćNierejestrowana
}

public enum SrodowiskoKSeF
{
	Test,
	Demo,
	Prod
}
