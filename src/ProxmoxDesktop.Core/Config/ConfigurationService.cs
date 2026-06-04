using System.Text.Json;

namespace ProxmoxDesktop.Core.Config;

/// <summary>
/// Service de configuration avec sauvegarde différée (lazy).
/// Remplace l'ancienne classe Configurations qui sauvegardait à chaque SetSetting().
/// </summary>
public class ConfigurationService
{
    private Dictionary<string, object?> _settings = [];
    private readonly string _filePath;
    private bool _isDirty;

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public ConfigurationService(string appName = "ProxmoxDesktopClient")
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            appName);
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");
        Load();
    }

    public T? Get<T>(string key, T? defaultValue = default)
    {
        if (!_settings.TryGetValue(key, out var raw)) return defaultValue;
        try
        {
            if (raw is JsonElement el)
                return el.Deserialize<T>(_jsonOptions) ?? defaultValue;
            return (T?)raw ?? defaultValue;
        }
        catch { return defaultValue; }
    }

    public void Set(string key, object? value)
    {
        _settings[key] = value;
        _isDirty = true;
    }

    /// <summary>Persiste les paramètres si des modifications sont en attente.</summary>
    public void SaveIfDirty()
    {
        if (!_isDirty) return;
        var json = JsonSerializer.Serialize(_settings, _jsonOptions);
        File.WriteAllText(_filePath, json);
        _isDirty = false;
    }

    /// <summary>Force la sauvegarde immédiate.</summary>
    public void Save()
    {
        var json = JsonSerializer.Serialize(_settings, _jsonOptions);
        File.WriteAllText(_filePath, json);
        _isDirty = false;
    }

    private void Load()
    {
        if (!File.Exists(_filePath)) return;
        var json = File.ReadAllText(_filePath);
        _settings = JsonSerializer.Deserialize<Dictionary<string, object?>>(_filePath, _jsonOptions) ?? [];
    }
}
