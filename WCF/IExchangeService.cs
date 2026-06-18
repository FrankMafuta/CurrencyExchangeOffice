using System.Runtime.Serialization;
using System.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
namespace CurrencyExchangeOffice.WcfService
{
    // ── DATA CONTRACTS ────────────────────────────────────────────────────────

    /// <summary>
    /// Represents a single exchange rate returned from the NBP API.
    /// </summary>
    [DataContract]
    public class ExchangeRate
    {
        [DataMember] public string CurrencyCode { get; set; }   // e.g. "USD"
        [DataMember] public string CurrencyName { get; set; }   // e.g. "dolar amerykański"
        [DataMember] public decimal MidRate { get; set; }        // mid rate from NBP table A
        [DataMember] public decimal BuyRate { get; set; }        // MidRate * (1 - spread)
        [DataMember] public decimal SellRate { get; set; }       // MidRate * (1 + spread)
        [DataMember] public DateTime EffectiveDate { get; set; } // date the rate applies
    }

    /// <summary>
    /// Result returned after a currency exchange operation.
    /// </summary>
    [DataContract]
    public class ExchangeResult
    {
        [DataMember] public bool Success { get; set; }
        [DataMember] public string Message { get; set; }
        [DataMember] public string FromCurrency { get; set; }
        [DataMember] public string ToCurrency { get; set; }
        [DataMember] public decimal AmountSent { get; set; }
        [DataMember] public decimal AmountReceived { get; set; }
        [DataMember] public decimal RateUsed { get; set; }
        [DataMember] public DateTime Timestamp { get; set; }
    }

    // ── SERVICE CONTRACT ──────────────────────────────────────────────────────

    /// <summary>
    /// WCF service contract for the currency exchange office.
    /// All methods are accessible without authorization (as required).
    /// </summary>
    [ServiceContract]
    public interface IExchangeService
    {
        /// <summary>
        /// Returns the current exchange rate for a single currency from the NBP API.
        /// Lab 2–4 core requirement.
        /// </summary>
        /// <param name="currencyCode">ISO 4217 code, e.g. "USD", "EUR", "GBP"</param>
        [OperationContract]
        ExchangeRate GetRate(string currencyCode);

        /// <summary>
        /// Returns exchange rates for a list of currencies in one call.
        /// </summary>
        [OperationContract]
        List<ExchangeRate> GetRates(List<string> currencyCodes);

        /// <summary>
        /// Returns exchange rates for all currencies in NBP Table A.
        /// </summary>
        [OperationContract]
        List<ExchangeRate> GetAllRates();

        /// <summary>
        /// Returns historical rates for a currency between two dates.
        /// </summary>
        /// <param name="currencyCode">ISO 4217 code</param>
        /// <param name="startDate">Start date (yyyy-MM-dd)</param>
        /// <param name="endDate">End date (yyyy-MM-dd)</param>
        [OperationContract]
        List<ExchangeRate> GetHistoricalRates(string currencyCode, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Simulates buying a foreign currency using PLN.
        /// Uses the service's sell rate (you pay more when buying foreign currency).
        /// </summary>
        /// <param name="currencyCode">Currency to buy, e.g. "USD"</param>
        /// <param name="amountPln">Amount in PLN to spend</param>
        [OperationContract]
        ExchangeResult BuyCurrency(string currencyCode, decimal amountPln);

        /// <summary>
        /// Simulates selling a foreign currency to receive PLN.
        /// Uses the service's buy rate (you receive less when selling foreign currency).
        /// </summary>
        /// <param name="currencyCode">Currency to sell, e.g. "USD"</param>
        /// <param name="amountForeign">Amount of foreign currency to sell</param>
        [OperationContract]
        ExchangeResult SellCurrency(string currencyCode, decimal amountForeign);

        /// <summary>
        /// Health check — confirms the service is running.
        /// </summary>
        [OperationContract]
        string Ping();
    }
}
