#!/bin/bash
# clean.sh - Cleans build outputs and/or artifacts
# Usage: ./scripts/clean.sh [--artifacts-only]
#   No args: cleans bin/, obj/, and all artifacts
#   --artifacts-only: cleans only artifacts (preserves bin/obj)
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
lib_init

ARTIFACTS_ONLY=false
if [ "${1:-}" = "--artifacts-only" ]; then
    ARTIFACTS_ONLY=true
fi

if [ "$ARTIFACTS_ONLY" = false ]; then
    echo "Cleaning bin/ and obj/ directories..."
    find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true
    echo "Done: Cleaned bin/ and obj/ directories"
fi

echo "Cleaning artifacts and temporary files..."
rm -rf artifacts/
rm -rf TestResults/
rm -rf StrykerOutput/
rm -rf coverage/
rm -rf mutation-reports/
echo "Done: Cleaned all artifacts"
