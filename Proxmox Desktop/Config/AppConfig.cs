namespace ProxmoxDesktop.Config;

/// <summary>Strongly-typed app configuration — persisted as JSON.</summary>
public sealed class AppConfig
{
    public string Server      { get; set; } = string.Empty;
    public string Port        { get; set; } = "8006";
    public bool   SkipSsl     { get; set; }
    public bool   UseApiToken { get; set; }
    public string Username    { get; set; } = string.Empty;
    public string TokenId     { get; set; } = string.Empty;
    public bool   IsDarkTheme { get; set; } = true;
    public int    RefreshIntervalSeconds { get; set; } = 60;
}
