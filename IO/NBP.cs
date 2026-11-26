using ProFak.DB;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace ProFak.IO;

public class NBP
{
	public static async Task<decimal> PobierzSredniKursWaluty(Waluta? waluta, DateTime data, CancellationToken cancellationToken)
		=> (await PobierzSredniKursWalutyZData(waluta, data, cancellationToken)).Kurs;

	/// <summary>
	/// Zwraca kurs średni waluty z ostatniej tabeli A NBP opublikowanej w dniu <paramref name="data"/> lub wcześniej,
	/// razem z datą tej tabeli. Dni bez tabeli (weekendy i święta) są pomijane, czyli dla daty sprzedaży
	/// pomniejszonej o jeden dzień wychodzi kurs z ostatniego dnia roboczego przed dniem sprzedaży (art. 31a ust. 1).
	/// </summary>
	public static async Task<(decimal Kurs, DateTime Data)> PobierzSredniKursWalutyZData(Waluta? waluta, DateTime data, CancellationToken cancellationToken)
	{
		data = data.Date;
		if (waluta == null || waluta.CzyDomyslna) return (1, data);
		if (data > DateTime.Today) throw new ApplicationException($"Kurs NBP z dnia {data:d} nie jest jeszcze dostępny.");

		// 10 dni wstecz obejmuje każdy weekend połączony ze świętami (np. 24-26 grudnia).
		var url = $"https://api.nbp.pl/api/exchangerates/rates/a/{waluta.Skrot}/{data.AddDays(-10):yyyy-MM-dd}/{data:yyyy-MM-dd}/";
		using var client = new HttpClient();
		string wynik;
		try
		{
			wynik = await client.GetStringAsync(url, cancellationToken);
		}
		catch (HttpRequestException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
		{
			throw new ApplicationException($"Brak kursu NBP waluty {waluta.Skrot} z dnia {data:d} ani z 10 dni wcześniej.", exc);
		}

		using var json = JsonDocument.Parse(wynik);
		var rates = json.RootElement.GetProperty("rates");
		if (rates.GetArrayLength() == 0) throw new ApplicationException($"Brak kursu dla waluty {waluta}.");
		// Tabele są zwracane chronologicznie - ostatnia jest najbliższa żądanej dacie.
		var rate = rates[rates.GetArrayLength() - 1];
		if (!rate.TryGetProperty("mid", out var mid)) throw new ApplicationException($"Nieprawidłowa struktura odpowiedzi dla waluty {waluta}.");
		var dataTabeli = rate.TryGetProperty("effectiveDate", out var effectiveDate)
			&& DateTime.TryParseExact(effectiveDate.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataEfektywna)
			? dataEfektywna : data;

		// Dzisiejsza tabela jest publikowana w dni robocze około południa. Wcześniej zakres zwróciłby
		// po cichu wczorajszy kurs, który dla dzisiejszej daty byłby nieprawidłowy.
		if (data == DateTime.Today && dataTabeli < data && data.DayOfWeek != DayOfWeek.Saturday && data.DayOfWeek != DayOfWeek.Sunday)
			throw new ApplicationException($"Tabela kursów NBP z dnia {data:d} nie została jeszcze opublikowana (zwykle ok. 12:00). Jeśli dziś jest święto, wpisz kurs z {dataTabeli:d} ręcznie.");

		return (mid.GetDecimal(), dataTabeli);
	}
}
