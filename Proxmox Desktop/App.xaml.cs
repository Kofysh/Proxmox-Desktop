using ProxmoxDesktop.Services;
using ProxmoxDesktop.Views;
using ProxmoxDesktop.Api;
using Microsoft.UI.Xaml;

namespace ProxmoxDesktop;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        NotificationService.Enable();
        MainWindow = new LoginWindow();
        MainWindow.Activate();
    }

    public static void ShowMainWindow(IApiClient api)
    {
        var window = new MainWindow(api);
        MainWindow = window;
        window.Activate();
    }
}
