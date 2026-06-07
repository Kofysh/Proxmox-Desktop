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

    public MainWindow(ApiClient api)
    {
        InitializeComponent();
        Instance = this;

        _vm = new MainViewModel(api);
        DataContext = _vm;

        _vm.OnLogout      += OnLogout;
        _vm.OnOpenConsole += OpenConsole;
        _vm.OnOpenSpice   += OpenSpice;

        Loaded += async (_, _) => await _vm.RefreshAsync();
        Closed += (_, _) => { _vm.Dispose(); Instance = null; };
    }

    private void OnLogout()
    {
        _vm.Dispose();
        Instance = null;
        var login = new LoginWindow();
        login.Show();
        Close();
    }

    private void OpenConsole(MachineData machine, string url)
    {
        var win = new ConsoleWindow(machine, url);
        win.Show();
    }

    private async void OpenSpice(SpiceObject cfg)
        => await SpiceLauncher.LaunchAsync(cfg);

    private void OnThemeToggle(object sender, RoutedEventArgs e)
    {
        _isDark = !_isDark;
        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();
        theme.SetBaseTheme(_isDark ? BaseTheme.Dark : BaseTheme.Light);
        paletteHelper.SetTheme(theme);
        ThemeIcon.Kind = _isDark ? PackIconKind.WeatherNight : PackIconKind.WeatherSunny;
    }
}
