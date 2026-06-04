namespace ProxmoxDesktop.Core.Api;

public enum LoginStatus { Success, Failure, TotpRequired }

public record LoginResult(LoginStatus Status, string? Message = null)
{
    public bool IsSuccess      => Status == LoginStatus.Success;
    public bool NeedsTotp      => Status == LoginStatus.TotpRequired;

    public static LoginResult Success()            => new(LoginStatus.Success);
    public static LoginResult Failure(string msg)  => new(LoginStatus.Failure, msg);
    public static LoginResult TotpRequired()       => new(LoginStatus.TotpRequired, "Code TOTP requis.");
}

public enum PowerResult { Success, Forbidden, Error }
