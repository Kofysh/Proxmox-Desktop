# Proxmox Desktop

<div align="center">

**Native Windows client for Proxmox VE &mdash; WinUI 3 &middot; Windows App SDK 2.5 &middot; .NET 10 LTS &middot; MVVM**

[![Build](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/build.yml/badge.svg)](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/build.yml)
[![Release](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/release.yml/badge.svg)](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/release.yml)
[![.NET](https://img.shields.io/badge/.NET-10%20LTS-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)]()
[![Proxmox VE](https://img.shields.io/badge/Proxmox-VE-E57000?style=flat-square&logo=proxmox)](https://www.proxmox.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

<br/>

![Dashboard](Screenshots/Capture-2.PNG)

</div>

---

## Overview

**Proxmox Desktop** is a native Windows client for Proxmox VE. It gives quick access to all your virtual machines and LXC containers across a cluster, without going through the WebGUI &mdash; directly from your Windows desktop.

Built with **WinUI 3** and the **Windows App SDK**, using an **MVVM** architecture (CommunityToolkit) and a fully async HTTP client with automatic retry.

---

## Features

- 🖥️ **VM/LXC Dashboard** &mdash; Grid **or list** view of all machines with real-time status, CPU%, RAM%, uptime
- 🌐 **Multi-server** &mdash; Connect several Proxmox clusters at once and see them side by side
- 📊 **Stats bar** &mdash; Total / Running / Stopped / VMs / LXC at a glance
- 🗂️ **Server &amp; node sidebar** &mdash; Filter by cluster or node with a single click
- 🏷️ **Proxmox tags** &mdash; Shown on each card, click a tag to filter
- 🧭 **Sorting** &mdash; By name, VMID, CPU, RAM, status or uptime
- 🕑 **Activity log** &mdash; Side panel recording power actions, state changes and errors
- 🔍 **Search** &mdash; Filter by name, VMID, node or server
- 🖥️ **Integrated console** &mdash; NoVNC, xTermJS and SPICE (Virt-Viewer)
- ⚡ **Power control** &mdash; Start / Shutdown / Reboot / Suspend / Hibernate / Force Stop / Reset
- 🔐 **Dual authentication** &mdash; Classic login + TOTP, or Proxmox API Token
- 🔔 **Windows notifications** &mdash; Native toast on VM state changes
- ⌨️ **Keyboard shortcuts** &mdash; `F5` refresh · `Ctrl+F` search · `Esc` clear
- 🔄 **Auto-refresh** &mdash; Configurable interval, ticket renewed automatically every 90 min
- 🌙 **Dark / Light theme** &mdash; Toggle in one click, remembered between sessions

---

## Screenshots

| Login | Dashboard | VM Card |
|-------|-----------|--------|
| ![Login](Screenshots/Capture-1.PNG) | ![Dashboard](Screenshots/Capture-2.PNG) | ![Card](Screenshots/Capture-3.PNG) |

---

## Requirements

| Component | Version | Link |
|-----------|---------|------|
| Windows | 10 (build 17763+) or 11 | &mdash; |
| .NET | 10 LTS (self-contained) | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Windows App SDK | 2.5.1 (bundled self-contained) | [Documentation](https://learn.microsoft.com/windows/apps/windows-app-sdk/) |
| WebView2 Runtime | Latest *(pre-installed on Windows 11)* | [microsoft.com/edge/webview2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) |
| Virt-Viewer + UsbDk *(SPICE only)* | Latest | [spice-space.org](https://www.spice-space.org/download.html) |

> **.NET runtime is bundled.** No separate .NET installation required &mdash; the app is self-contained.

---

## Installation

### From releases

Choose one of the two Windows packages from the [Releases](../../releases) page:

- **Installer (`*-setup-win-x64.exe`)** — installs Proxmox Desktop for the current Windows user and creates Start Menu/desktop shortcuts.
- **Portable (`*-portable-win-x64.zip`)** — extract it anywhere (including a USB drive) and run `ProxmoxDesktop.exe`; the Windows App SDK runtime is included; no installation or administrator rights are required.

Both packages include the .NET runtime. WebView2 is still required for the integrated web consoles.

### From source

```bash
git clone https://github.com/Kofysh/Proxmox-Desktop.git
cd "Proxmox-Desktop/Proxmox Desktop"
dotnet build -c Release
```

---

## CI / CD

### Build

Every push to `master` and every pull request automatically triggers a build. If it succeeds, a **build artifact** (`ProxmoxDesktop-win-x64`) is uploaded and available for 7 days under the [Actions](../../actions) tab — no tag needed.

### Release

Releases are created three ways:

**Option 1 — Automatic on version bump** *(recommended)*:
Bump `<InformationalVersion>` in `Proxmox Desktop/ProxmoxDesktop.csproj`, commit and merge to `master`. The Release workflow reads the version, and if no `v<version>` tag exists yet, it builds, zips and publishes the GitHub Release automatically. Pushing `master` without changing the version is a no-op, so it never spams releases.

**Option 2 — Git tag** (from your machine):
```bash
git tag v2.2.0
git push origin v2.2.0
```

**Option 3 — Manual trigger** (from GitHub UI):
1. Go to [Actions → Release](../../actions/workflows/release.yml)
2. Click **Run workflow**
3. Enter the tag name (e.g. `v2.2.0`) and confirm

Tags containing `-beta` or `-rc` are automatically marked as pre-releases.

---

## Configuration

On first launch, fill in:

| Field | Description | Example |
|-------|-------------|---------|
| **Server** | Proxmox IP or hostname | `192.168.1.10` |
| **Port** | API port (default 8006) | `8006` |
| **Username** | Proxmox user | `root` |
| **Realm** | Selected from the list | `pam` |
| **Password** | Password + TOTP if enabled | &mdash; |
| **Skip SSL** | For self-signed certificates | &check; homelab |

**Or via API Token:**

```
PVEAPIToken=user@realm!tokenid=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

Credentials are saved to `%AppData%\ProxmoxDesktop\config.json`. The token secret is encrypted using **Windows DPAPI**.

---

## Minimum Proxmox Permissions

For a dedicated account with minimal rights:

| Permission | Usage |
|---|---|
| `VM.Audit` | List and display VMs |
| `VM.Console` | Console access (NoVNC, xTermJS, SPICE) |
| `VM.PowerMgmt` | Power control |

---

## Architecture

```
Proxmox-Desktop/
├── Proxmox Desktop/               # Main WinUI 3 / Windows App SDK project
│   ├── Api/
│   │   ├── IApiClient.cs          # Interface (mockable for tests)
│   │   ├── ApiClient.cs           # Async HTTP client + retry
│   │   ├── ServerInfo.cs          # Connection record
│   │   ├── Internal/              # Internal PVE response types
│   │   └── Models/                # MachineData, NodeData, LoginResult...
│   ├── Config/
│   │   ├── AppConfig.cs           # Strongly-typed config
│   │   └── ConfigurationService.cs # JSON persistence + DPAPI
│   ├── Console/
│   │   └── SpiceLauncher.cs       # Virt-Viewer launcher
│   ├── Converters/                # StatusToBrush, BytesToReadable...
│   ├── Services/
│   │   └── NotificationService.cs # Native Windows Toast
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
├── installer/
├── ProxmoxDesktop.sln
└── .github/workflows/
    ├── build.yml                  # Build + artifact on every push / PR
    └── release.yml                # Installer + portable release on v*.*.* tag or manual trigger
```

**Tech stack:**
- UI: WinUI 3 + [Windows App SDK 2.5.1](https://learn.microsoft.com/windows/apps/windows-app-sdk/)
- Architecture: MVVM &mdash; [CommunityToolkit.Mvvm 8.4.2](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- Web console: WebView2
- HTTP: `HttpClient` fully async/await + retry
- Notifications: `Microsoft.Toolkit.Uwp.Notifications`
- Serialization: `System.Text.Json`
- Secrets: Windows DPAPI

---

## Roadmap

- [x] Proxmox API Token support
- [x] CPU% / RAM% metrics on each card
- [x] Filter / search by name or VMID
- [x] Group by node
- [x] Windows Toast notifications
- [x] Dark / Light theme
- [x] Multi-server support (multiple Proxmox clusters)
- [x] List view in addition to grid view
- [x] Proxmox tags displayed on cards
- [x] Action history / logs
- [ ] Bulk power actions on multiple machines
- [ ] Per-VM resource graphs (history)

---

## Contributing

Contributions are welcome. To propose a feature or report a bug:

1. Open an [Issue](../../issues)
2. Fork the repo and create a `feature/name` or `fix/name` branch
3. Submit a Pull Request targeting `master`

---

## Credits

Forked and rewritten from [sakakun/Proxmox-Desktop](https://github.com/sakakun/Proxmox-Desktop) (original WinForms app by Matthew Bate).

---

## License

[MIT](LICENSE)
