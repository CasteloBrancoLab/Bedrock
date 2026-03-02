#!/bin/bash
# mutate.sh - Runs mutation tests and extracts surviving mutants to artifacts/pending/
# Usage: ./scripts/mutate.sh [mutation-test-dir]
#   No args: runs all mutation test projects under tests/MutationTests/
#   With arg: runs only the specified mutation test directory
# Output: artifacts/pending/mutant_<project>_<NNN>.txt (per survivor)
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
lib_init

echo "=== MUTATE ==="

mkdir -p artifacts/mutation
clean_pending "mutant"

# Determine configs to run
if [ -n "${1:-}" ]; then
    CONFIGS=$(find "$1" -name "stryker-config.json" 2>/dev/null)
else
    CONFIGS=$(find tests/MutationTests -name "stryker-config.json" 2>/dev/null)
fi

if [ -z "$CONFIGS" ]; then
    echo "No mutation test configs found"
    "$SCRIPT_DIR/summarize.sh" 2>/dev/null || true
    exit 0
fi

FAILED=0

for config in $CONFIGS; do
    dir=$(dirname "$config")
    name=$(basename "$dir")
    echo "  Mutating: $name"

    # Check if source project has .cs files to mutate
    if ! has_source_files "$ROOT_DIR/$config"; then
        echo "    SKIPPED (no source files)"
        continue
    fi

    cd "$dir"

    if dotnet stryker \
        -O "$ROOT_DIR/artifacts/mutation/$name" \
        --reporter json \
        --reporter progress > /dev/null 2>&1; then
        echo "    PASSED"
    else
        echo "    FAILED"
        FAILED=1
    fi

    cd "$ROOT_DIR"
done

# Extract surviving mutants from Stryker JSON reports
for report in artifacts/mutation/*/reports/mutation-report.json; do
    if [ ! -f "$report" ]; then
        continue
    fi
    project=$(basename "$(dirname "$(dirname "$report")")")
    parse_surviving_mutants "$report" "$project"
done

MUTANT_COUNT=$(count_pending "mutant_*.txt")

"$SCRIPT_DIR/summarize.sh" 2>/dev/null || true

if [ "$FAILED" -eq 1 ] || [ "$MUTANT_COUNT" -gt 0 ]; then
    echo "MUTATION: FAILED ($MUTANT_COUNT surviving mutants)"
    exit 1
fi

echo "MUTATION: SUCCESS (100%)"
exit 0
