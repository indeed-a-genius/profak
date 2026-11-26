using Microsoft.EntityFrameworkCore;

namespace ProFak.DB;

public class DeklaracjaVat : Rekord<DeklaracjaVat>
{
	public DateTime Miesiac { get; set; } = DateTime.Now.Date.AddDays(1 - DateTime.Now.Day).AddMonths(-1);

	public decimal NettoZW { get; set; }
	public decimal Netto0 { get; set; }
	public decimal Netto5 { get; set; }
	public decimal Netto8 { get; set; }
	public decimal Netto23 { get; set; }
	public decimal NettoWDT { get; set; }
	public decimal NettoWNT { get; set; }
	public decimal NettoEksport { get; set; }
	public decimal NettoPozaKrajem { get; set; }
	public decimal NettoUslugiUE { get; set; }
	public decimal NettoOdwrotneObciazenie { get; set; }

	public decimal Nalezny5 { get; set; }
	public decimal Nalezny8 { get; set; }
	public decimal Nalezny23 { get; set; }
	public decimal NaleznyWNT { get; set; }

	public decimal NettoSrodkiTrwale { get; set; }
	public decimal NettoPozostale { get; set; }

	public decimal NaliczonyPrzeniesiony { get; set; }
	public decimal NaliczonySrodkiTrwale { get; set; }
	public decimal NaliczonyPozostale { get; set; }

	public string MiesiacFmt => Miesiac.ToString("MM/yyyy");

	// NettoUslugiUE zawiera się w NettoPozaKrajem (K_12 jest podzbiorem K_11), więc nie jest tu dodawane.
	// NettoOdwrotneObciazenie nie ma odpowiednika po stronie sprzedawcy w JPK_V7M - patrz komentarz w Przelicz.
	public decimal NettoRazem => NettoZW + Netto0 + Netto5 + Netto8 + Netto23 + NettoWDT + NettoWNT + NettoEksport + NettoPozaKrajem;
	public decimal NaleznyRazem => Nalezny5 + Nalezny8 + Nalezny23 + NaleznyWNT;
	public decimal NaliczonyRazem => NaliczonyPrzeniesiony + NaliczonySrodkiTrwale + NaliczonyPozostale;

	public decimal DoWplaty => Math.Max(NaleznyRazem - NaliczonyRazem, 0);
	public decimal DoPrzeniesienia => Math.Max(NaliczonyRazem - NaleznyRazem, 0);

	public List<Faktura> Faktury { get; set; } = default!;

	public override bool CzyPasuje(string fraza)
		=> base.CzyPasuje(fraza)
		|| CzyPasuje(Miesiac, fraza)
		|| CzyPasuje(NettoZW, fraza)
		|| CzyPasuje(Netto0, fraza)
		|| CzyPasuje(Netto5, fraza)
		|| CzyPasuje(Netto8, fraza)
		|| CzyPasuje(Netto23, fraza)
		|| CzyPasuje(NettoWDT, fraza)
		|| CzyPasuje(NettoWNT, fraza)
		|| CzyPasuje(NettoEksport, fraza)
		|| CzyPasuje(NettoPozaKrajem, fraza)
		|| CzyPasuje(NettoUslugiUE, fraza)
		|| CzyPasuje(NettoOdwrotneObciazenie, fraza)
		|| CzyPasuje(Nalezny5, fraza)
		|| CzyPasuje(Nalezny8, fraza)
		|| CzyPasuje(Nalezny23, fraza)
		|| CzyPasuje(NaleznyWNT, fraza)
		|| CzyPasuje(NettoSrodkiTrwale, fraza)
		|| CzyPasuje(NettoPozostale, fraza)
		|| CzyPasuje(NaliczonyPrzeniesiony, fraza)
		|| CzyPasuje(NaliczonySrodkiTrwale, fraza)
		|| CzyPasuje(NaliczonyPozostale, fraza)
		|| CzyPasuje(NettoRazem, fraza)
		|| CzyPasuje(NaleznyRazem, fraza)
		|| CzyPasuje(NaliczonyRazem, fraza)
		|| CzyPasuje(DoWplaty, fraza)
		|| CzyPasuje(DoPrzeniesienia, fraza);

