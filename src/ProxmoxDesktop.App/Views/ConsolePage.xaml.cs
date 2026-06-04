using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProxmoxDesktop.Core.Api.Models;

namespace ProxmoxDesktop.App.Views;

public sealed partial class ConsolePage : Page
{
    private readonly MachineData _machine;
    private readonly string _url;

    public ConsolePage(MachineData machine, string url)
    {
        InitializeComponent();
        _machine = machine;
        _url     = url;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ConsoleTitleBlock.Text = $"{_machine.Name} ({_machine.Vmid}) — {_machine.NodeName}";
        await ConsoleWebView.EnsureCoreWebView2Async();
        // Autoriser les certificats auto-signés Proxmox
        ConsoleWebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
        ConsoleWebView.Source = new Uri(_url);
    }

    private void Reload_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        => ConsoleWebView.Reload();

    private void WebView_NavigationStarting(WebView2 sender,
        Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e) { }

    private void WebView_NavigationCompleted(WebView2 sender,
        Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e) { }
}
