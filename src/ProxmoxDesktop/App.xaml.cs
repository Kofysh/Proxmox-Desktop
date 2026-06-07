using Microsoft.UI.Xaml;
using ProxmoxDesktop.Services;
using ProxmoxDesktop.Views;

namespace ProxmoxDesktop;

public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }

    public App() => InitializeComponent();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        NotificationService.Register();
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
