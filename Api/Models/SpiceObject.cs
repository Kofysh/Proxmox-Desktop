using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Api.Models;

public sealed class SpiceObject
{
    [JsonPropertyName("host")]     public string? Host     { get; init; }
    [JsonPropertyName("port")]     public string? Port     { get; init; }
    [JsonPropertyName("password")] public string? Password { get; init; }
    [JsonPropertyName("tls-port")] public string? TlsPort  { get; init; }
    [JsonPropertyName("ca")]       public string? Ca       { get; init; }
    [JsonPropertyName("proxy")]    public string? Proxy    { get; init; }
}
