#!/bin/bash
# build-check.sh - Compiles solution and extracts build errors to artifacts/pending/
# Usage: ./scripts/build-check.sh
# Output: artifacts/pending/build_errors.txt (if errors)
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== BUILD CHECK ==="

mkdir -p artifacts/pending
rm -f artifacts/pending/build_*.txt

LOG_FILE=$(mktemp)
trap 'rm -f "$LOG_FILE"' EXIT

echo "Building solution..."
if dotnet build > "$LOG_FILE" 2>&1; then
    echo "BUILD: SUCCESS"
    "$SCRIPT_DIR/generate-pending-summary.sh" 2>/dev/null || true
    exit 0
fi

# Build failed - extract errors from MSBuild output
# Format: path(line,col): error CODE: message [project]
echo "BUILD: FAILED"

ERROR_COUNT=0
{
    while IFS= read -r line; do
        ERROR_COUNT=$((ERROR_COUNT + 1))
        echo "--- ERROR $ERROR_COUNT ---"

        FILE=$(echo "$line" | sed -n 's/^\(.*\)([0-9]*,[0-9]*): error .*/\1/p')
        LINE_NUM=$(echo "$line" | sed -n 's/.*(\([0-9]*\),[0-9]*): error .*/\1/p')
        CODE=$(echo "$line" | sed -n 's/.*: error \([A-Za-z]*[0-9]*\): .*/\1/p')
        MSG=$(echo "$line" | sed -n 's/.*: error [A-Za-z]*[0-9]*: \(.*\) \[.*/\1/p')
        if [ -z "$MSG" ]; then
            MSG=$(echo "$line" | sed -n 's/.*: error [A-Za-z]*[0-9]*: \(.*\)/\1/p')
        fi
        PROJECT=$(echo "$line" | sed -n 's/.*\[\(.*\)\]/\1/p')

        [ -n "$FILE" ] && echo "FILE: $FILE"
        [ -n "$LINE_NUM" ] && echo "LINE: $LINE_NUM"
        [ -n "$CODE" ] && echo "CODE: $CODE"
        [ -n "$MSG" ] && echo "MESSAGE: $MSG"
        [ -n "$PROJECT" ] && echo "PROJECT: $(basename "${PROJECT%.csproj}")"
        echo ""
    done < <(grep ': error ' "$LOG_FILE" | grep -v "^Build FAILED" || true)

    echo "TOTAL_ERRORS: $ERROR_COUNT"
} > artifacts/pending/build_errors.txt

"$SCRIPT_DIR/generate-pending-summary.sh" 2>/dev/null || true
echo "Errors: $ERROR_COUNT (see artifacts/pending/build_errors.txt)"
exit 1
