using System.IO;
using System.Text.Json;

namespace ProxmoxDesktop.Config;

/// <summary>
/// Persists <see cref="AppConfig"/> to %AppData%/ProxmoxDesktop/config.json.
/// Sensitive values (token secret) are stored via Windows DPAPI.
/// </summary>
public sealed class ConfigurationService
{
    private static readonly string ConfigDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProxmoxDesktop");
    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public AppConfig Config { get; private set; } = new();

    public ConfigurationService() => Load();

    public void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
                Config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigPath)) ?? new();
        }
        catch { Config = new(); }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(Config, _opts));
        }
        catch { /* non-critical */ }
    }

    /// <summary>Protects <paramref name="plaintext"/> using Windows DPAPI (current user scope).</summary>
    public static string? ProtectSecret(string? plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return null;
        var bytes     = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var encrypted = System.Security.Cryptography.ProtectedData.Protect(
            bytes, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>Decrypts a DPAPI-protected value.</summary>
    public static string? UnprotectSecret(string? ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return null;
        try
        {
            var bytes     = Convert.FromBase64String(ciphertext);
            var decrypted = System.Security.Cryptography.ProtectedData.Unprotect(
                bytes, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return System.Text.Encoding.UTF8.GetString(decrypted);
        }
        catch { return null; }
    }
}
