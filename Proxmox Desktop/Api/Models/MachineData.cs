using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Api.Models;

public record MachineData
{
    [JsonPropertyName("vmid")]   public int    Vmid     { get; init; }
    [JsonPropertyName("name")]   public string Name     { get; init; } = string.Empty;
    [JsonPropertyName("status")] public string Status   { get; init; } = string.Empty;
    [JsonPropertyName("cpu")]    public double Cpu      { get; init; }
    [JsonPropertyName("mem")]    public long   Mem      { get; init; }
    [JsonPropertyName("maxmem")] public long   MaxMem   { get; init; }
    [JsonPropertyName("lock")]   public string? Lock    { get; init; }
    [JsonPropertyName("serial")] public int    Serial   { get; init; }

    public string NodeName  { get; init; } = string.Empty;
    public string Type      { get; init; } = string.Empty;

    public bool   IsRunning  => Status == "running";
    public bool   IsLxc      => Type   == "lxc";
    public double CpuPercent => Cpu * 100;
    public double MemPercent => MaxMem > 0 ? (double)Mem / MaxMem * 100 : 0;
}
