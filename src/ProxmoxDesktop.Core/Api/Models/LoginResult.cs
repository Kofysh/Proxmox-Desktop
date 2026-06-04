namespace ProxmoxDesktop.Core.Api.Models;

/// <summary>
/// Résultat d'une tentative de connexion.
/// Utilise un discriminated-union léger via les propriétés booléennes.
/// </summary>
public sealed class LoginResult
{
    public static LoginResult Success()             => new() { IsSuccess = true };
    public static LoginResult TotpRequired()        => new() { NeedsTotp = true, Message = "TOTP requis." };
    public static LoginResult Failure(string msg)   => new() { Message = msg };

    public bool   IsSuccess { get; private init; }
    public bool   NeedsTotp { get; private init; }
    public string Message   { get; private init; } = string.Empty;
}
