using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

public class DataTicket
{
    [JsonPropertyName("ticket")]
    public string Ticket { get; set; } = string.Empty;

    [JsonPropertyName("CSRFPreventionToken")]
    public string CsrfPreventionToken { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    public bool RequiresTotp => Ticket.Contains("PVE:!tfa!");
}

public class LoginResponse
{
    [JsonPropertyName("data")]
    public DataTicket? Data { get; set; }
}
