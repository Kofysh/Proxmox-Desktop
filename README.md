# Proxmox Desktop

<div align="center">

**Client Windows natif et moderne pour Proxmox VE**

[![Build](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/build.yml/badge.svg)](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/build.yml)
[![Release](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/release.yml/badge.svg)](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/release.yml)
[![.NET](https://img.shields.io/badge/.NET-10%20LTS-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![WinUI](https://img.shields.io/badge/WinUI-3-0078D4?style=flat-square&logo=windows)](https://learn.microsoft.com/windows/apps/winui/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

</div>

## Présentation

**Proxmox Desktop** est une application Windows native pour administrer les machines virtuelles et conteneurs LXC d'un ou plusieurs clusters Proxmox VE.

L'interface est construite avec **WinUI 3** et **Windows App SDK 2.5.1**. Le projet utilise **.NET 10 LTS**, le pattern MVVM et un client HTTP asynchrone avec renouvellement automatique des tickets Proxmox.

La navigation prépare les principaux domaines de Proxmox VE : vue d'ensemble, machines virtuelles, conteneurs LXC, nœuds, stockage, réseau, sauvegardes, tâches, journaux et gestion des utilisateurs. Les écrans d'administration seront ajoutés progressivement avec leurs endpoints API dédiés.

## Fonctionnalités

- Dashboard WinUI 3 avec cartes VM/LXC et vue liste.
- Recherche par nom, VMID, serveur ou nœud.
- Filtres par serveur, nœud et tags Proxmox.
- Statistiques globales : total, actifs, arrêtés, VMs et conteneurs.
- Tri par nom, VMID, CPU, mémoire, statut ou uptime.
- Connexion multi-serveurs.
- Actions de contrôle : démarrer, arrêter, redémarrer, suspendre, reprendre, hiberner et réinitialiser.
- Console intégrée NoVNC et xTermJS via WebView2.
- Console SPICE via Virt-Viewer.
- Authentification par mot de passe + TOTP ou par API Token.
- Notifications Windows lors des changements d'état.
- Journal d'activité et messages d'erreur visibles.
- Thème clair/sombre mémorisé.
- Publication autonome pour une utilisation installée ou portable.

## Configuration requise

| Composant | Version |
|---|---|
| Windows | Windows 10 build 17763+ ou Windows 11 |
| .NET SDK pour compiler | 10.0.100 ou version 10.0.x plus récente |
| Windows App SDK | 2.5.1, inclus dans l'application publiée |
| WebView2 Runtime | Version stable récente, généralement déjà installé sur Windows 11 |
| Microsoft Visual C++ Redistributable x64 | Requis par Windows App SDK ; inclus dans l'installateur |
| Virt-Viewer + UsbDk | Requis uniquement pour SPICE |

Les versions publiées sont autonomes : l'utilisateur n'a pas besoin d'installer le runtime .NET ou Windows App SDK. L'installateur et l'exécutable portable installent automatiquement le runtime Microsoft Visual C++ x64 si nécessaire. La version portable est distribuée sous la forme d'un exécutable auto-extractible unique : lancez-le directement, sans décompresser de fichiers. Elle utilise un dossier temporaire supprimé à la fermeture. WebView2 reste nécessaire pour les consoles web. En cas d'échec, le diagnostic est écrit dans `%LOCALAPPDATA%\ProxmoxDesktop\startup.log`.

## Installation

Depuis la page [Releases](../../releases), choisissez :

- **Installateur** `ProxmoxDesktop-*-setup-win-x64.exe` : installation par utilisateur, sans droits administrateur, avec raccourcis Windows.
- **Portable** `ProxmoxDesktop-*-portable-win-x64.zip` : extraction dans n'importe quel dossier ou sur une clé USB, puis lancement de `ProxmoxDesktop.exe`.

La configuration est enregistrée dans `%AppData%\ProxmoxDesktop\config.json`. Les secrets d'API sont protégés avec Windows DPAPI.

## Compiler depuis les sources

Depuis PowerShell à la racine du dépôt :

```powershell
dotnet restore "Proxmox Desktop\ProxmoxDesktop.csproj"
dotnet build "Proxmox Desktop\ProxmoxDesktop.csproj" `
  --configuration Release `
  -p:Platform=x64
```

Publier une version autonome portable :

```powershell
dotnet publish "Proxmox Desktop\ProxmoxDesktop.csproj" `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:WindowsAppSDKSelfContained=true `
  -p:PublishReadyToRun=true `
  -o output\portable
```

## Authentification Proxmox

### Mot de passe

Renseignez le serveur, le port `8006`, le realm, l'utilisateur et le mot de passe. Si le TOTP est activé, le code est demandé après la première tentative.

### API Token

Utilisez un identifiant au format :

```text
user@realm!tokenid
```

Le secret du token n'est jamais enregistré en clair.

## Permissions minimales

Pour un compte dédié, les permissions suivantes sont généralement suffisantes :

| Permission | Utilisation |
|---|---|
| `VM.Audit` | Lister et afficher les machines |
| `VM.Console` | Ouvrir les consoles |
| `VM.PowerMgmt` | Contrôler l'alimentation |

## Architecture

```text
Proxmox Desktop/
├── Api/                  Client Proxmox, réponses et modèles
├── Config/               Configuration JSON et chiffrement DPAPI
├── Console/              Lancement de Virt-Viewer/SPICE
├── Converters/           Convertisseurs WinUI
├── Services/             Notifications et journal d'activité
├── ViewModels/           Logique MVVM
├── Views/                Fenêtres WinUI 3
└── ProxmoxDesktop.csproj
```

Technologies principales :

- WinUI 3 et Windows App SDK 2.5.1
- .NET 10 LTS
- CommunityToolkit.Mvvm 8.4.2
- WebView2 1.0.4258.31
- System.Text.Json
- Windows DPAPI

## CI/CD

- Chaque push sur `master` et chaque pull request lance une compilation Release.
- Le workflow `release.yml` génère un ZIP portable et un installateur Inno Setup.
- Une nouvelle release peut être créée par tag `vX.Y.Z`, changement de version ou déclenchement manuel.
- Les tags contenant `-beta` ou `-rc` produisent une préversion.

## Roadmap

- [x] Dashboard VM/LXC WinUI 3
- [x] API Token et TOTP
- [x] Multi-serveurs
- [x] Recherche, filtres et tri
- [x] Contrôle d'alimentation
- [x] Consoles NoVNC, xTermJS et SPICE
- [x] Notifications Windows
- [x] Mode installé et portable
- [ ] Actions groupées sur plusieurs machines
- [ ] Graphiques historiques par VM

## Contribution

Les contributions et rapports de bugs sont les bienvenus. Ouvrez une [issue](../../issues), créez une branche dédiée, puis soumettez une pull request vers `master`.

## Licence

Ce projet est distribué sous licence [MIT](LICENSE).
