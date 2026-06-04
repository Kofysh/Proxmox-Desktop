using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

public class NodeData
{
    [JsonPropertyName("level")]
    public string? Level { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("node")]
    public string Node { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("ssl_fingerprint")]
    public string? SslFingerprint { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
