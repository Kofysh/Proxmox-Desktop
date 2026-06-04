using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

public sealed class SpiceObject
{
    [JsonPropertyName("host")]             public string Host            { get; init; } = string.Empty;
    [JsonPropertyName("tls-port")]         public int    TlsPort         { get; init; }
    [JsonPropertyName("password")]         public string Password        { get; init; } = string.Empty;
    [JsonPropertyName("ca")]               public string? Ca             { get; init; }
    [JsonPropertyName("host-subject")]     public string? HostSubject    { get; init; }
    [JsonPropertyName("proxy")]            public string? Proxy          { get; init; }
    [JsonPropertyName("title")]            public string? Title          { get; init; }
    [JsonPropertyName("type")]             public string? Type           { get; init; }
    [JsonPropertyName("toggle-fullscreen")]public string? ToggleFullscreen{ get; init; }
    [JsonPropertyName("release-cursor")]   public string? ReleaseCursor  { get; init; }
    [JsonPropertyName("secure-attention")] public string? SecureAttention{ get; init; }
    [JsonPropertyName("delete-this-file")] public int    DeleteThisFile  { get; init; }
}
