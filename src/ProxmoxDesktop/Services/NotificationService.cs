using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace ProxmoxDesktop.Services;

public static class NotificationService
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered) return;
        try { AppNotificationManager.Default.Register(); _registered = true; }
        catch { }
    }

    public static void Unregister()
    {
        if (!_registered) return;
        try { AppNotificationManager.Default.Unregister(); } catch { }
    }

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
            var notification = new AppNotificationBuilder()
                .AddText(title)
                .AddText($"VMID {vmid} · {oldStatus} → {newStatus}")
                .BuildNotification();
            AppNotificationManager.Default.Show(notification);
        }
        catch { }
    }
}
