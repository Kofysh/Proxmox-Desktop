namespace ProxmoxDesktop.Api.Models;

public sealed class LoginResult
{
    public static LoginResult Success()           => new() { IsSuccess = true };
    public static LoginResult TotpRequired()      => new() { NeedsTotp = true, Message = "TOTP code required." };
    public static LoginResult Failure(string msg) => new() { Message = msg };

    public bool   IsSuccess { get; private init; }
    public bool   NeedsTotp { get; private init; }
    public string Message   { get; private init; } = string.Empty;
}
