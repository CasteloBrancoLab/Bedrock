#!/bin/bash
# Limpa todas as pastas bin/ e obj/ de forma recursiva

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "Cleaning bin/ and obj/ directories..."

# Remove bin/ and obj/ directories recursively
find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true

echo "Done: Cleaned bin/ and obj/ directories"
