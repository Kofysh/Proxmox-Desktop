# copy-assets.ps1
# Copie les assets depuis l'ancien projet WinForms vers le nouveau projet WinUI 3.
# A executer UNE SEULE FOIS apres avoir clone la branche feature/winui3-migration.
#
# Usage depuis la racine du repo (3 manieres equivalentes) :
#   .\scripts\copy-assets.ps1          <- appel direct (recommande)
#   & '.\scripts\copy-assets.ps1'      <- appel avec &
#   . '.\scripts\copy-assets.ps1'      <- dot-sourcing (fonctionne aussi)

[CmdletBinding()]
param(
    [string]$SourceRoot,
    [string]$DestRoot
)

$ErrorActionPreference = "Stop"

# $PSScriptRoot est vide en dot-sourcing ; on le reconstruit depuis MyInvocation
if (-not $PSScriptRoot -or $PSScriptRoot -eq '') {
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
} else {
    $ScriptDir = $PSScriptRoot
}

# Si toujours vide (appel interactif rare), on part du repertoire courant
if (-not $ScriptDir -or $ScriptDir -eq '') {
    $ScriptDir = Join-Path (Get-Location).Path 'scripts'
}

# Resoudre les chemins par defaut si non passes en parametre
if (-not $SourceRoot) {
    $SourceRoot = Resolve-Path (Join-Path $ScriptDir '..' 'Resources') -ErrorAction SilentlyContinue
    if (-not $SourceRoot) {
        $SourceRoot = Join-Path $ScriptDir '..' 'Resources'
    }
}
if (-not $DestRoot) {
    $DestRoot = Join-Path $ScriptDir '..' 'src' 'ProxmoxDesktop.App' 'Assets'
}

# Normaliser les chemins (supprime les ..\ pour l'affichage)
$SourceRoot = [System.IO.Path]::GetFullPath($SourceRoot)
$DestRoot   = [System.IO.Path]::GetFullPath($DestRoot)

Write-Host "Source  : $SourceRoot"
Write-Host "Dest    : $DestRoot"
Write-Host ""

if (-not (Test-Path $SourceRoot)) {
    Write-Error "Dossier Resources introuvable : $SourceRoot`nLance ce script depuis la racine du repo."
    exit 1
}

New-Item -ItemType Directory -Force -Path $DestRoot | Out-Null

$copies = @(
    @{ Src = "Icons\default.png";      Dst = "proxmox.png" }
    @{ Src = "Icons\default-icon.ico"; Dst = "app.ico"     }
    @{ Src = "vm_logo.png";            Dst = "vm.png"      }
    @{ Src = "lxc_logo.png";           Dst = "lxc.png"     }
)

$ok = 0
$warn = 0

foreach ($item in $copies) {
    $src = Join-Path $SourceRoot $item.Src
    $dst = Join-Path $DestRoot   $item.Dst

    if (-not (Test-Path $src)) {
        Write-Warning "Source introuvable : $src"
        $warn++
        continue
    }

    Copy-Item -Path $src -Destination $dst -Force
    Write-Host "[OK] $($item.Src) -> Assets/$($item.Dst)" -ForegroundColor Cyan
    $ok++
}

Write-Host ""
if ($warn -gt 0) {
    Write-Warning "$warn fichier(s) manquant(s). Le dossier Resources est-il bien present ?"
}
if ($ok -gt 0) {
    Write-Host "$ok asset(s) copies avec succes dans : $DestRoot" -ForegroundColor Green
    Write-Host "Tu peux maintenant compiler : dotnet build src/ProxmoxDesktop.sln"
}
if ($ok -eq 0) {
    Write-Error "Aucun asset copie. Verifie que le dossier Resources/ est bien present a la racine du repo."
    exit 1
}