	public void WybierzFaktury(Baza baza)
	{
		var nieaktualneFaktury = baza.Faktury.Where(faktura => faktura.DeklaracjaVatId == Id).ToDictionary(faktura => faktura.Ref);
		var zmienioneFaktury = new List<Faktura>();
		var dataGraniczna = Miesiac.Date.AddMonths(1);

		var faktury = baza.Faktury
			.Where(faktura => (faktura.DataSprzedazy < dataGraniczna || faktura.DataWystawienia < dataGraniczna)
				&& faktura.Rodzaj != RodzajFaktury.Usunięta
				&& faktura.Rodzaj != RodzajFaktury.Proforma
				&& (faktura.DeklaracjaVatId == null || faktura.DeklaracjaVatId == Id))
			.ToList()
			.Where(faktura => (faktura.CzySprzedaz && faktura.DataSprzedazy < dataGraniczna) || (faktura.CzyZakup && faktura.DataWystawienia < dataGraniczna))
			.ToList();

		foreach (var faktura in faktury)
		{
			if (!nieaktualneFaktury.Remove(faktura))
			{
				faktura.DeklaracjaVatRef = this;
				zmienioneFaktury.Add(faktura);
			}
		}

		foreach (var faktura in nieaktualneFaktury.Values)
		{
			faktura.DeklaracjaVatRef = default;
			zmienioneFaktury.Add(faktura);
		}

		baza.Zapisz(zmienioneFaktury);
	}

