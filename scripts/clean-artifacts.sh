#!/bin/bash
# Limpa todos os artefatos gerados pela pipeline

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "Cleaning artifacts..."

# Remove generated artifacts
rm -rf artifacts/
rm -rf TestResults/
rm -rf StrykerOutput/
rm -rf coverage/
rm -rf mutation-reports/

echo "Done: Cleaned all artifacts"
