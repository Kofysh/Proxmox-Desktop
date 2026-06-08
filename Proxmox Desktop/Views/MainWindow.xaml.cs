using System.Windows;
using MaterialDesignThemes.Wpf;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Api.Models;
using ProxmoxDesktop.Console;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }
    public ISnackbarMessageQueue SnackbarService => MainSnackbar.MessageQueue!;

    private readonly MainViewModel _vm;
    private bool _isDark = true;

    public MainWindow(IApiClient api)
    {
        InitializeComponent();
        Instance    = this;
        _vm         = new MainViewModel((ApiClient)api);
        DataContext = _vm;
        _vm.OnLogout      += () => { _vm.Dispose(); Instance = null; new LoginWindow().Show(); Close(); };
        _vm.OnOpenConsole += (m, url) => new ConsoleWindow(m, url).Show();
        _vm.OnOpenSpice   += async cfg => await SpiceLauncher.LaunchAsync(cfg);
        Loaded += async (_, _) => await _vm.RefreshAsync();
        Closed += (_, _) => { _vm.Dispose(); Instance = null; };
    }

    private void OnThemeToggle(object sender, RoutedEventArgs e)
    {
        _isDark = !_isDark;
        var helper = new PaletteHelper();
        var theme  = helper.GetTheme();
        theme.SetBaseTheme(_isDark ? BaseTheme.Dark : BaseTheme.Light);
        helper.SetTheme(theme);
        ThemeIcon.Kind = _isDark ? PackIconKind.WeatherNight : PackIconKind.WeatherSunny;
    }
}
