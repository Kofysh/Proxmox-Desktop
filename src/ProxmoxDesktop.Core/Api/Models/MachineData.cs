using System.Text.Json.Serialization;

namespace ProxmoxDesktop.Core.Api.Models;

/// <summary>
/// Représente une VM QEMU ou un container LXC retourné par l'API PVE.
/// Déclaré en tant que record pour autoriser l'expression 'with' dans ApiClient.
/// </summary>
public sealed record MachineData
{
    [JsonPropertyName("vmid")]   public int    Vmid   { get; init; }
    [JsonPropertyName("name")]   public string Name   { get; init; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; init; } = string.Empty;
    [JsonPropertyName("node")]   public string Node   { get; init; } = string.Empty;

    /// <summary>CPU usage (0.0 – 1.0) retourné par l'API.</summary>
    [JsonPropertyName("cpu")]    public double Cpu    { get; init; }

    /// <summary>RAM utilisée en octets.</summary>
    [JsonPropertyName("mem")]    public long   Mem    { get; init; }

    /// <summary>RAM totale allouée en octets.</summary>
    [JsonPropertyName("maxmem")] public long   MaxMem { get; init; }

    // -------------------------------------------------------------------------
    // Champs injectés après désérialisation (absents de la réponse list PVE)
    // -------------------------------------------------------------------------

    /// <summary>Nom du nœud hôte (injecté par ApiClient).</summary>
    public string NodeName { get; init; } = string.Empty;

    /// <summary>"qemu" ou "lxc" (injecté par ApiClient).</summary>
    public string Type { get; init; } = string.Empty;

    // -------------------------------------------------------------------------
    // Propriétés calculées
    // -------------------------------------------------------------------------

    /// <summary>True si la VM/CT est en cours d'exécution.</summary>
    [JsonIgnore] public bool IsRunning  => Status == "running";

    /// <summary>True si c'est un container LXC (et non une VM QEMU).</summary>
    [JsonIgnore] public bool IsLxc      => Type   == "lxc";

    /// <summary>CPU en pourcentage (0–100).</summary>
    [JsonIgnore] public double CpuPercent => Math.Round(Cpu * 100, 1);

    /// <summary>RAM utilisée en pourcentage (0–100).</summary>
    [JsonIgnore] public double MemPercent =>
        MaxMem > 0 ? Math.Round((double)Mem / MaxMem * 100, 1) : 0;
}
