using System.Windows;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Config;
using ProxmoxDesktop.Console;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly ConfigurationService _config = new();
    public MainWindow(ApiClient api)
    {
        InitializeComponent();
        _vm = new MainViewModel(api, _config.Config.RefreshIntervalSeconds, Dispatcher);
        DataContext = _vm;
        _vm.OnLogout += Logout;
        _vm.OnOpenConsole += (machine, url) => new ConsoleWindow(machine, url).Show();
        _vm.OnOpenSpice += cfg => _ = SpiceLauncher.LaunchAsync(cfg);
        _vm.OnNotify += message => MessageBox.Show(message, "Proxmox Desktop");
        _vm.OnRequestAddServer += () => new LoginWindow(_vm.AddConnection).Show();
        Closed += (_, _) => _vm.Dispose();
        Loaded += async (_, _) => await _vm.RefreshAsync();
    }
    private void Logout() { _vm.Dispose(); new LoginWindow().Show(); Close(); }
    private void Section_Click(object sender, RoutedEventArgs e)
        => SectionTitle.Text = (sender as System.Windows.Controls.Button)?.Content?.ToString() ?? "Overview";
}
