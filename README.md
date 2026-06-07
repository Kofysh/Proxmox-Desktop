# Proxmox Desktop

<div align="center">

**Native Windows client for Proxmox VE — WinUI 3 · .NET 10 · MVVM**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?style=flat-square&logo=windows)](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
[![Proxmox VE](https://img.shields.io/badge/Proxmox-VE-E57000?style=flat-square&logo=proxmox)](https://www.proxmox.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

</div>

---

## Overview

**Proxmox Desktop** is a native Windows client for Proxmox VE, designed for homelabs and professional environments. It provides quick access to virtual machines and containers without going through the Proxmox WebGUI — directly from your Windows desktop.

Rewritten in **WinUI 3** with an **MVVM** architecture and a fully **async/await** API client, the application is smooth, responsive, and ready for future improvements.

---

## Features

- **VM/LXC Dashboard** — Tile-based view of all cluster machines with real-time status
- **Integrated Console** — NoVNC, xTermJS and SPICE (Virt-Viewer) in order of preference
- **Integrated WebGUI** — WebView2 panel with auto-login via the same API token
- **Power Control** — Start / Stop / Reboot / Shutdown on each VM
- **Authentication** — Classic login and TOTP supported
- **Auto-refresh** — Every 60 seconds, and 5 seconds after a state change
- **Configurable SPICE proxy** — Ability to specify an alternative SPICE proxy

---

## Requirements

| Component | Version | Link |
|-----------|---------|------|
| Windows | 10 (19041+) or 11 | — |
| .NET | 10.0 (self-contained, bundled) | — |
| WebView2 Runtime | Latest *(pre-installed on Windows 11)* | [microsoft.com/edge/webview2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) |
| Virt-Viewer + UsbDk *(SPICE only)* | Latest | [spice-space.org](https://www.spice-space.org/download.html) |

> **No Windows App SDK installation required.** All WinUI 3 runtime components are bundled inside the ZIP.

---

## Installation

### From releases

1. Download the latest `.zip` from the [Releases](../../releases) page
2. Extract anywhere and run `ProxmoxDesktop.exe`
3. That's it — no prerequisites needed

### From source

```bash
git clone https://github.com/Kofysh/Proxmox-Desktop.git
cd Proxmox-Desktop/src
dotnet build -c Release
```

> **Note:** The project targets `net10.0-windows10.0.19041.0` in **unpackaged** mode (no MSIX required) with `WindowsAppSDKSelfContained=true`.

---

## Configuration

On first launch, fill in:

- **Server address** — Proxmox VE URL (e.g. `https://192.168.1.10:8006`)
- **Username** — Format `user@realm` (e.g. `root@pam`)
- **Password** — Or TOTP code if enabled
- **Ignore SSL certificate** — Available option for self-signed certificates (homelab)

---

## Minimum Proxmox Permissions

For a dedicated account with the minimum required rights:

| Permission | Usage |
|---|---|
| `VM.Audit` | List and display VMs |
| `VM.Console` | Console access (NoVNC, xTermJS, SPICE) |
| `VM.PowerMgmt` | Power control |

---

## Architecture

```
Proxmox-Desktop/
├── src/
│   ├── ProxmoxDesktop.App/       # WinUI 3 application (UI, ViewModels, Pages)
│   │   ├── Pages/                # LoginPage, MainPage, ConsolePage
│   │   ├── ViewModels/           # MVVM — CommunityToolkit.Mvvm
│   │   └── Program.cs            # Entry point (STAThread + Bootstrap)
│   └── ProxmoxDesktop.Core/      # UI-independent business logic
│       ├── Api/                  # Async ApiClient (HttpClient)
│       ├── Models/               # MachineData (record), PowerResult
│       └── Services/             # Configuration, helpers
└── assets/                       # Icons and visual resources
```

**Tech stack:**
- UI: WinUI 3 (Windows App SDK 2.x, self-contained)
- Architecture: MVVM — [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- Web console: WebView2
- HTTP: `HttpClient` fully async/await
- Serialization: `System.Text.Json`

---

## Known Issues

See the [Issues](../../issues) section for the full list.

**Startup crash (`E_FAIL 0x80004005 / combase.dll`)** — Fixed in the current version. Was caused by a `Package.appxmanifest` present in unpackaged mode. Resolved by switching to `WindowsAppSDKSelfContained=true`.

---

## Roadmap

- [ ] **Proxmox API token** support (`PVEAPIToken: user@realm!tokenid=uuid`)
- [ ] **Resource metrics** display (CPU%, RAM) on each tile
- [ ] **VM filtering/search** by name or VMID
- [ ] **Group by node** for multi-node clusters
- [ ] **Windows notifications** on state changes
- [ ] Configurable **dark/light mode**
- [ ] **Multi-server** support (multiple Proxmox clusters)

---

## Contributing

Contributions are welcome. To propose a feature or report a bug:

1. Open an [Issue](../../issues) describing the problem or proposal
2. Fork the repo and create a `feature/feature-name` or `fix/fix-name` branch
3. Submit a Pull Request targeting `master`

---

## Credits

Forked and rewritten from [sakakun/Proxmox-Desktop](https://github.com/sakakun/Proxmox-Desktop) (original WinForms app by Matthew Bate).

---

## License

[MIT](LICENSE)
