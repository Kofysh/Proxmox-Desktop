using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

/// <summary>Représente une VM QEMU ou un container LXC retourné par l'API PVE.</summary>
public sealed class MachineData
{
    [JsonPropertyName("vmid")]    public int    Vmid     { get; init; }
    [JsonPropertyName("name")]    public string Name     { get; init; } = string.Empty;
    [JsonPropertyName("nodename")]public string NodeName { get; init; } = string.Empty;
    [JsonPropertyName("type")]    public string Type     { get; init; } = "qemu";
    [JsonPropertyName("status")] public string Status   { get; init; } = string.Empty;
    [JsonPropertyName("lock")]    public string? Lock    { get; init; }
    [JsonPropertyName("tags")]    public string? Tags    { get; init; }

    // Ressources
    [JsonPropertyName("cpu")]     public double Cpu     { get; init; }
    [JsonPropertyName("cpus")]    public int    Cpus    { get; init; }
    [JsonPropertyName("mem")]     public long   Mem     { get; init; }
    [JsonPropertyName("maxmem")] public long   MaxMem  { get; init; }
    [JsonPropertyName("disk")]    public long   Disk    { get; init; }
    [JsonPropertyName("maxdisk")]public long   MaxDisk { get; init; }
    [JsonPropertyName("swap")]    public long   Swap    { get; init; }
    [JsonPropertyName("maxswap")]public long   MaxSwap { get; init; }
    [JsonPropertyName("uptime")] public long   Uptime  { get; init; }
    [JsonPropertyName("netin")]   public long   NetIn   { get; init; }
    [JsonPropertyName("netout")] public long   NetOut  { get; init; }
    [JsonPropertyName("diskread")]public long   DiskRead  { get; init; }
    [JsonPropertyName("diskwrite")]public long  DiskWrite { get; init; }
    [JsonPropertyName("serial")] public int    Serial  { get; init; }
    [JsonPropertyName("pid")]     public int    Pid     { get; init; }

    // Propriétés calculées (pas de champ JSON correspondant)
    [JsonIgnore] public bool IsRunning => Status == "running";
    [JsonIgnore] public bool IsLxc     => Type   == "lxc";

    /// <summary>CPU en % (0-100), arrondi à 1 décimale.</summary>
    [JsonIgnore] public double CpuPercent => Math.Round(Cpu * 100, 1);

    /// <summary>RAM utilisée en % (0-100), arrondi à 1 décimale.</summary>
    [JsonIgnore] public double MemPercent =>
        MaxMem > 0 ? Math.Round((double)Mem / MaxMem * 100, 1) : 0;
}
