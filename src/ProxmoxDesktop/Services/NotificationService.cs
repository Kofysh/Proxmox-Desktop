using System.Windows;

namespace ProxmoxDesktop.Services;

/// <summary>
/// Windows toast notifications via WPF MessageBox fallback.
/// For proper toasts, integrate Hardcodet.NotifyIcon or Windows Community Toolkit.
/// </summary>
public static class NotificationService
{
    private static bool _registered;

    public static void Register()   => _registered = true;
    public static void Unregister() => _registered = false;

    public static void NotifyStateChange(string vmName, int vmid, string oldStatus, string newStatus)
    {
        if (!_registered) return;
        try
        {
            var title = newStatus switch
            {
                "running"   => $"{vmName} started",
                "stopped"   => $"{vmName} stopped",
                "suspended" => $"{vmName} suspended",
                _           => $"{vmName} — {newStatus}"
            };

            Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                var snackbar = ProxmoxDesktop.Views.MainWindow.Instance?.SnackbarService;
                snackbar?.Enqueue(title, $"VMID {vmid} · {oldStatus} → {newStatus}", null, null, false, false, TimeSpan.FromSeconds(4));
            });
        }
        catch { }
    }
}
