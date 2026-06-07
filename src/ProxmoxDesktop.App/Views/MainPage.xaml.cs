using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProxmoxDesktop.App.ViewModels;
using ProxmoxDesktop.Core.Api;
using ProxmoxDesktop.Core.Api.Models;

namespace ProxmoxDesktop.App.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; private set; } = null!;

    public MainPage() => InitializeComponent();

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not ApiClient api) return;

        ViewModel = new MainViewModel(api);
        ViewModel.OnLogout += () =>
        {
            ViewModel.Dispose();
            if (App.MainWindow is MainWindow win) win.NavigateToLogin();
        };
        ViewModel.OnOpenConsole += OpenConsole;
        ViewModel.OnOpenSpice  += OpenSpice;

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
        if (MainWindow.Instance is { } win)
        {
            win.ToggleTheme();
            UpdateThemeIcon();
        }
    }

    private void UpdateThemeIcon()
    {
        if (MainWindow.Instance is not { } win) return;
        // Moon = dark mode active, Sun = light mode active
        ThemeIcon.Glyph = win.CurrentTheme == ElementTheme.Dark
            ? "\uE706"   // Sun — click to switch to light
            : "\uE708";  // Moon — click to switch to dark
    }

    private void OpenConsole(MachineData machine, string url)
    {
        var page = new ConsolePage(machine, url);
        var window = new Microsoft.UI.Xaml.Window();
        window.Content = page;
        window.Title = $"Console — {machine.Name}";
        window.Activate();
    }

    private async void OpenSpice(SpiceObject spiceConfig)
    {
        await ProxmoxDesktop.Core.Console.SpiceLauncher.LaunchAsync(spiceConfig);
    }
}
