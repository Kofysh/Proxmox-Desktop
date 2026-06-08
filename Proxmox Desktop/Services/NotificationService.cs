using Microsoft.Toolkit.Uwp.Notifications;

namespace ProxmoxDesktop.Services;

/// <summary>
/// Sends Windows 10/11 toast notifications for VM state changes.
/// Falls back silently if toasts are unavailable (e.g. Windows Server without notification center).
/// </summary>
public static class NotificationService
{
    private static bool _enabled;

    public static void Enable()  => _enabled = true;
    public static void Disable() => _enabled = false;

    public static void NotifyStateChange(
        string vmName, int vmid, string oldStatus, string newStatus)
    {
        if (!_enabled) return;
        try
        {
            var (icon, title) = newStatus switch
            {
                "running"   => ("\u25B6", $"{vmName} is now running"),
                "stopped"   => ("\u23F9", $"{vmName} has stopped"),
                "suspended" => ("\u23F8", $"{vmName} is suspended"),
                "paused"    => ("\u23F8", $"{vmName} is paused"),
                _           => ("\u2139", $"{vmName} — {newStatus}")
            };

            new ToastContentBuilder()
                .AddText($"{icon} {title}")
                .AddText($"VMID {vmid} · {oldStatus} \u2192 {newStatus}")
                .AddAttributionText("Proxmox Desktop")
                .Show();
        }
        catch { /* never crash the app for a notification */ }
    }

    public static void ClearHistory()
    {
        try { ToastNotificationManagerCompat.History.Clear(); }
        catch { }
    }
}
