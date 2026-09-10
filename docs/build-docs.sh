#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MDWEB_CLI="${MDWEB_CLI:-$ROOT/../MDWeb/src/MDWeb.Cli}"
DOC_DIR="$ROOT/docs"
BUILD_DIR="$DOC_DIR/_build"

if [ ! -e "$MDWEB_CLI" ]; then
  echo "MDWeb CLI not found at: $MDWEB_CLI" >&2
  echo "Clone MDWeb as a sibling repo or set MDWEB_CLI to your MDWeb.Cli project path." >&2
  exit 1
fi

dotnet run --project "$MDWEB_CLI" -- \
  --source "$DOC_DIR/pages" \
  --output "$BUILD_DIR" \
  --theme "$DOC_DIR/theme" \
  --title "SkyUI" \
  --description "Cross-platform UI library for .NET on Avalonia" \
  --footer "<p>SkyUI documentation · <a href=\"https://github.com/hoihky/skyui\">GitHub</a> · Generated with <a href=\"https://github.com/hoihky/MDWeb\">MDWeb</a></p>"

for html in "$BUILD_DIR"/*.html; do
  [ -f "$html" ] || continue
  name="$(basename "$html")"
  if [ "$name" != "index.html" ]; then
    cp "$html" "$DOC_DIR/$name"
  fi
done

echo "Built HTML pages in $DOC_DIR"
python3 "$DOC_DIR/build-index.py"
