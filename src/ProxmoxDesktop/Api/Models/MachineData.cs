using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Api.Models;

public sealed record MachineData
{
    [JsonPropertyName("vmid")]   public int    Vmid   { get; init; }
    [JsonPropertyName("name")]   public string Name   { get; init; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; init; } = string.Empty;
    [JsonPropertyName("node")]   public string Node   { get; init; } = string.Empty;
    [JsonPropertyName("cpu")]    public double Cpu    { get; init; }
    [JsonPropertyName("mem")]    public long   Mem    { get; init; }
    [JsonPropertyName("maxmem")] public long   MaxMem { get; init; }
    [JsonPropertyName("lock")]   public string? Lock  { get; init; }
    [JsonPropertyName("serial")] public int    Serial { get; init; }

    public string NodeName { get; init; } = string.Empty;
    public string Type     { get; init; } = string.Empty;

    [JsonIgnore] public bool   IsRunning   => Status == "running";
    [JsonIgnore] public bool   IsLxc       => Type   == "lxc";
    [JsonIgnore] public double CpuPercent  => Math.Round(Cpu * 100, 1);
    [JsonIgnore] public double MemPercent  => MaxMem > 0 ? Math.Round((double)Mem / MaxMem * 100, 1) : 0;
}
