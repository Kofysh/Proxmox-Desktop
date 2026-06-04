using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProxmoxDesktop.Core.Config;

/// <summary>
/// Service de configuration persisté dans un fichier JSON local.
/// Remplace l'ancienne classe Configurations qui sauvegardait à chaque SetSetting().
/// Sauvegarde uniquement lors d'un appel explicite à Save().
/// Le mot de passe n'est JAMAIS stocké.
/// </summary>
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
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                _data = JsonNode.Parse(json) as JsonObject ?? new JsonObject();
            }
            else
            {
                _data = new JsonObject();
            }
        }
        catch { _data = new JsonObject(); }
    }

    /// <summary>Lit une valeur de configuration. Retourne la valeur par défaut si absente.</summary>
    public T? Get<T>(string key)
    {
        if (!_data.TryGetPropertyValue(key, out var node) || node is null)
            return default;
        try   { return node.GetValue<T>(); }
        catch { return default; }
    }

    /// <summary>Définit une valeur. La sauvegarde n'est effectuée qu'après Save().</summary>
    public void Set<T>(string key, T value)
    {
        _data[key] = JsonValue.Create(value);
        _dirty     = true;
    }

    /// <summary>Persiste la configuration sur disque (appel explicite uniquement).</summary>
    public void Save()
    {
        if (!_dirty) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            var json = _data.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
            _dirty = false;
        }
        catch { /* log silencieux — ne pas planter l'UI pour un échec de sauvegarde */ }
    }
}
