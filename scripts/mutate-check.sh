#!/bin/bash
# mutate-check.sh - Runs mutation tests and extracts surviving mutants to artifacts/pending/
# Usage: ./scripts/mutate-check.sh [mutation-test-dir]
#   No args: runs all mutation test projects under tests/MutationTests/
#   With arg: runs only the specified mutation test directory
# Output: artifacts/pending/mutant_<project>_<NNN>.txt (per survivor)
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== MUTATE CHECK ==="

mkdir -p artifacts/pending artifacts/mutation
rm -f artifacts/pending/mutant_*.txt

# Determine configs to run
if [ -n "${1:-}" ]; then
    CONFIGS=$(find "$1" -name "stryker-config.json" 2>/dev/null)
else
    CONFIGS=$(find tests/MutationTests -name "stryker-config.json" 2>/dev/null)
fi

if [ -z "$CONFIGS" ]; then
    echo "No mutation test configs found"
    "$SCRIPT_DIR/generate-pending-summary.sh" 2>/dev/null || true
    exit 0
fi

FAILED=0

for config in $CONFIGS; do
    dir=$(dirname "$config")
    name=$(basename "$dir")
    echo "  Mutating: $name"

    # Check if source project has .cs files to mutate
    src_project=$(grep -o '"project":.*"' "$ROOT_DIR/$config" | head -1 | sed 's/"project":[[:space:]]*"//;s/"$//')
    if [ -n "$src_project" ]; then
        src_csproj=$(find "$ROOT_DIR" -name "$src_project" -not -path "*/bin/*" -not -path "*/obj/*" | head -1)
        if [ -n "$src_csproj" ]; then
            src_dir=$(dirname "$src_csproj")
            cs_count=$(find "$src_dir" -name "*.cs" ! -name "GlobalUsings.cs" -not -path "*/obj/*" -not -path "*/bin/*" 2>/dev/null | wc -l)
            cs_count=${cs_count//[^0-9]/}
            if [ "$cs_count" -eq 0 ]; then
                echo "    SKIPPED (no source files)"
                continue
            fi
        fi
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

    # Parse mutation report JSON using awk (pure bash, cross-platform)
    # Strip "source" lines first — they contain full file code that confuses the parser
    grep -v '^[[:space:]]*"source"[[:space:]]*:' "$report" | awk -v project="$project" -v dir="artifacts/pending" '
    BEGIN {
        idx = 0; file = ""; m_file = ""
        m_mutator = ""; m_line = ""; m_status = ""; m_desc = ""
        in_start = 0; had_mutant = 0
    }
    /^[[:space:]]*"[^"]+[.][a-zA-Z]+"[[:space:]]*:[[:space:]]*\{/ {
        s = $0; sub(/^[[:space:]]*"/, "", s); sub(/".*/, "", s); file = s
    }
    /[[:space:]]*"id"[[:space:]]*:[[:space:]]*"/ {
        if (had_mutant && (m_status == "Survived" || m_status == "NoCoverage")) {
            idx++
            num = sprintf("%03d", idx)
            outfile = dir "/mutant_" project "_" num ".txt"
            print "PROJECT: " project > outfile
            print "FILE: " m_file >> outfile
            print "LINE: " m_line >> outfile
            print "STATUS: " m_status >> outfile
            print "MUTATOR: " m_mutator >> outfile
            print "DESCRIPTION: " m_desc >> outfile
            close(outfile)
        }
        m_file = file; m_mutator = ""; m_line = ""; m_status = ""; m_desc = ""
        in_start = 0; had_mutant = 1
    }
    /"mutatorName"[[:space:]]*:/ {
        s = $0; sub(/.*"mutatorName"[[:space:]]*:[[:space:]]*"/, "", s); sub(/".*/, "", s)
        m_mutator = s
    }
    /"start"[[:space:]]*:[[:space:]]*\{/ { in_start = 1 }
    /"end"[[:space:]]*:[[:space:]]*\{/ { in_start = 0 }
    /"line"[[:space:]]*:/ && in_start {
        s = $0; sub(/.*"line"[[:space:]]*:[[:space:]]*/, "", s); sub(/[^0-9].*/, "", s)
        if (s ~ /^[0-9]+$/) m_line = s
    }
    /"status"[[:space:]]*:/ {
        s = $0; sub(/.*"status"[[:space:]]*:[[:space:]]*"/, "", s); sub(/".*/, "", s)
        m_status = s
    }
    /"description"[[:space:]]*:/ {
        s = $0; sub(/.*"description"[[:space:]]*:[[:space:]]*"/, "", s); sub(/".*/, "", s)
        m_desc = s
    }
    END {
        if (had_mutant && (m_status == "Survived" || m_status == "NoCoverage")) {
            idx++
            num = sprintf("%03d", idx)
            outfile = dir "/mutant_" project "_" num ".txt"
            print "PROJECT: " project > outfile
            print "FILE: " m_file >> outfile
            print "LINE: " m_line >> outfile
            print "STATUS: " m_status >> outfile
            print "MUTATOR: " m_mutator >> outfile
            print "DESCRIPTION: " m_desc >> outfile
            close(outfile)
        }
    }
    ' || true
done

MUTANT_COUNT=$(find artifacts/pending -name "mutant_*.txt" 2>/dev/null | wc -l)
MUTANT_COUNT=${MUTANT_COUNT//[^0-9]/}

"$SCRIPT_DIR/generate-pending-summary.sh" 2>/dev/null || true

if [ "$FAILED" -eq 1 ] || [ "$MUTANT_COUNT" -gt 0 ]; then
    echo "MUTATION: FAILED ($MUTANT_COUNT surviving mutants)"
    exit 1
fi

echo "MUTATION: SUCCESS (100%)"
exit 0
