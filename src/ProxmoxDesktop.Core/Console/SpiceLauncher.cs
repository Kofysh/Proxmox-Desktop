using System.Diagnostics;
using System.Text;
using ProxmoxDesktop.Core.Api.Models;

namespace ProxmoxDesktop.Core.Console;

/// <summary>
/// Lance une session SPICE via virt-viewer en générant un fichier .vv temporaire.
/// Logique identique à l'ancienne app WinForms, portée en async.
/// </summary>
public static class SpiceLauncher
{
    public static async Task LaunchAsync(SpiceObject spice, string virtViewerPath = @"C:\Program Files\VirtViewer v?.?-x64\bin\remote-viewer.exe")
    {
        var vvContent = BuildVvFile(spice);
        var tempFile  = Path.Combine(Path.GetTempPath(), $"proxmox_spice_{Guid.NewGuid():N}.vv");

        try
        {
            await File.WriteAllTextAsync(tempFile, vvContent, Encoding.UTF8);

            var psi = new ProcessStartInfo
            {
                FileName        = virtViewerPath,
                Arguments       = $"\"{tempFile}\"",
                UseShellExecute = true
            };
            Process.Start(psi);

            // Attendre 3s puis supprimer le fichier temporaire
            await Task.Delay(3000);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static string BuildVvFile(SpiceObject spice)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[virt-viewer]");
        sb.AppendLine("type=spice");
        sb.AppendLine($"host={spice.Host}");
        sb.AppendLine($"tls-port={spice.TlsPort}");
        sb.AppendLine($"password={spice.Password}");
        if (!string.IsNullOrEmpty(spice.Ca))           sb.AppendLine($"ca={spice.Ca}");
        if (!string.IsNullOrEmpty(spice.HostSubject))  sb.AppendLine($"host-subject={spice.HostSubject}");
        if (!string.IsNullOrEmpty(spice.Proxy))        sb.AppendLine($"proxy={spice.Proxy}");
        sb.AppendLine($"title={spice.Title ?? "Proxmox SPICE"}");
        sb.AppendLine("delete-this-file=1");
        sb.AppendLine("fullscreen=0");
        return sb.ToString();
    }
}
