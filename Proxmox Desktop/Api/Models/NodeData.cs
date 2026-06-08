using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Api.Models;

public record NodeData
{
    [JsonPropertyName("node")]    public string Node   { get; init; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; init; } = string.Empty;
    [JsonPropertyName("cpu")]    public double Cpu    { get; init; }
    [JsonPropertyName("mem")]    public long   Mem    { get; init; }
    [JsonPropertyName("maxmem")] public long   MaxMem { get; init; }
    [JsonPropertyName("uptime")] public long   Uptime { get; init; }

    public bool IsOnline => Status == "online";
}
