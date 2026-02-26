#!/bin/bash
# pipeline-check.sh - Wrapper around pipeline.sh that suppresses verbose output
# Usage: ./scripts/pipeline-check.sh
# Output: artifacts/pending/SUMMARY.txt (consolidated)
#         artifacts/summary.json
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== PIPELINE CHECK ==="

LOG_FILE=$(mktemp)
trap 'rm -f "$LOG_FILE"' EXIT

echo "Running full pipeline..."
if "$SCRIPT_DIR/pipeline.sh" > "$LOG_FILE" 2>&1; then
    echo "PIPELINE: SUCCESS"
    echo ""
    cat artifacts/pending/SUMMARY.txt 2>/dev/null || echo "No pending items."
    exit 0
fi

echo "PIPELINE: FAILED"
echo ""
cat artifacts/pending/SUMMARY.txt 2>/dev/null || echo "Check artifacts/summary.json for details."
exit 1