	public void Przelicz(Baza baza)
	{
		NettoZW = 0;
		Netto0 = 0;
		Netto5 = 0;
		Netto8 = 0;
		Netto23 = 0;
		NettoWDT = 0;
		NettoWNT = 0;
		NettoEksport = 0;
		NettoPozaKrajem = 0;
		NettoUslugiUE = 0;
		NettoOdwrotneObciazenie = 0;

		Nalezny5 = 0;
		Nalezny8 = 0;
		Nalezny23 = 0;
		NaleznyWNT = 0;

		NettoSrodkiTrwale = 0;
		NettoPozostale = 0;

		NaliczonyPrzeniesiony = 0;
		NaliczonySrodkiTrwale = 0;
		NaliczonyPozostale = 0;

		var poprzedniaDeklaracja = baza.DeklaracjeVat
			.Where(deklaracja => deklaracja.Miesiac < Miesiac)
			.OrderByDescending(deklaracja => deklaracja.Miesiac)
			.FirstOrDefault();

		if (poprzedniaDeklaracja != null) NaliczonyPrzeniesiony = poprzedniaDeklaracja.DoPrzeniesienia;

		var faktury = baza.Faktury
			.Where(faktura => faktura.DeklaracjaVatId == Id)
			.Include(faktura => faktura.Pozycje).ThenInclude(pozycja => pozycja.StawkaVat)
			.ToList();

		foreach (var faktura in faktury)
		{
			if (faktura.CzySprzedaz)
			{
				var sumaPozycjiBrutto = faktura.Pozycje.Where(e => !e.CzyPrzedKorekta).Sum(e => e.WartoscBrutto);
				var ulamekZaliczki = (faktura.CzyZaliczka || faktura.CzyRozliczenie) && sumaPozycjiBrutto != 0 ? (faktura.RazemBrutto + faktura.KwotaZaliczkiPrzedKorekta) / sumaPozycjiBrutto : 1;
				foreach (var pozycja in faktura.Pozycje)
				{
					if (pozycja.StawkaVat == null) continue;
					var netto = pozycja.WartoscNetto * faktura.KursWaluty * ulamekZaliczki;
					var vat = pozycja.WartoscVat * faktura.KursWaluty * ulamekZaliczki;
					switch (pozycja.StawkaVat.KategoriaDlaFaktury(faktura.CzyWDT))
					{
						case KategoriaStawkiVat.Zwolniona: NettoZW += netto; break;
						case KategoriaStawkiVat.DostawaWewnatrzwspolnotowa: NettoWDT += netto; break;
						case KategoriaStawkiVat.Eksport: NettoEksport += netto; break;
						case KategoriaStawkiVat.PozaTerytoriumKraju: NettoPozaKrajem += netto; break;
						// Usługi z art. 100 ust. 1 pkt 4 wykazuje się zarówno w K_11/P_11, jak i w K_12/P_12
						case KategoriaStawkiVat.UslugiRozliczaneWUE: NettoPozaKrajem += netto; NettoUslugiUE += netto; break;
						// Odwrotne obciążenie: po uchyleniu art. 17 ust. 1 pkt 7 i 8 (XI 2019) JPK_V7M nie ma już
						// pola sprzedawcy dla takich dostaw, a K_31/K_32 (P_31/P_32) służą rozliczeniu podatku
						// należnego przez nabywcę przy dostawach z art. 17 ust. 1 pkt 5. Kwota jest więc tylko
						// ewidencjonowana informacyjnie i nie wchodzi do podstawy ani do generowanego JPK_V7M.
						case KategoriaStawkiVat.OdwrotneObciazenie: NettoOdwrotneObciazenie += netto; break;
						default:
							if (pozycja.StawkaVat.Wartosc == 0) { Netto0 += netto; }
							else if (pozycja.StawkaVat.Wartosc <= 5) { Netto5 += netto; Nalezny5 += vat; }
							else if (pozycja.StawkaVat.Wartosc <= 8) { Netto8 += netto; Nalezny8 += vat; }
							else { Netto23 += netto; Nalezny23 += vat; }
							break;
					}
				}
			}
			else if (faktura.CzyZakup)
			{
				if (faktura.CzyWNT) { NettoWNT += faktura.RazemNetto * faktura.KursWaluty; NaleznyWNT += faktura.VatNaliczony * faktura.KursWaluty; }
				/* bez else */
				if (faktura.CzyZakupSrodkowTrwalych) { NettoSrodkiTrwale += faktura.RazemNetto * faktura.KursWaluty; NaliczonySrodkiTrwale += faktura.VatNaliczony * faktura.KursWaluty; }
				else { NettoPozostale += faktura.RazemNetto * faktura.KursWaluty; NaliczonyPozostale += faktura.VatNaliczony * faktura.KursWaluty; }
			}
		}

		NettoZW = NettoZW.Zaokragl(miejsca: 0);
		Netto0 = Netto0.Zaokragl(miejsca: 0);
		Netto5 = Netto5.Zaokragl(miejsca: 0);
		Netto8 = Netto8.Zaokragl(miejsca: 0);
		Netto23 = Netto23.Zaokragl(miejsca: 0);
		NettoWDT = NettoWDT.Zaokragl(miejsca: 0);
		NettoWNT = NettoWNT.Zaokragl(miejsca: 0);
		NettoEksport = NettoEksport.Zaokragl(miejsca: 0);
		NettoPozaKrajem = NettoPozaKrajem.Zaokragl(miejsca: 0);
		NettoUslugiUE = NettoUslugiUE.Zaokragl(miejsca: 0);
		NettoOdwrotneObciazenie = NettoOdwrotneObciazenie.Zaokragl(miejsca: 0);

		Nalezny5 = Nalezny5.Zaokragl(miejsca: 0);
		Nalezny8 = Nalezny8.Zaokragl(miejsca: 0);
		Nalezny23 = Nalezny23.Zaokragl(miejsca: 0);
		NaleznyWNT = NaleznyWNT.Zaokragl(miejsca: 0);

		NettoSrodkiTrwale = NettoSrodkiTrwale.Zaokragl(miejsca: 0);
		NettoPozostale = NettoPozostale.Zaokragl(miejsca: 0);

		NaliczonyPrzeniesiony = NaliczonyPrzeniesiony.Zaokragl(miejsca: 0);
		NaliczonySrodkiTrwale = NaliczonySrodkiTrwale.Zaokragl(miejsca: 0);
		NaliczonyPozostale = NaliczonyPozostale.Zaokragl(miejsca: 0);
	}
}
