using CurrencyExchangeClient.Services;

namespace CurrencyExchangeClient.ViewModels
{
    /// <summary>
    /// Root ViewModel for MainWindow.
    /// Owns child ViewModels for each tab and handles the connection check.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        // ── Child ViewModels ──────────────────────────────────────────────────

        public RatesViewModel RatesVM { get; } = new();
        public ExchangeViewModel ExchangeVM { get; } = new();
        public HistoryViewModel HistoryVM { get; } = new();

        // ── Connection status ─────────────────────────────────────────────────

        private string _connectionStatus = "Checking connection...";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetField(ref _connectionStatus, value);
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetField(ref _isConnected, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────

        public AsyncRelayCommand PingCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        public MainViewModel()
        {
            PingCommand = new AsyncRelayCommand(PingServiceAsync);

            // Auto-ping on startup
            _ = PingServiceAsync();
        }

        // ── Logic ─────────────────────────────────────────────────────────────

        private async Task PingServiceAsync()
        {
            ConnectionStatus = "Connecting to service...";
            IsConnected = false;

            await Task.Run(() =>
            {
                try
                {
                    using var client = new ExchangeServiceClient();
                    string response = client.Ping();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsConnected = true;
                        ConnectionStatus = $"✓ Connected — {response}";
                    });
                }
                catch (Exception ex)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsConnected = false;
                        ConnectionStatus = $"⚠ Cannot reach service: {ex.Message}";
                    });
                }
            });
        }
    }
}
