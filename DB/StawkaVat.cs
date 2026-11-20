namespace ProFak.DB;

public class StawkaVat : Rekord<StawkaVat>
{
	public string Skrot { get; set; } = "";
	public decimal Wartosc { get; set; }
	public bool CzyDomyslna { get; set; }
	public KategoriaStawkiVat Kategoria { get; set; } = KategoriaStawkiVat.Krajowa;

	public string CzyDomyslnaFmt => CzyDomyslna ? "Tak" : "Nie";
	public string KategoriaFmt => Opis(Kategoria);
	public bool CzyZW => Kategoria == KategoriaStawkiVat.Zwolniona;
	public bool CzyEX => Kategoria == KategoriaStawkiVat.Eksport;
	public bool CzyNP_I => Kategoria == KategoriaStawkiVat.PozaTerytoriumKraju;
	public bool CzyNP_II => Kategoria == KategoriaStawkiVat.UslugiRozliczaneWUE;
	public bool CzyOO => Kategoria == KategoriaStawkiVat.OdwrotneObciazenie;

	/// <summary>
	/// Kategoria stawki z uwzględnieniem oznaczenia WDT na poziomie faktury. Stawka 0% krajowa
	/// na fakturze oznaczonej jako wewnątrzwspólnotowa dostawa towarów jest traktowana jako WDT.
	/// </summary>
	public KategoriaStawkiVat KategoriaDlaFaktury(bool czyWDT)
		=> Kategoria == KategoriaStawkiVat.Krajowa && Wartosc == 0 && czyWDT
			? KategoriaStawkiVat.DostawaWewnatrzwspolnotowa
			: Kategoria;

	/// <summary>
	/// Opis kategorii wraz z odpowiadającą jej wartością pola P_12 schematu FA(3).
	/// </summary>
	public static string Opis(KategoriaStawkiVat kategoria) => kategoria switch
	{
		KategoriaStawkiVat.Krajowa => "Krajowa (23 / 8 / 5 / 0 KR)",
		KategoriaStawkiVat.Zwolniona => "Zwolniona od podatku (zw)",
		KategoriaStawkiVat.DostawaWewnatrzwspolnotowa => "Wewnątrzwspólnotowa dostawa towarów (0 WDT)",
		KategoriaStawkiVat.Eksport => "Eksport towarów (0 EX)",
		KategoriaStawkiVat.PozaTerytoriumKraju => "Poza terytorium kraju (np I)",
		KategoriaStawkiVat.UslugiRozliczaneWUE => "Usługi art. 100 ust. 1 pkt 4 (np II)",
		KategoriaStawkiVat.OdwrotneObciazenie => "Odwrotne obciążenie (oo)",
		_ => kategoria.ToString()
	};

	public override bool CzyPasuje(string fraza)
		=> base.CzyPasuje(fraza)
		|| CzyPasuje(Skrot, fraza)
		|| CzyPasuje(Wartosc, fraza)
		|| CzyPasuje(KategoriaFmt, fraza)
		|| CzyPasuje(CzyDomyslna ? "Domyślna" : "", fraza);
}

/// <summary>
/// Kategoria stawki podatku decydująca o wartości pola P_12 faktury ustrukturyzowanej FA(3)
/// oraz o polu ewidencji i deklaracji, w którym wykazywana jest sprzedaż.
/// Kolejność wartości jest utrwalona w bazie danych - nowe kategorie należy dopisywać na końcu.
/// </summary>
public enum KategoriaStawkiVat
{
	/// <summary>Sprzedaż krajowa opodatkowana stawką procentową albo stawką 0% (P_12: 23, 8, 5, 0 KR).</summary>
	Krajowa = 0,

	/// <summary>Sprzedaż zwolniona od podatku (P_12: zw).</summary>
	Zwolniona = 1,

	/// <summary>Wewnątrzwspólnotowa dostawa towarów (P_12: 0 WDT).</summary>
	DostawaWewnatrzwspolnotowa = 2,

	/// <summary>Eksport towarów (P_12: 0 EX).</summary>
	Eksport = 3,

	/// <summary>Dostawa towarów i świadczenie usług poza terytorium kraju (P_12: np I).</summary>
	PozaTerytoriumKraju = 4,

	/// <summary>Świadczenie usług, o których mowa w art. 100 ust. 1 pkt 4 ustawy (P_12: np II).</summary>
	UslugiRozliczaneWUE = 5,

	/// <summary>Odwrotne obciążenie w obrocie krajowym (P_12: oo).</summary>
	OdwrotneObciazenie = 6
}
