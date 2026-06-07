using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace ProxmoxDesktop.App.Services;

/// <summary>
/// Sends Windows toast notifications when a VM/CT changes state.
/// Uses AppNotificationManager (Windows App SDK) — no MSIX required in unpackaged mode
/// as long as the app is registered via AppNotificationManager.Default.Register().
/// </summary>
public static class NotificationService
{
    private static bool _registered;

    /// <summary>
    /// Call once at app startup to register the notification channel.
    /// Safe to call multiple times.
    /// </summary>
    public static void Register()
    {
        if (_registered) return;
        try
        {
            AppNotificationManager.Default.Register();
            _registered = true;
        }
        catch
        {
            // Notifications not available on this system — silently degrade
        }
    }

    /// <summary>Unregister on app exit to clean up the notification channel.</summary>
    public static void Unregister()
    {
        if (!_registered) return;
        try { AppNotificationManager.Default.Unregister(); }
        catch { }
    }

    /// <summary>
    /// Notifies that a VM/CT has changed its running state.
    /// </summary>
    /// <param name="vmName">Display name of the VM or container.</param>
    /// <param name="vmid">VMID for reference.</param>
    /// <param name="oldStatus">Previous status (e.g. "stopped").</param>
    /// <param name="newStatus">New status (e.g. "running").</param>
    public static void NotifyStateChange(string vmName, int vmid, string oldStatus, string newStatus)
    {
        if (!_registered) return;
        try
        {
            var (icon, title) = newStatus switch
            {
                "running"   => ("\uE768", $"{vmName} started"),
                "stopped"   => ("\uEE95", $"{vmName} stopped"),
                "suspended" => ("\uE945", $"{vmName} suspended"),
                _           => ("\uE9CE", $"{vmName} — {newStatus}")
            };

            var builder = new AppNotificationBuilder()
                .AddText(title)
                .AddText($"VMID {vmid} · {oldStatus} \u2192 {newStatus}")
                .SetAppLogoOverride(new Uri("ms-appx:///Assets/proxmox.png"));

            AppNotificationManager.Default.Show(builder.BuildNotification());
        }
        catch { }
    }
}
