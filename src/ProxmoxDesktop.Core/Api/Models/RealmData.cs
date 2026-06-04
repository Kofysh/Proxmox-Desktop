using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

public sealed class RealmData
{
    [JsonPropertyName("realm")]   public string Realm   { get; init; } = string.Empty;
    [JsonPropertyName("comment")] public string? Comment { get; init; }
    [JsonPropertyName("type")]    public string? Type    { get; init; }

    // Affiché dans le ComboBox
    public override string ToString() =>
        string.IsNullOrEmpty(Comment) ? Realm : $"{Realm} — {Comment}";
}
