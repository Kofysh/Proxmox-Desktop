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
        // Desktop toast notifications are intentionally disabled in the WPF build.
    }

    public static void ClearHistory()
    {
        // No notification history is kept by the WPF build.
    }
}
