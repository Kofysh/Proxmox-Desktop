# Proxmox Desktop

<div align="center">

**Client Windows natif pour Proxmox VE — WinUI 3 · .NET 10 · MVVM**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?style=flat-square&logo=windows)](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
[![Proxmox VE](https://img.shields.io/badge/Proxmox-VE-E57000?style=flat-square&logo=proxmox)](https://www.proxmox.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

</div>

---

## Présentation

**Proxmox Desktop** est un client Windows natif pour Proxmox VE, conçu pour les homelab et environnements professionnels. Il permet d'accéder rapidement aux machines virtuelles et conteneurs sans passer par le WebGUI Proxmox — directement depuis le bureau Windows.

Refactorisé en **WinUI 3** avec une architecture **MVVM** et un client API entièrement **async/await**, l'application est fluide, réactive et prête pour les futures évolutions.

---

## Fonctionnalités

- **Tableau de bord VM/LXC** — Affichage en tuiles de toutes les machines du cluster avec statut en temps réel
- **Accès console intégré** — NoVNC, xTermJS et SPICE (Virt-Viewer) dans l'ordre de préférence
- **WebGUI intégré** — Panneau WebView2 avec auto-login via le même token API
- **Contrôle d'alimentation** — Start / Stop / Reboot / Shutdown sur chaque VM
- **Authentification** — Login classique et TOTP supportés
- **Rafraîchissement automatique** — Toutes les 60 secondes, et 5 secondes après un changement d'état
- **Proxy SPICE configurable** — Possibilité de spécifier un proxy SPICE alternatif

---

## Prérequis

| Composant | Version | Lien |
|-----------|---------|------|
| Windows | 10 (19041+) ou 11 | — |
| .NET | 10.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Windows App SDK | 2.x | [Installeur Microsoft](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads) |
| WebView2 Runtime | Dernière version | [microsoft.com/edge/webview2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) |
| Virt-Viewer + UsbDk *(SPICE)* | Dernière version | [spice-space.org](https://www.spice-space.org/download.html) |

---

## Installation

### Depuis les releases

1. Télécharger le dernier `.zip` depuis la page [Releases](../../releases)
2. Extraire et lancer `ProxmoxDesktop.exe`
3. Installer le **Windows App SDK** si ce n'est pas déjà fait (voir prérequis)

### Depuis les sources

```bash
git clone https://github.com/Kofysh/Proxmox-Desktop.git
cd Proxmox-Desktop/src
dotnet build -c Release
```

> **Note** : Le projet cible `net10.0-windows10.0.19041.0` en mode **unpackaged** (pas de MSIX requis).

---

## Configuration

Au premier lancement, renseigner :

- **Adresse du serveur** — URL de Proxmox VE (ex: `https://192.168.1.10:8006`)
- **Nom d'utilisateur** — Format `user@realm` (ex: `root@pam`)
- **Mot de passe** — Ou code TOTP si activé
- **Ignorer le certificat SSL** — Option disponible pour les certificats auto-signés (homelab)

---

## Permissions Proxmox minimales

Pour un compte dédié avec le minimum de droits nécessaires :

| Permission | Usage |
|---|---|
| `VM.Audit` | Lister et afficher les VMs |
| `VM.Console` | Accès console (NoVNC, xTermJS, SPICE) |
| `VM.PowerMgmt` | Contrôle d'alimentation |

---

## Architecture

```
Proxmox-Desktop/
├── src/
│   ├── ProxmoxDesktop.App/       # Application WinUI 3 (UI, ViewModels, Pages)
│   │   ├── Pages/                # LoginPage, MainPage, ConsolePage
│   │   ├── ViewModels/           # MVVM — CommunityToolkit.Mvvm
│   │   └── Program.cs            # Entry point (STAThread + Bootstrap)
│   └── ProxmoxDesktop.Core/      # Logique métier indépendante de l'UI
│       ├── Api/                  # ApiClient async (HttpClient)
│       ├── Models/               # MachineData (record), PowerResult
│       └── Services/             # Configurations, helpers
└── assets/                       # Icônes et ressources visuelles
```

**Stack technique :**
- UI : WinUI 3 (Windows App SDK 2.x)
- Architecture : MVVM — [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- Console web : WebView2
- HTTP : `HttpClient` entièrement async/await
- Sérialisation : `System.Text.Json`

---

## Problèmes connus

Consulter la section [Issues](../../issues) pour la liste complète.

**Crash au démarrage (`E_FAIL 0x80004005 / combase.dll`)** — Résolu dans la version actuelle. Causé par un `Package.appxmanifest` présent en mode unpackaged. S'assurer que le Windows App SDK est correctement installé.

---

## Feuille de route

- [ ] Support des **tokens API Proxmox** (`PVEAPIToken: user@realm!tokenid=uuid`)
- [ ] Affichage des **métriques ressources** (CPU%, RAM) sur chaque tuile
- [ ] **Filtrage/recherche** des VMs par nom ou VMID
- [ ] **Regroupement par nœud** pour les clusters multi-nœuds
- [ ] **Notifications Windows** lors des changements d'état
- [ ] Mode **sombre/clair** configurable
- [ ] Support **multi-serveurs** (plusieurs clusters Proxmox)

---

## Contribution

Les contributions sont les bienvenues. Pour proposer une fonctionnalité ou signaler un bug :

1. Ouvrir une [Issue](../../issues) en décrivant le problème ou la proposition
2. Fork le repo et créer une branche `feature/nom-feature` ou `fix/nom-fix`
3. Soumettre une Pull Request vers `master`

---

## Crédits

Forké et refactorisé depuis [sakakun/Proxmox-Desktop](https://github.com/sakakun/Proxmox-Desktop) (WinForms original par Matthew Bate).

---

## Licence

[MIT](LICENSE)
