using System.IO;
using System.Text.Json;

namespace ProxmoxDesktop.Config;

public sealed class ConfigurationService
{
    private static readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProxmoxDesktop", "config.json");

    private Dictionary<string, JsonElement> _data = [];

    public ConfigurationService() => Load();

    private void Load()
    {
        try
        {
            if (File.Exists(_path))
                _data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(_path)) ?? [];
        }
        catch { _data = []; }
    }

    public T? Get<T>(string key)
    {
        if (!_data.TryGetValue(key, out var el)) return default;
        try { return el.Deserialize<T>(); } catch { return default; }
    }

    public void Set<T>(string key, T value)
        => _data[key] = JsonSerializer.SerializeToElement(value);

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true }));
    }
}
