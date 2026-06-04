using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

public sealed class NodeData
{
    [JsonPropertyName("node")]            public string Node           { get; init; } = string.Empty;
    [JsonPropertyName("status")]          public string Status         { get; init; } = string.Empty;
    [JsonPropertyName("type")]            public string Type           { get; init; } = string.Empty;
    [JsonPropertyName("level")]           public string? Level         { get; init; }
    [JsonPropertyName("id")]              public string? Id            { get; init; }
    [JsonPropertyName("ssl_fingerprint")] public string? SslFingerprint{ get; init; }
}
