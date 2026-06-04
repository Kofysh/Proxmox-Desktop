# Assets — ProxmoxDesktop.App

Ce dossier contient les images utilisées par l'interface WinUI 3.
Les fichiers binaires (PNG, ICO) ne sont **pas** commités dans Git
pour éviter de dupliquer les ressources déjà présentes dans `/Resources/`.

## Copier les assets

Exécute **une seule fois** depuis la racine du repo :

**Windows (PowerShell) :**
```powershell
.\scripts\copy-assets.ps1
```

**Linux / WSL (Bash) :**
```bash
bash scripts/copy-assets.sh
```

## Fichiers attendus

| Fichier | Source originale | Rôle |
|---|---|---|
| `proxmox.png` | `Resources/Icons/default.png` | Logo Proxmox (header, splash) |
| `vm.png` | `Resources/vm_logo.png` | Icône des VM QEMU sur les tiles |
| `lxc.png` | `Resources/lxc_logo.png` | Icône des containers LXC sur les tiles |
| `app.ico` | `Resources/Icons/default-icon.ico` | Icône de l'application (barre des tâches) |
