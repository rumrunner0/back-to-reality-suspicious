#!/bin/zsh

set -euo pipefail

SCRIPT_DIR=${0:A:h}

cd "$SCRIPT_DIR"
echo "Script directory: $PWD"

cd ".."
echo "Working directory: $PWD"

CONFIGURATION="Release"

dotnet pack --configuration ${CONFIGURATION}