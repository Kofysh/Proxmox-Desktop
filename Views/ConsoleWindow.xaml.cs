using System.Windows;
using ProxmoxDesktop.Api.Models;

namespace ProxmoxDesktop.Views;

public partial class ConsoleWindow : Window
{
    public ConsoleWindow(MachineData machine, string url)
    {
        InitializeComponent();
        Title = $"Console — {machine.Name} (VMID {machine.Vmid})";
        Loaded += async (_, _) => { await ConsoleWebView.EnsureCoreWebView2Async(); ConsoleWebView.CoreWebView2.Navigate(url); };
    }
}
