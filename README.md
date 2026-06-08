# Proxmox Desktop

<div align="center">

**Native Windows client for Proxmox VE &mdash; WPF &middot; .NET 9 &middot; Material Design &middot; MVVM**

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

## Overview

**Proxmox Desktop** is a native Windows client for Proxmox VE. It gives quick access to all your virtual machines and LXC containers across a cluster, without going through the WebGUI &mdash; directly from your Windows desktop.

Built with **WPF** and **Material Design**, using an **MVVM** architecture (CommunityToolkit) and a fully async HTTP client with automatic retry.

---

## Features

- 🖥️ **VM/LXC Dashboard** &mdash; Grid view of all machines with real-time status, CPU%, RAM%, uptime
- 📊 **Stats bar** &mdash; Total / Running / Stopped / VMs / LXC at a glance
- 🗂️ **Node sidebar** &mdash; Filter by node with a single click
- 🔍 **Search** &mdash; Filter by name, VMID or node
- 🖥️ **Integrated console** &mdash; NoVNC, xTermJS and SPICE (Virt-Viewer)
- ⚡ **Power control** &mdash; Start / Shutdown / Reboot / Suspend / Hibernate / Force Stop / Reset
- 🔐 **Dual authentication** &mdash; Classic login + TOTP, or Proxmox API Token
- 🔔 **Windows notifications** &mdash; Native toast on VM state changes
- 🔄 **Auto-refresh** &mdash; Every 60 seconds, ticket renewed automatically every 90 min
- 🌙 **Dark / Light theme** &mdash; Toggle in one click

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
| WebView2 Runtime | Latest *(pre-installed on Windows 11)* | [microsoft.com/edge/webview2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) |
| Virt-Viewer + UsbDk *(SPICE only)* | Latest | [spice-space.org](https://www.spice-space.org/download.html) |

> **.NET runtime is bundled.** No separate .NET installation required &mdash; the app is self-contained.

---

## Installation

### From releases

1. Download the latest `.zip` from the [Releases](../../releases) page
2. Extract anywhere
3. Run `ProxmoxDesktop.exe`

### From source

```bash
git clone https://github.com/Kofysh/Proxmox-Desktop.git
cd "Proxmox-Desktop/Proxmox Desktop"
dotnet build -c Release
```

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
├── Proxmox Desktop/               # Main WPF project
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
│   ├── Controls/
│   │   ├── MachineCard.xaml       # VM/LXC card
│   │   └── StatsBar.xaml          # Dashboard stats bar
│   ├── Converters/                # StatusToBrush, BytesToReadable...
│   ├── Helpers/                   # VisualTreeHelperExtensions
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
├── ProxmoxDesktop.sln
└── .github/workflows/
    ├── build.yml                  # Build on every push / PR
    └── release.yml                # Release on v* tag
```

**Tech stack:**
- UI: WPF + [MaterialDesignThemes 5.1](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- Architecture: MVVM &mdash; [CommunityToolkit.Mvvm 8.4](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
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
- [ ] Multi-server support (multiple Proxmox clusters)
- [ ] List view in addition to grid view
- [ ] Proxmox tags displayed on cards
- [ ] Action history / logs

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
