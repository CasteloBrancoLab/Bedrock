#!/bin/bash
# Compila a solução

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== BUILD ==="

echo "Restoring dependencies..."
dotnet restore

echo "Building solution..."
dotnet build --no-restore

echo "Done: Build completed successfully"
