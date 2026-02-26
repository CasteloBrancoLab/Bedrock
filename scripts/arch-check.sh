#!/bin/bash
# arch-check.sh - Runs architecture tests and extracts violations to artifacts/pending/
# Usage: ./scripts/arch-check.sh
# Output: artifacts/pending/architecture_*.txt (if violations)
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== ARCHITECTURE CHECK ==="

mkdir -p artifacts/pending
rm -f artifacts/pending/architecture_*.txt

# Build silently (architecture tests use --no-build)
echo "Building solution..."
dotnet build > /dev/null 2>&1

# Run architecture tests (writes violations to artifacts/pending/architecture_*.txt)
ARCH_FAILED=0
"$SCRIPT_DIR/architecture.sh" > /dev/null 2>&1 || ARCH_FAILED=1

"$SCRIPT_DIR/generate-pending-summary.sh" 2>/dev/null || true

if [ "$ARCH_FAILED" -eq 1 ]; then
    VIOLATION_COUNT=$(find artifacts/pending -name "architecture_*.txt" 2>/dev/null | wc -l)
    VIOLATION_COUNT=${VIOLATION_COUNT//[^0-9]/}
    echo "ARCHITECTURE: FAILED ($VIOLATION_COUNT violations)"
    exit 1
fi

echo "ARCHITECTURE: SUCCESS"
exit 0
