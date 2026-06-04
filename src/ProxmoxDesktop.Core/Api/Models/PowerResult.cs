namespace ProxmoxDesktop.Core.Api.Models;

/// <summary>Résultat d'une action d'alimentation sur une VM/CT.</summary>
public enum PowerResult
{
    /// <summary>Commande acceptée par Proxmox VE.</summary>
    Ok,

    /// <summary>Refusé : droits insuffisants (HTTP 403).</summary>
    Forbidden,

    /// <summary>Erreur inattendue (timeout, nœud hors-ligne, etc.).</summary>
    Error
}
