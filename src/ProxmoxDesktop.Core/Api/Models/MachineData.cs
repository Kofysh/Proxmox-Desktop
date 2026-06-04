using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

public class MachineData
{
    [JsonPropertyName("pid")]
    public int Pid { get; set; }

    [JsonPropertyName("vmid")]
    public int Vmid { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    // Set manually after fetch — not returned by PVE at the list level
    public string NodeName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "qemu";

    [JsonPropertyName("cpu")]
    public double Cpu { get; set; }

    [JsonPropertyName("cpus")]
    public int Cpus { get; set; }

    [JsonPropertyName("mem")]
    public long Mem { get; set; }

    [JsonPropertyName("maxmem")]
    public long MaxMem { get; set; }

    [JsonPropertyName("netin")]
    public long NetIn { get; set; }

    [JsonPropertyName("netout")]
    public long NetOut { get; set; }

    [JsonPropertyName("disk")]
    public long Disk { get; set; }

    [JsonPropertyName("diskread")]
    public long DiskRead { get; set; }

    [JsonPropertyName("diskwrite")]
    public long DiskWrite { get; set; }

    [JsonPropertyName("maxdisk")]
    public long MaxDisk { get; set; }

    [JsonPropertyName("swap")]
    public long Swap { get; set; }

    [JsonPropertyName("maxswap")]
    public long MaxSwap { get; set; }

    [JsonPropertyName("tags")]
    public string? Tags { get; set; }

    [JsonPropertyName("uptime")]
    public long Uptime { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("serial")]
    public int Serial { get; set; }

    [JsonPropertyName("lock")]
    public string? Lock { get; set; }

    // Computed helpers
    public bool IsRunning => Status.Equals("running", StringComparison.OrdinalIgnoreCase);
    public bool IsLxc    => Type.Equals("lxc", StringComparison.OrdinalIgnoreCase);
    public double CpuPercent => Math.Round(Cpu * 100, 1);
    public double MemPercent => MaxMem > 0 ? Math.Round((double)Mem / MaxMem * 100, 1) : 0;
}
