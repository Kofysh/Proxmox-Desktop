# Proxmox Desktop

<div align="center">

**Client Windows natif pour Proxmox VE — WPF · .NET 9 · Material Design · MVVM**

[![Build](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/build.yml/badge.svg)](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/build.yml)
[![Release](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/release.yml/badge.svg)](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/release.yml)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)]()
[![Proxmox VE](https://img.shields.io/badge/Proxmox-VE-E57000?style=flat-square&logo=proxmox)](https://www.proxmox.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

<br/>

![Dashboard](Screenshots/Capture-2.PNG)

</div>

---

## Vue d'ensemble

**Proxmox Desktop** est un client Windows natif pour Proxmox VE. Il donne un accès rapide à toutes vos machines virtuelles et conteneurs LXC d'un cluster, sans passer par le WebGUI — directement depuis votre bureau Windows.

Basé sur **WPF** avec **Material Design**, une architecture **MVVM** (CommunityToolkit), et un client HTTP entièrement async avec retry automatique.

---

## Fonctionnalités

- 🖥️ **Dashboard VM/LXC** — Vue en grille de toutes les machines avec statut temps réel, CPU%, RAM%, uptime
- 📊 **Barre de stats** — Total / Running / Stopped / VMs / LXC en un coup d'œil
- 🗂️ **Sidebar nodes** — Filtrage par node d'un clic
- 🔍 **Recherche** — Filtrage par nom, VMID ou node
- 🖥️ **Console intégrée** — NoVNC, xTermJS et SPICE (Virt-Viewer)
- ⚡ **Contrôle d'alimentation** — Start / Shutdown / Reboot / Suspend / Hibernate / Force Stop / Reset
- 🔐 **Double authentification** — Login classique + TOTP, ou API Token Proxmox
- 🔔 **Notifications Windows** — Toast natif lors des changements d'état VM
- 🔄 **Auto-refresh** — Toutes les 60 secondes, ticket renouvelé automatiquement toutes les 90 min
- 🌙 **Thème sombre/clair** — Toggle en un clic

---

## Captures d'écran

| Login | Dashboard | Carte VM |
|-------|-----------|----------|
| ![Login](Screenshots/Capture-1.PNG) | ![Dashboard](Screenshots/Capture-2.PNG) | ![Card](Screenshots/Capture-3.PNG) |

---

## Prérequis

| Composant | Version | Lien |
|-----------|---------|------|
| Windows | 10 (build 17763+) ou 11 | — |
| WebView2 Runtime | Dernière *(pré-installé sur Win 11)* | [microsoft.com/edge/webview2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) |
| Virt-Viewer + UsbDk *(SPICE uniquement)* | Dernière | [spice-space.org](https://www.spice-space.org/download.html) |

> **Aucune installation de .NET requise.** L'application est self-contained (runtime inclus dans le ZIP).

---

## Installation

### Depuis les releases

1. Télécharger le dernier `.zip` depuis la page [Releases](../../releases)
2. Extraire n'importe où
3. Lancer `ProxmoxDesktop.exe`

### Depuis les sources

```bash
git clone https://github.com/Kofysh/Proxmox-Desktop.git
cd "Proxmox-Desktop/Proxmox Desktop"
dotnet build -c Release
```

---

## Configuration

Au premier lancement, renseigner :

| Champ | Description | Exemple |
|-------|-------------|---------|
| **Serveur** | IP ou hostname Proxmox | `192.168.1.10` |
| **Port** | Port API (défaut 8006) | `8006` |
| **Username** | Utilisateur Proxmox | `root` |
| **Realm** | Sélectionné depuis la liste | `pam` |
| **Password** | Mot de passe + TOTP si activé | — |
| **Ignorer SSL** | Pour les certificats auto-signés | ✓ homelab |

**Ou via API Token :**

```
PVEAPIToken=user@realm!tokenid=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

Les identifiants sont sauvegardés dans `%AppData%\ProxmoxDesktop\config.json`. Le secret du token est chiffré via **Windows DPAPI**.

---

## Permissions Proxmox minimales

Pour un compte dédié avec les droits minimaux :

| Permission | Usage |
|---|---|
| `VM.Audit` | Lister et afficher les VMs |
| `VM.Console` | Accès console (NoVNC, xTermJS, SPICE) |
| `VM.PowerMgmt` | Contrôle d'alimentation |

---

## Architecture

```
Proxmox-Desktop/
├── Proxmox Desktop/               # Projet WPF principal
│   ├── Api/
│   │   ├── IApiClient.cs          # Interface (testable)
│   │   ├── ApiClient.cs           # Client HTTP async + retry
│   │   ├── ServerInfo.cs          # Record de connexion
│   │   ├── Internal/              # Types internes PVE
│   │   └── Models/                # MachineData, NodeData, LoginResult…
│   ├── Config/
│   │   ├── AppConfig.cs           # Config typée
│   │   └── ConfigurationService.cs # Persistance JSON + DPAPI
│   ├── Console/
│   │   └── SpiceLauncher.cs       # Lancement Virt-Viewer
│   ├── Controls/
│   │   ├── MachineCard.xaml       # Carte VM/LXC
│   │   └── StatsBar.xaml          # Barre de stats dashboard
│   ├── Converters/                # StatusToBrush, BytesToReadable…
│   ├── Helpers/                   # VisualTreeHelperExtensions
│   ├── Services/
│   │   └── NotificationService.cs # Windows Toast natif
│   ├── ViewModels/
│   │   ├── LoginViewModel.cs
│   │   └── MainViewModel.cs
│   ├── Views/
│   │   ├── LoginWindow.xaml
│   │   ├── MainWindow.xaml
│   │   └── ConsoleWindow.xaml
│   ├── App.xaml
│   └── ProxmoxDesktop.csproj
├── Screenshots/
├── Resources/
├── ProxmoxDesktop.sln
└── .github/workflows/
    ├── build.yml                  # Build sur chaque push/PR
    └── release.yml                # Release sur tag v*
```

**Stack technique :**
- UI : WPF + [MaterialDesignThemes 5.1](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- Architecture : MVVM — [CommunityToolkit.Mvvm 8.4](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- Web console : WebView2
- HTTP : `HttpClient` fully async/await + retry
- Notifications : `Microsoft.Toolkit.Uwp.Notifications`
- Sérialisation : `System.Text.Json`
- Secrets : Windows DPAPI

---

## Roadmap

- [x] API Token Proxmox
- [x] Métriques CPU% / RAM% sur chaque carte
- [x] Filtrage/recherche par nom ou VMID
- [x] Regroupement par node
- [x] Notifications Windows Toast
- [x] Thème sombre/clair
- [ ] Support multi-serveur (plusieurs clusters Proxmox)
- [ ] Vue liste en plus de la vue grille
- [ ] Tags Proxmox affichés sur les cartes
- [ ] Logs / historique des actions

---

## Contribuer

Les contributions sont les bienvenues. Pour proposer une fonctionnalité ou signaler un bug :

1. Ouvrir une [Issue](../../issues)
2. Forker le repo et créer une branche `feature/nom` ou `fix/nom`
3. Soumettre une Pull Request vers `master`

---

## Crédits

Forké et réécrit depuis [sakakun/Proxmox-Desktop](https://github.com/sakakun/Proxmox-Desktop) (app WinForms originale par Matthew Bate).

---

## Licence

[MIT](LICENSE)
