using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

public class RootObject<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];
}
