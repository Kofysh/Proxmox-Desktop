using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Api.Internal;

internal sealed class PveResponse<T>
{
    [JsonPropertyName("data")] public T? Data { get; init; }
}

internal sealed class PveListResponse<T>
{
    [JsonPropertyName("data")] public List<T>? Data { get; init; }
}

internal sealed class TicketResponse
{
    [JsonPropertyName("ticket")]              public string? Ticket              { get; init; }
    [JsonPropertyName("CSRFPreventionToken")] public string? CsrfPreventionToken { get; init; }
    [JsonPropertyName("username")]            public string? Username            { get; init; }
}
