using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Config;
using ProxmoxDesktop.Console;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly ConfigurationService _config = new();

    public MainWindow(IApiClient api)
    {
        InitializeComponent();
        _vm = new MainViewModel((ApiClient)api, _config.Config.RefreshIntervalSeconds,
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
        Root.DataContext = _vm;
        _vm.OnLogout += Logout;
        _vm.OnOpenConsole += (machine, url) => new ConsoleWindow(machine, url).Activate();
        _vm.OnOpenSpice += cfg => _ = SpiceLauncher.LaunchAsync(cfg);
        _vm.OnNotify += message => _ = ShowMessageAsync(message);
        _vm.OnRequestAddServer += () => new LoginWindow(_vm.AddConnection).Activate();
        Closed += (_, _) => { _vm.Dispose(); };
        Activated += async (_, _) => { if (_vm.Machines.Count == 0) await _vm.RefreshAsync(); };
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1440, 900));
    }

    private void Logout()
    {
        _vm.Dispose();
        var login = new LoginWindow();
        login.Activate();
        Close();
    }

    private void OnThemeToggle(object sender, RoutedEventArgs e)
    {
        _config.Config.IsDarkTheme = !_config.Config.IsDarkTheme;
        _config.Save();
        Root.RequestedTheme = _config.Config.IsDarkTheme ? ElementTheme.Dark : ElementTheme.Light;
    }

    private async Task ShowMessageAsync(string message)
    {
        var dialog = new ContentDialog { Title = "Proxmox Desktop", Content = message, CloseButtonText = "OK", XamlRoot = Root.XamlRoot };
        await dialog.ShowAsync();
    }

}
