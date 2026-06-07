using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProxmoxDesktop.Config;

public sealed class ConfigurationService
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProxmoxDesktop", "config.json");

    private readonly JsonObject _data;
    private bool _dirty;

    public ConfigurationService()
    {
        try
        {
            _data = File.Exists(ConfigPath)
                ? JsonNode.Parse(File.ReadAllText(ConfigPath)) as JsonObject ?? new JsonObject()
                : new JsonObject();
        }
        catch { _data = new JsonObject(); }
    }

    public T? Get<T>(string key)
    {
        if (!_data.TryGetPropertyValue(key, out var node) || node is null) return default;
        try   { return node.GetValue<T>(); }
        catch { return default; }
    }

    public void Set<T>(string key, T value) { _data[key] = JsonValue.Create(value); _dirty = true; }

    public void Save()
    {
        if (!_dirty) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            File.WriteAllText(ConfigPath, _data.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            _dirty = false;
        }
        catch { }
    }
}
