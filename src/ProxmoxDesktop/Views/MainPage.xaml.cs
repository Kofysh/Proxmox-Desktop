using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Api.Models;
using ProxmoxDesktop.Console;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; private set; } = null!;

    public MainPage() => InitializeComponent();

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not ApiClient api) return;

        ViewModel = new MainViewModel(api);
        ViewModel.OnLogout += () => { ViewModel.Dispose(); App.MainWindow?.NavigateToLogin(); };
        ViewModel.OnOpenConsole += OpenConsole;
        ViewModel.OnOpenSpice   += OpenSpice;

        Bindings.Update();
        UpdateThemeIcon();
        await ViewModel.RefreshAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel?.Dispose();
    }

    private void OnThemeToggleClick(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.ToggleTheme();
        UpdateThemeIcon();
    }

    private void UpdateThemeIcon()
    {
        if (MainWindow.Instance is not { } win) return;
        ThemeIcon.Glyph = win.CurrentTheme == ElementTheme.Dark ? "\uE706" : "\uE708";
    }

    private void OpenConsole(MachineData machine, string url)
    {
        var win = new Microsoft.UI.Xaml.Window();
        win.Content = new ConsolePage(machine, url);
        win.Title   = $"Console — {machine.Name}";
        win.Activate();
    }

    private async void OpenSpice(SpiceObject cfg) => await SpiceLauncher.LaunchAsync(cfg);
}
