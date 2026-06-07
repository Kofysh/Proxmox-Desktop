using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Api.Models;

public sealed class RealmData
{
    [JsonPropertyName("realm")]   public string Realm   { get; init; } = string.Empty;
    [JsonPropertyName("type")]    public string Type    { get; init; } = string.Empty;
    [JsonPropertyName("comment")] public string Comment { get; init; } = string.Empty;
}
