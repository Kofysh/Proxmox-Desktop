using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

public class RealmData
{
    [JsonPropertyName("realm")]
    public string Realm { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
