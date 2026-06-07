namespace ProxmoxDesktop.Api.Models;

public sealed class LoginResult
{
    public bool    IsSuccess  { get; private init; }
    public bool    NeedsTotp  { get; private init; }
    public string? Message    { get; private init; }

    public static LoginResult Success()                => new() { IsSuccess = true };
    public static LoginResult TotpRequired()           => new() { NeedsTotp = true };
    public static LoginResult Failure(string message)  => new() { Message   = message };
}
