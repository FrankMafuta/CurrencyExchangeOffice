using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using CurrencyExchangeOffice.WcfService;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;

namespace CurrencyExchangeOffice.WcfService
{
    /// <summary>
    /// Self-hosted WCF service entry point.
    /// Run this console app to start the service, then connect from the WPF client.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Currency Exchange Office — WCF Service Host";

            Uri baseAddress = new Uri("http://localhost:8080/ExchangeService");

            using (ServiceHost host = new ServiceHost(typeof(ExchangeService), baseAddress))
            {
                try
                {
                    // Open the service host (reads App.config for endpoints)
                    host.Open();

                    PrintBanner(baseAddress);

                    Console.WriteLine("Press [ENTER] to stop the service...");
                    Console.ReadLine();

                    host.Close();
                    Console.WriteLine("Service stopped.");
                }
                catch (AddressAccessDeniedException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine("ERROR: Access denied when opening the HTTP port.");
                    Console.WriteLine("Run Visual Studio (or this executable) as Administrator,");
                    Console.WriteLine("or register the URL namespace with:");
                    Console.WriteLine($"  netsh http add urlacl url={baseAddress}/ user=Everyone");
                    Console.ResetColor();
                    Console.ReadLine();
                }
                catch (CommunicationException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.ResetColor();
                    host.Abort();
                    Console.ReadLine();
                }
            }
        }

        private static void PrintBanner(Uri baseAddress)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            Console.WriteLine("║   Currency Exchange Office — WCF Service Host    ║");
            Console.WriteLine("╠══════════════════════════════════════════════════╣");
            Console.WriteLine($"║  Endpoint : {baseAddress,-38}║");
            Console.WriteLine($"║  WSDL     : {baseAddress}?wsdl     ║");
            Console.WriteLine($"║  MEX      : {baseAddress}/mex      ║");
            Console.WriteLine("╠══════════════════════════════════════════════════╣");
            Console.WriteLine("║  Operations available:                           ║");
            Console.WriteLine("║    Ping()                                        ║");
            Console.WriteLine("║    GetRate(currencyCode)                         ║");
            Console.WriteLine("║    GetRates(currencyCodes)                       ║");
            Console.WriteLine("║    GetAllRates()                                 ║");
            Console.WriteLine("║    GetHistoricalRates(code, startDate, endDate)  ║");
            Console.WriteLine("║    BuyCurrency(currencyCode, amountPln)          ║");
            Console.WriteLine("║    SellCurrency(currencyCode, amountForeign)     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Service is running. Data source: api.nbp.pl");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
