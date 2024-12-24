using NPServer.Application.Main;
using NPServer.Application.Threading;
using NPServer.Core.Interfaces.Session;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Logging.Targets;
using NPServer.Shared.Management;
using NPServer.Shared.Services;
using NPServer.UI.Core.Enums;
using NPServer.UI.Core.Implementations;
using NPServer.UI.ViewsModels;
using System;
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
        private readonly SessionViewModel _viewModel;
        private static readonly CancellationTokenSource _ctokens = new();

        private readonly App _appInstance = (App)System.Windows.Application.Current;
        private static readonly ServerApp _serverApplication = Singleton.GetInstance<ServerApp>(() => new ServerApp(_ctokens));

        public MainWindow()
        {
            this.InitializeServices();
            this.InitializeComponent();
            this.InitializeLogging();

            ButtonStop.IsEnabled = false;
            LabelInfoCPU.Content = "CPU Name: " + InfoCPU.Name();
            LabelInfoOS.Content = "OS: " + InfoOS.Details();

            ButtonTheme.Content = Theme.Dark.ToString();

            _viewModel = new SessionViewModel(Singleton.GetInstanceOfInterface<ISessionManager>());
            DataContext = _viewModel;
        }

        private void InitializeServices()
        {
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
            ButtonTheme.Content = currentTheme.ToString();

            var newTheme = currentTheme == Theme.Dark ? Theme.Light : Theme.Dark;
            _appInstance.ChangeTheme(newTheme);
        }

        private async void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is FrameworkElement content)
                {
                    await AnimateOpacity(content, from: 1.0, to: 0.0, duration: 100);
                    await AnimateOpacity(content, from: 0.0, to: 1.0, duration: 100);
                }
            }
        }

        private static async Task AnimateOpacity(FrameworkElement element, double from, double to, int duration)
        {
            const int frames = 120; // Số khung hình
            double step = (to - from) / frames;
            int delay = duration / frames;

            for (int i = 0; i <= frames; i++)
            {
                element.Opacity = from + (step * i);
                await Task.Delay(delay);
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _serverApplication.Shutdown();
            _ctokens.Cancel();
        }

        private async void ClearLogButtonClick(object sender, RoutedEventArgs e)
        {
            double opacity = 1.0;
            while (opacity >= 0)
            {
                LogsTextBox.Opacity = opacity;
                opacity -= 0.1;
                await Task.Delay(50);
            }

            LogsTextBox.Text = string.Empty;
            LogsTextBox.Opacity = 1.0;
        }

        private void InfoButtonClick(object sender, RoutedEventArgs e)
        {
            LogsTextBox.AppendText("CPU: " + InfoCPU.Usage() + Environment.NewLine);
            LogsTextBox.AppendText("Ram: " + InfoMemory.Usage() + Environment.NewLine);
            LogsTextBox.ScrollToEnd();
        }

        private async void StartServerButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => _serverApplication.Run());
            }
            finally
            {
                LabelAddressIP.Content = $"IP: {ServiceController.NetworkConfig.IP}";
                LabelPort.Content = $"Port: {ServiceController.NetworkConfig.Port}";
                ButtonStart.IsEnabled = false;

                await Task.Delay(2000);
                ButtonStop.IsEnabled = true;
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
                LabelAddressIP.Content = $"IP: None";
                LabelPort.Content = $"Port: None";
                ButtonStop.IsEnabled = false;

                await Task.Delay(2000);
                ButtonStart.IsEnabled = true;
            }
        }
    }
}