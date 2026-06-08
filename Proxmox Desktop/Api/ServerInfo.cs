namespace ProxmoxDesktop.Api;

/// <summary>Immutable connection info for a Proxmox VE server.</summary>
public sealed record ServerInfo(
    string Host,
    int    Port               = 8006,
    bool   SkipSslVerification = false);
