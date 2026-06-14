using System.Windows;
using MaterialDesignThemes.Wpf;
using ProxmoxDesktop.Config;
using ProxmoxDesktop.Services;

namespace ProxmoxDesktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        NotificationService.Enable();
        ApplySavedTheme();
        new Views.LoginWindow().Show();
    }

    private static void ApplySavedTheme()
    {
        try
        {
            var cfg    = new ConfigurationService().Config;
            var helper = new PaletteHelper();
            var theme  = helper.GetTheme();
            theme.SetBaseTheme(cfg.IsDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
            helper.SetTheme(theme);
        }
        catch { /* fall back to the default theme from App.xaml */ }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        NotificationService.Disable();
        base.OnExit(e);
    }
}
