using System.IO;
using System.Text;
using ProxmoxDesktop.Api.Models;

namespace ProxmoxDesktop.Console;

public static class SpiceLauncher
{
    public static async Task LaunchAsync(SpiceObject spice)
    {
        var virt = FindVirtViewer();
        if (virt is null) { System.Windows.MessageBox.Show("virt-viewer not found. Install it to use SPICE.", "SPICE"); return; }

        var tmp = Path.GetTempFileName() + ".vv";
        await File.WriteAllTextAsync(tmp, BuildVvFile(spice), Encoding.UTF8);
        System.Diagnostics.Process.Start(virt, $"\"{tmp}\"");
    }

    private static string? FindVirtViewer()
    {
        string[] candidates = [
            @"C:\Program Files\VirtViewer\bin\remote-viewer.exe",
            @"C:\Program Files (x86)\VirtViewer\bin\remote-viewer.exe"
        ];
        return candidates.FirstOrDefault(File.Exists);
    }

    private static string BuildVvFile(SpiceObject s)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[virt-viewer]");
        sb.AppendLine("type=spice");
        if (s.Host     is not null) sb.AppendLine($"host={s.Host}");
        if (s.Port     is not null) sb.AppendLine($"port={s.Port}");
        if (s.TlsPort  is not null) sb.AppendLine($"tls-port={s.TlsPort}");
        if (s.Password is not null) sb.AppendLine($"password={s.Password}");
        if (s.Ca       is not null) sb.AppendLine($"ca={s.Ca}");
        if (s.Proxy    is not null) sb.AppendLine($"proxy={s.Proxy}");
        sb.AppendLine("fullscreen=0");
        sb.AppendLine("title=Proxmox SPICE Console");
        return sb.ToString();
    }
}
