using Microsoft.UI.Xaml.Controls;
using ProxmoxDesktop.Api.Models;

namespace ProxmoxDesktop.Views;

public sealed partial class ConsolePage : Page
{
    public ConsolePage(MachineData machine, string url)
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            await ConsoleWebView.EnsureCoreWebView2Async();
            ConsoleWebView.CoreWebView2.Navigate(url);
        };
    }
}
