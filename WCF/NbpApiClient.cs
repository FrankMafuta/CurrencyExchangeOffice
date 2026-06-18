using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CurrencyExchangeOffice.WcfService
{
    // ── NBP API RESPONSE MODELS ───────────────────────────────────────────────

    internal class NbpTableResponse
    {
        [JsonProperty("table")]
        public string Table { get; set; }

        [JsonProperty("no")]
        public string No { get; set; }

        [JsonProperty("effectiveDate")]
        public string EffectiveDate { get; set; }

        [JsonProperty("rates")]
        public List<NbpRate> Rates { get; set; }
    }

    internal class NbpRate
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("mid")]
        public decimal Mid { get; set; }
    }

    // ── NBP API CLIENT ────────────────────────────────────────────────────────

    internal class NbpApiClient
    {
        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.nbp.pl/api/"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        private const decimal SpreadPercent = 0.02m;

        // ── SINGLE RATE ───────────────────────────────────────────────────────

        public async Task<ExchangeRate> GetRateAsync(string currencyCode)
        {
            string url = $"exchangerates/rates/A/{currencyCode.ToUpper()}/?format=json";

            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"NBP API error for {currencyCode}");

            string json = await response.Content.ReadAsStringAsync();

            dynamic root = JsonConvert.DeserializeObject(json);

            string name = root.currency;
            string code = root.code;
            var rate = root.rates[0];

            decimal mid = (decimal)rate.mid;
            string dateStr = rate.effectiveDate;

            return BuildExchangeRate(code, name, mid, dateStr);
        }

        // ── ALL RATES ─────────────────────────────────────────────────────────

        public async Task<List<ExchangeRate>> GetAllRatesAsync()
        {
            string url = "exchangerates/tables/A/?format=json";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var tables = JsonConvert.DeserializeObject<List<NbpTableResponse>>(json);

            var table = tables[0];
            string dateStr = table.EffectiveDate;

            return table.Rates
                .Select(r => BuildExchangeRate(r.Code, r.Currency, r.Mid, dateStr))
                .ToList();
        }

        // ── HISTORICAL ───────────────────────────────────────────────────────

        public async Task<List<ExchangeRate>> GetHistoricalRatesAsync(
            string currencyCode, DateTime startDate, DateTime endDate)
        {
            string url =
                $"exchangerates/rates/A/{currencyCode.ToUpper()}/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}/?format=json";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            dynamic root = JsonConvert.DeserializeObject(json);

            string name = root.currency;
            string code = root.code;

            var result = new List<ExchangeRate>();

            foreach (var rate in root.rates)
            {
                decimal mid = (decimal)rate.mid;
                string dateStr = rate.effectiveDate;

                result.Add(BuildExchangeRate(code, name, mid, dateStr));
            }

            return result;
        }

        // ── HELPER ────────────────────────────────────────────────────────────

        private static ExchangeRate BuildExchangeRate(
            string code, string name, decimal mid, string dateStr)
        {
            DateTime.TryParse(dateStr, out DateTime date);

            return new ExchangeRate
            {
                CurrencyCode = code,
                CurrencyName = name,
                MidRate = mid,
                BuyRate = Math.Round(mid * 0.98m, 4),
                SellRate = Math.Round(mid * 1.02m, 4),
                EffectiveDate = date
            };
        }
    }
}