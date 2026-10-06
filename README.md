# Proxmox Desktop

<div align="center">

**A polished native Windows client for Proxmox VE**

[![Build](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/build.yml/badge.svg)](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/build.yml)
[![Release](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/release.yml/badge.svg)](https://github.com/Kofysh/Proxmox-Desktop/actions/workflows/release.yml)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Windows](https://img.shields.io/badge/Windows-10%2B-0078D4?style=flat-square&logo=windows)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

</div>

## Overview

**Proxmox Desktop** is a native WPF application for managing Proxmox VE infrastructure from a clean, focused Windows interface. It brings the most common administration tasks together in one dashboard for virtual machines, LXC containers, nodes, storage, tasks and consoles.

The project is built with **.NET 10**, **WPF**, **WPF-UI**, **MVVM**, **WebView2** and the Proxmox VE REST API. The interface is designed for fast daily administration while keeping authentication and local secret storage aligned with Windows capabilities.

## Highlights

- Modern dark Windows interface inspired by professional desktop utilities.
- Native Proxmox VE authentication with password, TOTP and API tokens.
- Server endpoint validation before credentials are submitted.
- VM and LXC overview with status, node, VMID and resource information.
- Quick power actions: start, stop, restart, suspend, resume and reset.
- Integrated NoVNC and xterm.js web consoles through WebView2.
- SPICE console support through Virt-Viewer.
- Activity logging and visible connection diagnostics.
- Dedicated application User-Agent and standard API request headers.
- Windows DPAPI protection for locally stored secrets.
- Self-contained Windows installer; no separate .NET installation is required.

## Screens and navigation

The application is organized around the main Proxmox administration areas:

| Area | Purpose |
| --- | --- |
| Overview | Infrastructure summary and recent activity |
| Virtual Machines | VM status, details and power actions |
| Containers | LXC status, details and power actions |
| Nodes | Cluster node information |
| Storage | Storage overview and availability |
| Network | Network-related infrastructure views |
| Backups | Backup-related administration |
| Tasks | Recent and active Proxmox tasks |
| Users | User and access administration |

## Download and installation

Download the latest installer from the [Releases page](../../releases):

```text
ProxmoxDesktop-<version>-setup-win-x64.exe
```

The installer is self-contained and installs the Windows application for the current user. Current releases distribute the installer only; no portable package is included.

### Requirements

- Windows 10 version 1809 (build 17763) or Windows 11.
- 64-bit Windows.
- Network access to the Proxmox VE API.
- WebView2 Runtime for integrated web consoles.
- Virt-Viewer and UsbDk for SPICE consoles.

## Authentication

### Username and password

Enter the Proxmox server address, port, realm, username and password. The default Proxmox API port is `8006`. If TOTP is enabled, the application requests the one-time code during login.

### API token

Use the Proxmox token identifier format:

```text
user@realm!tokenid
```

The token secret is never stored as plain text. Locally persisted secrets are protected with Windows DPAPI.

### Reverse proxies

The application connects directly to the Proxmox API using native HTTP requests. A reverse proxy must forward the Proxmox API routes, including:

```text
/api2/json/version
/api2/json/access/domains
/api2/json/access/ticket
/api2/json/*
```

If a proxy requires an external browser session or an interactive access gateway, native Proxmox credentials alone cannot bypass that policy. For the most reliable setup, expose the Proxmox API through a route that allows native API authentication.

## Minimal permissions

A dedicated Proxmox account commonly needs the following permissions:

| Permission | Purpose |
| --- | --- |
| `VM.Audit` | List and inspect virtual machines and containers |
| `VM.Console` | Open VM and container consoles |
| `VM.PowerMgmt` | Perform power operations |

Add node, storage, task or user permissions according to the administration features required by your deployment.

## Build from source

From the repository root in PowerShell:

```powershell
dotnet restore "Proxmox Desktop\ProxmoxDesktop.csproj"
dotnet build "Proxmox Desktop\ProxmoxDesktop.csproj" `
  --configuration Release `
  -p:Platform=x64
```

To publish a self-contained Windows build:

```powershell
dotnet publish "Proxmox Desktop\ProxmoxDesktop.csproj" `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishReadyToRun=true `
  -o output\publish
```

## Project structure

```text
Proxmox Desktop/
├── Api/                  HTTP client, API models and responses
├── Config/               JSON configuration and DPAPI protection
├── Converters/           WPF value converters
├── Services/             Notifications and activity logging
├── ViewModels/           MVVM presentation logic
├── Views/                WPF windows and application screens
└── ProxmoxDesktop.csproj
```

### Main technologies

- .NET 10
- WPF
- WPF-UI 2.1.0
- CommunityToolkit.Mvvm 8.4.2
- Microsoft.Web.WebView2 1.0.4258.31
- System.Text.Json
- Windows DPAPI

## CI/CD

- Every push to `master` and every pull request can run the validation build.
- Versioned tags in the form `vX.Y.Z` trigger the release workflow.
- The release workflow publishes a self-contained x64 Windows installer.
- Release notes are generated in English with installation, requirements, security and changelog sections.
- The release workflow uses .NET 10 and Node.js 24-compatible GitHub Actions.

## Roadmap

- [x] Modern WPF dashboard
- [x] Password, TOTP and API token authentication
- [x] Server endpoint validation
- [x] VM and LXC overview
- [x] Search, filtering and sorting foundations
- [x] Power management actions
- [x] NoVNC, xterm.js and SPICE console support
- [x] Windows installer
- [ ] Bulk actions across multiple machines
- [ ] Historical resource charts
- [ ] Expanded storage, network and user administration

## Contributing

Bug reports, feature requests and pull requests are welcome. Please open an [issue](../../issues) before proposing a large change, then create a focused branch and submit a pull request to `master`.

Use clear English commit messages in the following style:

```text
Add native API request headers
Fix login endpoint validation
Improve release documentation
```

## License

This project is licensed under the [MIT License](LICENSE).
