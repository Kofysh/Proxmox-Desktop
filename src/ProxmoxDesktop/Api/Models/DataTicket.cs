namespace ProxmoxDesktop.Api.Models;

internal sealed class DataTicket
{
    public required string Ticket              { get; init; }
    public required string CsrfPreventionToken { get; init; }
    public required string Username            { get; init; }
}
