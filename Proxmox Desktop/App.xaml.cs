using System.Windows;
using ProxmoxDesktop.Views;

namespace ProxmoxDesktop;

public partial class App : Application
{
    public new static Window? MainWindow { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        MainWindow = new LoginWindow();
        MainWindow.Show();
    }

    public static void ShowMainWindow(Api.IApiClient api)
    {
        var window = new MainWindow((Api.ApiClient)api);
        MainWindow = window;
        window.Show();
    }
}
