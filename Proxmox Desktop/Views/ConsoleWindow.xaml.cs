using System.Windows;
using ProxmoxDesktop.Api.Models;

namespace ProxmoxDesktop.Views;

public partial class ConsoleWindow : Window
{
    private readonly string _url;
    public ConsoleWindow(MachineData machine, string url)
    {
        InitializeComponent();
        _url = url;
        Title = $"Console - {machine.Name} (VMID {machine.Vmid})";
        Width = 1280;
        Height = 780;
        Loaded += async (_, _) =>
        {
            await ConsoleWebView.EnsureCoreWebView2Async();
            ConsoleWebView.CoreWebView2.Navigate(_url);
        };
    }
}
