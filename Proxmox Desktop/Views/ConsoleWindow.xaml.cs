using Microsoft.UI.Xaml;
using ProxmoxDesktop.Api.Models;

namespace ProxmoxDesktop.Views;

public sealed partial class ConsoleWindow : Window
{
    private readonly string _url;

    public ConsoleWindow(MachineData machine, string url)
    {
        InitializeComponent();
        _url = url;
        Title = $"Console — {machine.Name} (VMID {machine.Vmid})";
        Activated += OnActivated;
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1280, 780));
    }

    private async void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (ConsoleWebView.CoreWebView2 is not null) return;
        await ConsoleWebView.EnsureCoreWebView2Async();
        ConsoleWebView.CoreWebView2?.Navigate(_url);
    }
}
