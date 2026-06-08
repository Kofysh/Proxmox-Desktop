using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Api.Models;

public record NodeData
{
    [JsonPropertyName("node")]   public string Node   { get; init; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; init; } = string.Empty;
}
