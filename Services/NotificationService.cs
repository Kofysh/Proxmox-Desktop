using System.Windows;

namespace ProxmoxDesktop.Services;

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
                Views.MainWindow.Instance?.SnackbarService.Enqueue(
                    title, $"VMID {vmid} · {oldStatus} → {newStatus}",
                    null, null, false, false, TimeSpan.FromSeconds(4)));
        }
        catch { }
    }
}
