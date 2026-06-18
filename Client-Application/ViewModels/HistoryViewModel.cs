using System.Collections.ObjectModel;
using System.Windows;
using CurrencyExchangeClient.Services;

namespace CurrencyExchangeClient.ViewModels
{
    /// <summary>
    /// ViewModel for the Historical Rates tab.
    /// </summary>
    public class HistoryViewModel : BaseViewModel
    {
        // ── State ─────────────────────────────────────────────────────────────

        private string _currencyCode = "USD";
        public string CurrencyCode
        {
            get => _currencyCode;
            set => SetField(ref _currencyCode, value);
        }

        private DateTime _startDate = DateTime.Today.AddDays(-30);
        public DateTime StartDate
        {
            get => _startDate;
            set => SetField(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetField(ref _endDate, value);
        }

        private ObservableCollection<ExchangeRate> _rates = new();
        public ObservableCollection<ExchangeRate> Rates
        {
            get => _rates;
            set => SetField(ref _rates, value);
        }

        private string _statusMessage = "Select a currency and date range, then click Load.";
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

        // Summary stats
        private string _summaryText = string.Empty;
        public string SummaryText
        {
            get => _summaryText;
            set => SetField(ref _summaryText, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────

        public AsyncRelayCommand LoadCommand { get; }

        public HistoryViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadHistoryAsync, () => !IsBusy);
        }

        // ── Logic ─────────────────────────────────────────────────────────────

        private async Task LoadHistoryAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrencyCode))
            {
                StatusMessage = "⚠ Enter a currency code.";
                return;
            }

            if (StartDate > EndDate)
            {
                StatusMessage = "⚠ Start date must be before end date.";
                return;
            }

            IsBusy = true;
            StatusMessage = $"Loading {CurrencyCode.ToUpper()} rates from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}...";
            Rates.Clear();
            SummaryText = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    using var client = new ExchangeServiceClient();
                    var rates = client.GetHistoricalRates(
                        CurrencyCode.Trim().ToUpper(), StartDate, EndDate);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var r in rates.OrderByDescending(r => r.EffectiveDate))
                            Rates.Add(r);

                        if (rates.Count > 0)
                        {
                            decimal min = rates.Min(r => r.MidRate);
                            decimal max = rates.Max(r => r.MidRate);
                            decimal avg = rates.Average(r => r.MidRate);
                            SummaryText = $"Min: {min:N4}  |  Max: {max:N4}  |  Avg: {avg:N4}  |  Days: {rates.Count}";
                        }

                        StatusMessage = $"✓ Loaded {rates.Count} records for {CurrencyCode.ToUpper()}";
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
    }
}
