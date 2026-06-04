# copy-assets.ps1
# Copie les assets depuis l'ancien projet WinForms vers le nouveau projet WinUI 3.
# A executer UNE SEULE FOIS apres avoir clone la branche feature/winui3-migration.
#
# Usage depuis la racine du repo :
#   .\scripts\copy-assets.ps1

[CmdletBinding()]
param(
    [string]$SourceRoot = $PSScriptRoot + "\..\Resources",
    [string]$DestRoot   = $PSScriptRoot + "\..\src\ProxmoxDesktop.App\Assets"
)

$ErrorActionPreference = "Stop"

# Creer le dossier de destination s'il n'existe pas
New-Item -ItemType Directory -Force -Path $DestRoot | Out-Null

$copies = @(
    # Source (relatif a $SourceRoot)          -> Destination (relatif a $DestRoot)
    @{ Src = "Icons\default.png";    Dst = "proxmox.png" }
    @{ Src = "Icons\default-icon.ico"; Dst = "app.ico"   }
    @{ Src = "vm_logo.png";          Dst = "vm.png"      }
    @{ Src = "lxc_logo.png";         Dst = "lxc.png"     }
)

foreach ($item in $copies) {
    $src = Join-Path $SourceRoot $item.Src
    $dst = Join-Path $DestRoot   $item.Dst

    if (-not (Test-Path $src)) {
        Write-Warning "Source introuvable : $src"
        continue
    }

    Copy-Item -Path $src -Destination $dst -Force
    Write-Host "[OK] $($item.Src) -> Assets/$($item.Dst)"
}

Write-Host ""
Write-Host "Assets copies avec succes dans : $DestRoot" -ForegroundColor Green
Write-Host "Tu peux maintenant compiler le projet avec Visual Studio ou 'dotnet build'."
