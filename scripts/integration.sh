#!/bin/bash
# integration.sh - Runs integration tests and extracts failures to artifacts/pending/
# Usage: ./scripts/integration.sh [project.csproj]
#   No args: runs all integration test projects under tests/IntegrationTests/
#   With arg: runs only the specified test project
# Output: artifacts/pending/integration_<project>_<NNN>.txt (per failure)
#         artifacts/pending/SUMMARY.txt
# Requires: Docker (Testcontainers)
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
lib_init

echo "=== INTEGRATION ==="

mkdir -p artifacts/test-results
clean_pending "integration"

# Determine projects to test
if [ -n "${1:-}" ]; then
    PROJECTS="$1"
else
    PROJECTS=$(find tests/IntegrationTests -name "*.csproj" 2>/dev/null)
fi

if [ -z "$PROJECTS" ]; then
    echo "No integration test projects found"
    "$SCRIPT_DIR/summarize.sh" 2>/dev/null || true
    exit 0
fi

OVERALL_FAILED=0

for project in $PROJECTS; do
    short_name=$(derive_short_name "$project" "tests/IntegrationTests")
    echo "  Testing: $short_name"

    LOG_FILE=$(mktemp)

    # Run integration tests (no --no-build: incremental build handles changes)
    if ! dotnet test "$project" --verbosity normal > "$LOG_FILE" 2>&1; then
        OVERALL_FAILED=1
        parse_test_failures "$LOG_FILE" "$short_name" "integration"
    fi

    rm -f "$LOG_FILE"
done

FAILURE_COUNT=$(count_pending "integration_*.txt")

"$SCRIPT_DIR/summarize.sh" 2>/dev/null || true

if [ "$OVERALL_FAILED" -eq 1 ]; then
    echo "INTEGRATION: FAILED ($FAILURE_COUNT failures)"
    exit 1
fi

echo "INTEGRATION: SUCCESS"
exit 0
