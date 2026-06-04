# copy-assets.ps1
# Copie les assets depuis Resources/ vers src/ProxmoxDesktop.App/Assets/
#
# Compatible PowerShell 5.1 et PowerShell 7+
# Usage (toutes les formes fonctionnent) :
#   .\scripts\copy-assets.ps1
#   & '.\scripts\copy-assets.ps1'
#   . '.\scripts\copy-assets.ps1'

$ErrorActionPreference = "Stop"

# Retrouver le dossier du script (robuste en appel direct ET dot-sourcing)
$_scriptPath = if ($MyInvocation.MyCommand.Path) {
    $MyInvocation.MyCommand.Path
} elseif ($PSCommandPath) {
    $PSCommandPath
} else {
    [IO.Path]::Combine((Get-Location).Path, 'scripts', 'copy-assets.ps1')
}

$_repoRoot  = [IO.Path]::GetFullPath([IO.Path]::Combine((Split-Path $_scriptPath), '..'))
$SourceRoot = [IO.Path]::Combine($_repoRoot, 'Resources')
$DestRoot   = [IO.Path]::Combine($_repoRoot, 'src', 'ProxmoxDesktop.App', 'Assets')

Write-Host "Repo    : $_repoRoot"
Write-Host "Source  : $SourceRoot"
Write-Host "Dest    : $DestRoot"
Write-Host ""

if (-not (Test-Path $SourceRoot)) {
    Write-Error "Dossier Resources introuvable : $SourceRoot`nVerifie que tu lances le script depuis la racine du repo."
    return
}

New-Item -ItemType Directory -Force -Path $DestRoot | Out-Null

$copies = @(
    @{ Src = [IO.Path]::Combine('Icons', 'default.png');      Dst = 'proxmox.png' }
    @{ Src = [IO.Path]::Combine('Icons', 'default-icon.ico'); Dst = 'app.ico'     }
    @{ Src = 'vm_logo.png';                                   Dst = 'vm.png'      }
    @{ Src = 'lxc_logo.png';                                  Dst = 'lxc.png'     }
)

$ok   = 0
$warn = 0

foreach ($item in $copies) {
    $src = [IO.Path]::Combine($SourceRoot, $item.Src)
    $dst = [IO.Path]::Combine($DestRoot,   $item.Dst)

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
    Write-Error "Aucun asset copie. Verifie que Resources/ est bien present a la racine du repo."
}
