using System.Windows;
using ProxmoxDesktop.Services;

namespace ProxmoxDesktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        NotificationService.Register();

        var login = new Views.LoginWindow();
        login.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        NotificationService.Unregister();
        base.OnExit(e);
    }
}
