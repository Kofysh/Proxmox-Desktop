using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Config;
using ProxmoxDesktop.Console;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }
    public ISnackbarMessageQueue SnackbarService => MainSnackbar.MessageQueue!;

    private readonly MainViewModel        _vm;
    private readonly ConfigurationService _config = new();
    private bool _isDark;

    public MainWindow(IApiClient api)
    {
        InitializeComponent();
        Instance = this;

        _isDark = _config.Config.IsDarkTheme;
        ApplyTheme();

        _vm         = new MainViewModel((ApiClient)api, _config.Config.RefreshIntervalSeconds);
        DataContext = _vm;

        _vm.OnLogout      += () => { _vm.Dispose(); Instance = null; new LoginWindow().Show(); Close(); };
        _vm.OnOpenConsole += (m, url) => new ConsoleWindow(m, url).Show();
        _vm.OnOpenSpice   += async cfg => await SpiceLauncher.LaunchAsync(cfg);
        _vm.OnNotify      += msg => Dispatcher.Invoke(() => SnackbarService.Enqueue(msg));
        _vm.OnRequestAddServer += () =>
        {
            var dlg = new LoginWindow(connectedApi => _vm.AddConnection(connectedApi)) { Owner = this };
            dlg.ShowDialog();
        };

        Loaded += async (_, _) => await _vm.RefreshAsync();
        Closed += (_, _) => { _vm.Dispose(); Instance = null; };
    }

    private void OnThemeToggle(object sender, RoutedEventArgs e)
    {
        _isDark = !_isDark;
        ApplyTheme();
        _config.Config.IsDarkTheme = _isDark;
        _config.Save();
    }

    private void ApplyTheme()
    {
        var helper = new PaletteHelper();
        var theme  = helper.GetTheme();
        theme.SetBaseTheme(_isDark ? BaseTheme.Dark : BaseTheme.Light);
        helper.SetTheme(theme);
        ThemeIcon.Kind = _isDark ? PackIconKind.WeatherNight : PackIconKind.WeatherSunny;
    }

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
            e.Handled = true;
        }
    }
}
