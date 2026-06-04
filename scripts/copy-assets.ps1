# copy-assets.ps1
# Copie les assets depuis Resources/ vers src/ProxmoxDesktop.App/Assets/
#
# Usage (toutes les formes fonctionnent) :
#   .\scripts\copy-assets.ps1
#   & '.\scripts\copy-assets.ps1'
#   . '.\scripts\copy-assets.ps1'

$ErrorActionPreference = "Stop"

# Retrouver le dossier du script de facon robuste (appel direct ET dot-sourcing)
$_scriptPath = if ($MyInvocation.MyCommand.Path) {
    $MyInvocation.MyCommand.Path
} elseif ($PSCommandPath) {
    $PSCommandPath
} else {
    Join-Path (Get-Location).Path 'scripts\copy-assets.ps1'
}

$_repoRoot  = [System.IO.Path]::GetFullPath((Join-Path (Split-Path $_scriptPath) '..'))
$SourceRoot = Join-Path $_repoRoot 'Resources'
$DestRoot   = Join-Path $_repoRoot 'src' 'ProxmoxDesktop.App' 'Assets'

Write-Host "Repo    : $_repoRoot"
Write-Host "Source  : $SourceRoot"
Write-Host "Dest    : $DestRoot"
Write-Host ""

if (-not (Test-Path $SourceRoot)) {
    Write-Error "Dossier Resources introuvable : $SourceRoot`nVerifie que tu es bien dans la racine du repo."
    return
}

New-Item -ItemType Directory -Force -Path $DestRoot | Out-Null

$copies = @(
    @{ Src = "Icons\default.png";      Dst = "proxmox.png" }
    @{ Src = "Icons\default-icon.ico"; Dst = "app.ico"     }
    @{ Src = "vm_logo.png";            Dst = "vm.png"      }
    @{ Src = "lxc_logo.png";           Dst = "lxc.png"     }
)

$ok   = 0
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
    Write-Warning "$warn fichier(s) manquant(s)."
}
if ($ok -gt 0) {
    Write-Host "$ok asset(s) copies avec succes dans : $DestRoot" -ForegroundColor Green
    Write-Host "Lance maintenant : dotnet build src/ProxmoxDesktop.sln"
} else {
    Write-Error "Aucun asset copie. Verifie que Resources/ est bien present."
}
