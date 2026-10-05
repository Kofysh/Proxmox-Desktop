using ProxmoxDesktop.Services;
using ProxmoxDesktop.Views;
using ProxmoxDesktop.Api;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;

namespace ProxmoxDesktop;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    public App()
    {
        UnhandledException += OnUnhandledException;
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            NotificationService.Enable();
            MainWindow = new LoginWindow();
            MainWindow.Activate();
        }
        catch (Exception exception)
        {
            ReportStartupFailure(exception);
        }
    }

    public static void ShowMainWindow(IApiClient api)
    {
        var window = new MainWindow(api);
        MainWindow = window;
        window.Activate();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
    {
        args.Handled = true;
        ReportStartupFailure(args.Exception);
    }

    private static void ReportStartupFailure(Exception exception)
    {
        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ProxmoxDesktop");
            Directory.CreateDirectory(directory);
            File.WriteAllText(
                Path.Combine(directory, "startup.log"),
                $"{DateTimeOffset.Now:O}{Environment.NewLine}{exception}");
        }
        catch
        {
            // Keep the native error notification available if log creation fails.
        }

        NativeMessageBox(
            "Proxmox Desktop n'a pas pu démarrer. Consultez le fichier startup.log dans %LOCALAPPDATA%\\ProxmoxDesktop.",
            "Proxmox Desktop");
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(
        nint hWnd,
        string text,
        string caption,
        uint type);

    private static void NativeMessageBox(string text, string caption)
        => MessageBox(nint.Zero, text, caption, 0x10);
}
