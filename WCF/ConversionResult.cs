using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Runtime.Serialization;
namespace CurrencyExchangeOffice.WcfService
{

    /// <summary>
    /// Immutable record holding the result of a completed currency conversion.
    /// Returned by ExchangeService.BuyCurrency() and SellCurrency().
    /// </summar>
    [DataContract]
    public class ConversionResult
    {
        [DataMember] public string FromCurrency { get; set; }
        [DataMember] public string ToCurrency { get; set; }
        [DataMember] public decimal Amount { get; set; }
        [DataMember] public decimal Rate { get; set; }
        [DataMember] public decimal Converted { get; set; }
        [DataMember] public DateTime FetchedAt { get; set; }
    }
}
