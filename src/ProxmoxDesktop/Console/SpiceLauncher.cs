using System.Diagnostics;
using System.Text;
using ProxmoxDesktop.Api.Models;

namespace ProxmoxDesktop.Console;

public static class SpiceLauncher
{
    public static async Task LaunchAsync(SpiceObject spice,
        string virtViewerPath = @"C:\Program Files\VirtViewer v?.?-x64\bin\remote-viewer.exe")
    {
        var vvContent = BuildVvFile(spice);
        var tempFile  = Path.Combine(Path.GetTempPath(), $"proxmox_spice_{Guid.NewGuid():N}.vv");
        try
        {
            await File.WriteAllTextAsync(tempFile, vvContent, Encoding.UTF8);
            Process.Start(new ProcessStartInfo { FileName = virtViewerPath, Arguments = $"\"{tempFile}\"", UseShellExecute = true });
            await Task.Delay(3000);
        }
        finally { if (File.Exists(tempFile)) File.Delete(tempFile); }
    }

    private static string BuildVvFile(SpiceObject s)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[virt-viewer]");
        sb.AppendLine("type=spice");
        sb.AppendLine($"host={s.Host}");
        sb.AppendLine($"tls-port={s.TlsPort}");
        sb.AppendLine($"password={s.Password}");
        if (!string.IsNullOrEmpty(s.Ca))          sb.AppendLine($"ca={s.Ca}");
        if (!string.IsNullOrEmpty(s.HostSubject)) sb.AppendLine($"host-subject={s.HostSubject}");
        if (!string.IsNullOrEmpty(s.Proxy))       sb.AppendLine($"proxy={s.Proxy}");
        sb.AppendLine($"title={s.Title ?? "Proxmox SPICE"}");
        sb.AppendLine("delete-this-file=1");
        sb.AppendLine("fullscreen=0");
        return sb.ToString();
    }
}
