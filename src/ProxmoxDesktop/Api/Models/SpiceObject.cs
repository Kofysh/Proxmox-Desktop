using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Api.Models;

public sealed class SpiceObject
{
    [JsonPropertyName("type")]         public string? Type        { get; init; }
    [JsonPropertyName("host")]         public string? Host        { get; init; }
    [JsonPropertyName("tls-port")]     public string? TlsPort     { get; init; }
    [JsonPropertyName("password")]     public string? Password    { get; init; }
    [JsonPropertyName("ca")]           public string? Ca          { get; init; }
    [JsonPropertyName("host-subject")] public string? HostSubject { get; init; }
    [JsonPropertyName("proxy")]        public string? Proxy       { get; init; }
    [JsonPropertyName("title")]        public string? Title       { get; init; }
}
