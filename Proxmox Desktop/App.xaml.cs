using System.Windows;
using ProxmoxDesktop.Services;

namespace ProxmoxDesktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        NotificationService.Enable();
        new Views.LoginWindow().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        NotificationService.Disable();
        base.OnExit(e);
    }
}
