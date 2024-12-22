using NPServer.Application.Main;
using NPServer.Application.Threading;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Logging.Targets;
using NPServer.Shared.Services;
using NPServer.UI.Enums;
using NPServer.UI.Implementations;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NPServer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly CancellationTokenSource _ctokens = new();
        private readonly App _appInstance = (App)System.Windows.Application.Current;
        private static readonly ServerApp _serverApplication = Singleton.GetInstance<ServerApp>(() => new ServerApp(_ctokens));

        public MainWindow()
        {
            this.InitializeServices();  
            this.InitializeComponent();
            this.InitializeLogging();

            StopButton.IsEnabled = false;
        }

        private void InitializeServices()
        {
            ServiceController.RegisterSingleton();
            ServiceController.Initialization();

            _appInstance.Initialize();
        }

        private void InitializeLogging()
        {
            NPLog.Instance.LoggerHandlerManager
                .AddHandler(new FileTarget())
                .AddHandler(new WinFormTagers(new NLogWinFormTagers(LogsTextBox)));
        }

        private void ThemeSwitchButtonClick(object sender, RoutedEventArgs e)
        {
            var currentTheme = _appInstance.GetCurrentTheme();
            var newTheme = currentTheme == Theme.Dark ? Theme.Light : Theme.Dark;
            _appInstance.ChangeTheme(newTheme);
        }

        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _serverApplication.Shutdown();
            _ctokens.Cancel();
        }

        private async void StartServerButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => _serverApplication.Run());
            }
            finally
            {
                StartButton.IsEnabled = false;

                await Task.Delay(5000);
                StopButton.IsEnabled = true;
            }
        }

        private async void ShutdownServerButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => _serverApplication.Shutdown());
            }
            finally
            {
                StopButton.IsEnabled = false;

                await Task.Delay(5000);
                StartButton.IsEnabled = true;
            }
        }
    }
}