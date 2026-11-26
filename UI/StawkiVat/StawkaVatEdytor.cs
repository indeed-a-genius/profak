using ProFak.DB;

namespace ProFak.UI;

class StawkaVatEdytor : EdytorDwieKolumny<StawkaVat>
{
	private readonly TNumericUpDown numericUpDownWartosc;

	public StawkaVatEdytor()
	{
		DodajTextBox(stawkaVat => stawkaVat.Skrot, "Skrót", wymagane: true);

		var comboBoxKategoria = Kontrolki.DropDownList();
		DodajWiersz(comboBoxKategoria, "Kategoria");
		kontroler.Slownik(comboBoxKategoria, Enum.GetValues<KategoriaStawkiVat>()
			.Select(kategoria => new PozycjaListy<KategoriaStawkiVat?> { Wartosc = kategoria, Opis = StawkaVat.Opis(kategoria) })
			.ToArray());
		kontroler.Powiazanie(comboBoxKategoria, stawkaVat => stawkaVat.Kategoria, KategoriaZmieniona);

		numericUpDownWartosc = DodajNumericUpDown(stawkaVat => stawkaVat.Wartosc, "Wartość");
		DodajCheckBox(stawkaVat => stawkaVat.CzyDomyslna, "Domyślna");
		UstawRozmiar();
	}

	protected override void RekordGotowy()
	{
		base.RekordGotowy();
		KonfigurujWartosc();
	}

	private void KategoriaZmieniona()
	{
		// Poza sprzedażą krajową stawka podatku zawsze wynosi 0%
		if (Rekord.Kategoria != KategoriaStawkiVat.Krajowa && Rekord.Wartosc != 0)
		{
			Rekord.Wartosc = 0;
			kontroler.AktualizujKontrolki();
		}
		KonfigurujWartosc();
	}

	private void KonfigurujWartosc()
	{
		numericUpDownWartosc.Enabled = Rekord.Kategoria == KategoriaStawkiVat.Krajowa;
	}
}
