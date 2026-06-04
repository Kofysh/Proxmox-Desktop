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
        await ViewModel.RefreshAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel?.Dispose();
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
        // Lance virt-viewer via le SpiceLauncher (logique identique à l'ancienne app)
        await ProxmoxDesktop.Core.Console.SpiceLauncher.LaunchAsync(spiceConfig);
    }
}
