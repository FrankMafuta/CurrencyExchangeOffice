using System.Collections.ObjectModel;
using System.Windows;
using CurrencyExchangeClient.Services;

namespace CurrencyExchangeClient.ViewModels
{
    /// <summary>
    /// ViewModel for the Buy / Sell tab.
    /// </summary>
    public class ExchangeViewModel : BaseViewModel
    {
        // ── State ─────────────────────────────────────────────────────────────

        private string _currencyCode = "USD";
        public string CurrencyCode
        {
            get => _currencyCode;
            set => SetField(ref _currencyCode, value);
        }

        private decimal _amount = 100m;
        public decimal Amount
        {
            get => _amount;
            set => SetField(ref _amount, value);
        }

        private string _statusMessage = "Enter a currency code and amount, then click Buy or Sell.";
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

        // Result panel
        private ExchangeResult? _lastResult;
        public ExchangeResult? LastResult
        {
            get => _lastResult;
            set
            {
                SetField(ref _lastResult, value);
                OnPropertyChanged(nameof(HasResult));
            }
        }
        public bool HasResult => LastResult != null;

        // Transaction history shown in this tab
        public ObservableCollection<ExchangeResult> History { get; } = new();

        // ── Commands ──────────────────────────────────────────────────────────

        public AsyncRelayCommand BuyCommand { get; }
        public AsyncRelayCommand SellCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        public ExchangeViewModel()
        {
            BuyCommand = new AsyncRelayCommand(BuyAsync, () => !IsBusy);
            SellCommand = new AsyncRelayCommand(SellAsync, () => !IsBusy);
        }

        // ── Logic ─────────────────────────────────────────────────────────────

        /// <summary>
        /// BUY: client spends PLN, receives foreign currency.
        /// Amount field = PLN to spend.
        /// </summary>
        private async Task BuyAsync()
        {
            if (!Validate()) return;

            IsBusy = true;
            StatusMessage = $"Buying {CurrencyCode.ToUpper()} with {Amount:N2} PLN...";

            await Task.Run(() =>
            {
                try
                {
                    using var client = new ExchangeServiceClient();
                    var result = client.BuyCurrency(CurrencyCode.Trim().ToUpper(), Amount);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LastResult = result;
                        History.Insert(0, result);
                        StatusMessage = result.Success
                            ? $"✓ {result.Message}"
                            : $"⚠ {result.Message}";
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

        /// <summary>
        /// SELL: client spends foreign currency, receives PLN.
        /// Amount field = foreign currency to sell.
        /// </summary>
        private async Task SellAsync()
        {
            if (!Validate()) return;

            IsBusy = true;
            StatusMessage = $"Selling {Amount:N2} {CurrencyCode.ToUpper()} for PLN...";

            await Task.Run(() =>
            {
                try
                {
                    using var client = new ExchangeServiceClient();
                    var result = client.SellCurrency(CurrencyCode.Trim().ToUpper(), Amount);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LastResult = result;
                        History.Insert(0, result);
                        StatusMessage = result.Success
                            ? $"✓ {result.Message}"
                            : $"⚠ {result.Message}";
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

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(CurrencyCode) || CurrencyCode.Trim().Length != 3)
            {
                StatusMessage = "⚠ Enter a valid 3-letter currency code (e.g. USD, EUR).";
                return false;
            }
            if (Amount <= 0)
            {
                StatusMessage = "⚠ Amount must be greater than zero.";
                return false;
            }
            return true;
        }
    }
}
