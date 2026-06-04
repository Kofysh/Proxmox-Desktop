using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Internal;

/// <summary>Enveloppe générique des réponses PVE : { "data": T }</summary>
internal sealed class PveResponse<T>
{
    [JsonPropertyName("data")] public T? Data { get; init; }
}

/// <summary>Enveloppe quand data est un tableau.</summary>
internal sealed class PveListResponse<T>
{
    [JsonPropertyName("data")] public List<T>? Data { get; init; }
}

/// <summary>Réponse de /access/ticket</summary>
internal sealed class TicketResponse
{
    [JsonPropertyName("ticket")]              public string? Ticket              { get; init; }
    [JsonPropertyName("CSRFPreventionToken")] public string? CsrfPreventionToken { get; init; }
    [JsonPropertyName("username")]            public string? Username            { get; init; }
}
