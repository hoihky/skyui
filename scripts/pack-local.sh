#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/artifacts/packages"
CONFIG="${1:-Release}"

mkdir -p "$OUT"

pack() {
  echo "Packing $1..."
  dotnet pack "$ROOT/$1" -c "$CONFIG" -o "$OUT" \
    /p:Version=0.1.0-local \
    /p:PackageVersion=0.1.0-local \
    /p:ContinuousIntegrationBuild=true
}

pack src/SkyUI.Core/SkyUI.Core.csproj
pack src/SkyUI.Fonts/SkyUI.Fonts.csproj
pack src/SkyUI.Icons/SkyUI.Icons.csproj
pack src/SkyUI/SkyUI.csproj
pack src/SkyUI.Themes.Sky/SkyUI.Themes.Sky.csproj
pack src/SkyUI.Data/SkyUI.Data.csproj
pack src/SkyUI.Diagram/SkyUI.Diagram.csproj

echo "Local packages written to $OUT (version 0.1.0-local)"
