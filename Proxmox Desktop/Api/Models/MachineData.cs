using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Api.Models;

public record MachineData
{
    [JsonPropertyName("vmid")]    public int    Vmid    { get; init; }
    [JsonPropertyName("name")]    public string Name    { get; init; } = string.Empty;
    [JsonPropertyName("status")]  public string Status  { get; init; } = string.Empty;
    [JsonPropertyName("cpu")]     public double Cpu     { get; init; }
    [JsonPropertyName("mem")]     public long   Mem     { get; init; }
    [JsonPropertyName("maxmem")]  public long   MaxMem  { get; init; }
    [JsonPropertyName("disk")]    public long   Disk    { get; init; }
    [JsonPropertyName("maxdisk")] public long   MaxDisk { get; init; }
    [JsonPropertyName("uptime")]  public long   Uptime  { get; init; }  // seconds
    [JsonPropertyName("netin")]   public long   NetIn   { get; init; }  // bytes
    [JsonPropertyName("netout")]  public long   NetOut  { get; init; }  // bytes
    [JsonPropertyName("lock")]    public string? Lock   { get; init; }
    [JsonPropertyName("serial")]  public int    Serial  { get; init; }
    [JsonPropertyName("tags")]    public string? Tags   { get; init; }

    // Set by ApiClient / ViewModel after deserialization
    public string NodeName   { get; init; } = string.Empty;
    public string Type       { get; init; } = string.Empty;
    public string ServerName { get; init; } = string.Empty;

    // Computed
    public bool   IsRunning    => Status == "running";
    public bool   IsLxc        => Type   == "lxc";
    public bool   IsLocked     => !string.IsNullOrEmpty(Lock);
    public double CpuPercent   => Cpu * 100.0;
    public double MemPercent   => MaxMem  > 0 ? (double)Mem  / MaxMem  * 100.0 : 0;
    public double DiskPercent  => MaxDisk > 0 ? (double)Disk / MaxDisk * 100.0 : 0;

    public string UptimeFormatted => Uptime switch
    {
        0     => "—",
        < 60  => $"{Uptime}s",
        < 3600 => $"{Uptime / 60}m",
        < 86400 => $"{Uptime / 3600}h {Uptime % 3600 / 60}m",
        _     => $"{Uptime / 86400}d {Uptime % 86400 / 3600}h"
    };

    public IReadOnlyList<string> TagList =>
        string.IsNullOrWhiteSpace(Tags)
            ? []
            : Tags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
