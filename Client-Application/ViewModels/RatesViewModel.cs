using System.Collections.ObjectModel;
using System.Windows;
using CurrencyExchangeClient.Services;

namespace CurrencyExchangeClient.ViewModels
{
    /// <summary>
    /// ViewModel for the Exchange Rates tab.
    /// Loads all NBP rates and supports single-currency lookup.
    /// </summary>
    public class RatesViewModel : BaseViewModel
    {
        // ── State ─────────────────────────────────────────────────────────────

        private ObservableCollection<ExchangeRate> _rates = new();
        public ObservableCollection<ExchangeRate> Rates
        {
            get => _rates;
            set => SetField(ref _rates, value);
        }

        private string _searchCode = string.Empty;
        public string SearchCode
        {
            get => _searchCode;
            set => SetField(ref _searchCode, value);
        }

        private ExchangeRate? _selectedRate;
        public ExchangeRate? SelectedRate
        {
            get => _selectedRate;
            set => SetField(ref _selectedRate, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetField(ref _isBusy, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────

        public AsyncRelayCommand LoadAllRatesCommand { get; }
        public AsyncRelayCommand SearchRateCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        public RatesViewModel()
        {
            LoadAllRatesCommand = new AsyncRelayCommand(LoadAllRatesAsync);
            SearchRateCommand = new AsyncRelayCommand(SearchRateAsync);
        }

        // ── Logic ─────────────────────────────────────────────────────────────

        private async Task LoadAllRatesAsync()
        {
            IsBusy = true;
            StatusMessage = "Fetching all rates from NBP...";
            Rates.Clear();

            await Task.Run(() =>
            {
                try
                {
                    using var client = new ExchangeServiceClient();
                    var rates = client.GetAllRates();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var r in rates.OrderBy(r => r.CurrencyCode))
                            Rates.Add(r);

                        StatusMessage = $"✓ Loaded {rates.Count} currencies — {DateTime.Now:HH:mm:ss}";
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        StatusMessage = $"⚠ Error: {ex.Message}");
                }
            });

            IsBusy = false;
        }

        private async Task SearchRateAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchCode)) return;

            IsBusy = true;
            StatusMessage = $"Searching for {SearchCode.ToUpper()}...";

            await Task.Run(() =>
            {
                try
                {
                    using var client = new ExchangeServiceClient();
                    var rate = client.GetRate(SearchCode.Trim().ToUpper());

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SelectedRate = rate;
                        StatusMessage = $"✓ Found {rate.CurrencyCode} — {rate.EffectiveDate:yyyy-MM-dd}";
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SelectedRate = null;
                        StatusMessage = $"⚠ {ex.Message}";
                    });
                }
            });

            IsBusy = false;
        }
    }
}
