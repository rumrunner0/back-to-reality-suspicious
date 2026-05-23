#!/usr/bin/env zsh

set -euo pipefail

SCRIPT_DIRECTORY=$(dirname "$(readlink -f "$0")")

cd "$SCRIPT_DIRECTORY" || { echo "Failed to cd to $SCRIPT_DIRECTORY" >&2; exit 1; }
echo "Script directory: $PWD"

cd ".."
echo "Working directory: $PWD"

CONFIGURATION="Release" \

dotnet pack --configuration ${CONFIGURATION}