using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;

namespace CurrencyExchangeOffice.WcfService
{
    /// <summary>
    /// Implementation of the IExchangeService WCF contract.
    /// Wraps NbpApiClient and adds exchange office business logic.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,   // new instance per request — thread safe
        ConcurrencyMode = ConcurrencyMode.Single)]
    public class ExchangeService : IExchangeService
    {
        private readonly NbpApiClient _nbp = new();

        // ── Ping ──────────────────────────────────────────────────────────────

        public string Ping()
        {
            return $"Currency Exchange Office Service is running. Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        // ── Single rate ───────────────────────────────────────────────────────

        public ExchangeRate GetRate(string currencyCode)
        {
            ValidateCurrencyCode(currencyCode);

            try
            {
                return _nbp.GetRateAsync(currencyCode).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new FaultException($"Failed to retrieve rate for '{currencyCode}': {ex.Message}");
            }
        }

        // ── Multiple rates ────────────────────────────────────────────────────

        public List<ExchangeRate> GetRates(List<string> currencyCodes)
        {
            if (currencyCodes == null || currencyCodes.Count == 0)
                throw new FaultException("Currency code list cannot be empty.");

            var results = new List<ExchangeRate>();

            foreach (var code in currencyCodes)
            {
                try
                {
                    ValidateCurrencyCode(code);
                    var rate = _nbp.GetRateAsync(code).GetAwaiter().GetResult();
                    results.Add(rate);
                }
                catch (Exception ex)
                {
                    // Skip invalid codes, log and continue
                    Console.WriteLine($"[WARN] Could not fetch rate for '{code}': {ex.Message}");
                }
            }

            return results;
        }

        // ── All rates ─────────────────────────────────────────────────────────

        public List<ExchangeRate> GetAllRates()
        {
            try
            {
                return _nbp.GetAllRatesAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new FaultException($"Failed to retrieve all rates: {ex.Message}");
            }
        }

        // ── Historical rates ──────────────────────────────────────────────────

        public List<ExchangeRate> GetHistoricalRates(
            string currencyCode, DateTime startDate, DateTime endDate)
        {
            ValidateCurrencyCode(currencyCode);

            if (startDate > endDate)
                throw new FaultException("Start date must be before end date.");

            if (endDate > DateTime.Today)
                throw new FaultException("End date cannot be in the future.");

            if ((endDate - startDate).TotalDays > 367)
                throw new FaultException("NBP API only provides data for the last 367 days.");

            try
            {
                return _nbp.GetHistoricalRatesAsync(currencyCode, startDate, endDate)
                           .GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new FaultException(
                    $"Failed to retrieve historical rates for '{currencyCode}': {ex.Message}");
            }
        }

        // ── Buy currency (client spends PLN, receives foreign currency) ────────

        public ExchangeResult BuyCurrency(string currencyCode, decimal amountPln)
        {
            ValidateCurrencyCode(currencyCode);

            if (amountPln <= 0)
                throw new FaultException("Amount must be greater than zero.");

            try
            {
                var rate = _nbp.GetRateAsync(currencyCode).GetAwaiter().GetResult();

                // Client is BUYING foreign currency → we use SellRate (office sells at higher price)
                decimal received = Math.Round(amountPln / rate.SellRate, 2);

                return new ExchangeResult
                {
                    Success = true,
                    Message = $"Successfully bought {received} {currencyCode} for {amountPln} PLN.",
                    FromCurrency = "PLN",
                    ToCurrency = currencyCode,
                    AmountSent = amountPln,
                    AmountReceived = received,
                    RateUsed = rate.SellRate,
                    Timestamp = DateTime.Now
                };
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ExchangeResult
                {
                    Success = false,
                    Message = $"Exchange failed: {ex.Message}",
                    Timestamp = DateTime.Now
                };
            }
        }

        // ── Sell currency (client spends foreign currency, receives PLN) ───────

        public ExchangeResult SellCurrency(string currencyCode, decimal amountForeign)
        {
            ValidateCurrencyCode(currencyCode);

            if (amountForeign <= 0)
                throw new FaultException("Amount must be greater than zero.");

            try
            {
                var rate = _nbp.GetRateAsync(currencyCode).GetAwaiter().GetResult();

                // Client is SELLING foreign currency → we use BuyRate (office buys at lower price)
                decimal received = Math.Round(amountForeign * rate.BuyRate, 2);

                return new ExchangeResult
                {
                    Success = true,
                    Message = $"Successfully sold {amountForeign} {currencyCode} for {received} PLN.",
                    FromCurrency = currencyCode,
                    ToCurrency = "PLN",
                    AmountSent = amountForeign,
                    AmountReceived = received,
                    RateUsed = rate.BuyRate,
                    Timestamp = DateTime.Now
                };
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ExchangeResult
                {
                    Success = false,
                    Message = $"Exchange failed: {ex.Message}",
                    Timestamp = DateTime.Now
                };
            }
        }

        // ── Validation helper ─────────────────────────────────────────────────

        private static void ValidateCurrencyCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new FaultException("Currency code cannot be empty.");

            if (code.Length != 3 || !code.All(char.IsLetter))
                throw new FaultException($"'{code}' is not a valid ISO 4217 currency code (must be 3 letters, e.g. USD).");
        }
    }
}
