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

    private async void OnSectionClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string section }) return;

        SectionTitle.Text = section;
        if (section is "Overview" or "Virtual machines" or "Containers")
            return;

        var connection = _vm.Connections.FirstOrDefault();
        if (connection is null) return;

        var path = section switch
        {
            "Nodes" => "nodes",
            "Storage" => "storage",
            "Network" => "cluster/ha/resources",
            "Backups" => "cluster/backup",
            "Tasks and logs" => "cluster/tasks",
            "Users and permissions" => "access/users",
            _ => string.Empty
        };

        if (path.Length == 0) return;
        try
        {
            var rows = await connection.Api.GetResourceListAsync(path);
            var panel = new StackPanel { Spacing = 8, MinWidth = 620 };
            panel.Children.Add(new TextBlock
            {
                Text = $"{rows.Count} élément(s) · API: /api2/json/{path}",
                Opacity = 0.7
            });

            foreach (var row in rows.Take(100))
            {
                var values = row
                    .Where(pair => pair.Value.ValueKind is not System.Text.Json.JsonValueKind.Null)
                    .Select(pair => $"{pair.Key}: {pair.Value}")
                    .ToArray();
                panel.Children.Add(new TextBlock
                {
                    Text = string.Join("  •  ", values),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            var dialog = new ContentDialog
            {
                Title = section,
                Content = new ScrollViewer { Content = panel, MaxHeight = 600 },
                CloseButtonText = "Fermer",
                XamlRoot = Root.XamlRoot
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            await ShowMessageAsync($"Impossible de charger {section}: {ex.Message}");
        }
    }

    private async Task ShowMessageAsync(string message)
    {
        var dialog = new ContentDialog { Title = "Proxmox Desktop", Content = message, CloseButtonText = "OK", XamlRoot = Root.XamlRoot };
        await dialog.ShowAsync();
    }

}
