#!/usr/bin/env bash
# copy-assets.sh
# Equivalent bash de copy-assets.ps1 (pour WSL ou CI Linux).
# Usage depuis la racine du repo :
#   bash scripts/copy-assets.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")"; pwd)"
SOURCE_ROOT="$SCRIPT_DIR/../Resources"
DEST_ROOT="$SCRIPT_DIR/../src/ProxmoxDesktop.App/Assets"

mkdir -p "$DEST_ROOT"

copy_asset() {
  local src="$SOURCE_ROOT/$1"
  local dst="$DEST_ROOT/$2"
  if [ ! -f "$src" ]; then
    echo "[WARN] Source introuvable : $src"
    return
  fi
  cp "$src" "$dst"
  echo "[OK] $1 -> Assets/$2"
}

copy_asset "Icons/default.png"     "proxmox.png"
copy_asset "Icons/default-icon.ico" "app.ico"
copy_asset "vm_logo.png"            "vm.png"
copy_asset "lxc_logo.png"           "lxc.png"

echo ""
echo "Assets copiés avec succès dans : $DEST_ROOT"
echo "Tu peux maintenant compiler avec : dotnet build src/ProxmoxDesktop.sln"
