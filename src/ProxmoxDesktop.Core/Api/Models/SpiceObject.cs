using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

public class SpiceObject
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("proxy")]
    public string? Proxy { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("ca")]
    public string? Ca { get; set; }

    [JsonPropertyName("host-subject")]
    public string? HostSubject { get; set; }

    [JsonPropertyName("tls-port")]
    public int TlsPort { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("host")]
    public string? Host { get; set; }

    [JsonPropertyName("delete-this-file")]
    public int DeleteThisFile { get; set; }
}

public class SpiceResponse
{
    [JsonPropertyName("data")]
    public SpiceObject? Data { get; set; }
}
