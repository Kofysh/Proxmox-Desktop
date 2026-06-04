namespace ProxmoxDesktop.Core.Api.Models;

/// <summary>
/// Ticket d'authentification PVE retourné par /access/ticket.
/// Seul le ticket et le CSRF sont conservés — le mot de passe n'est JAMAIS stocké ici.
/// </summary>
public sealed class DataTicket
{
    public string Ticket              { get; init; } = string.Empty;
    public string CsrfPreventionToken { get; init; } = string.Empty;
    public string Username            { get; init; } = string.Empty;
}
