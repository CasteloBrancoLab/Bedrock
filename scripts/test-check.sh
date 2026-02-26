#!/bin/bash
# test-check.sh - Runs unit tests and extracts failures to artifacts/pending/
# Usage: ./scripts/test-check.sh [project.csproj]
#   No args: runs all unit test projects under tests/UnitTests/
#   With arg: runs only the specified test project
# Output: artifacts/pending/test_<project>_<NNN>.txt (per failure)
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== TEST CHECK ==="

mkdir -p artifacts/pending artifacts/test-results
rm -f artifacts/pending/test_*.txt

# Determine projects to test
if [ -n "${1:-}" ]; then
    PROJECTS="$1"
else
    PROJECTS=$(find tests/UnitTests -name "*.csproj" 2>/dev/null)
fi

if [ -z "$PROJECTS" ]; then
    echo "No test projects found"
    "$SCRIPT_DIR/generate-pending-summary.sh" 2>/dev/null || true
    exit 0
fi

OVERALL_FAILED=0

for project in $PROJECTS; do
    # Build unique name: tests/UnitTests/BuildingBlocks/Core/*.csproj -> BuildingBlocks.Core
    short_name=$(echo "$project" | sed 's|tests/UnitTests/||' | sed 's|/[^/]*\.csproj$||' | sed 's|/|.|g')
    echo "  Testing: $short_name"

    LOG_FILE=$(mktemp)

    # Run tests (no --no-build: incremental build handles changes)
    if ! dotnet test "$project" --verbosity normal > "$LOG_FILE" 2>&1; then
        OVERALL_FAILED=1

        # Parse dotnet test console output for failed tests
        # Format:
        #   Failed TestNamespace.TestClass.TestMethod [12ms]
        #   Error Message:
        #    assertion message
        #   Stack Trace:
        #    at Namespace.Class.Method() in /path/file.cs:line 42
        awk -v project="$short_name" -v dir="artifacts/pending" '
        BEGIN { idx = 0; test_name = ""; msg = ""; st = ""; in_msg = 0; in_st = 0 }

        /^[[:space:]]+Failed / {
            if (test_name != "") {
                idx++
                num = sprintf("%03d", idx)
                outfile = dir "/test_" project "_" num ".txt"
                print "PROJECT: " project > outfile
                print "TEST: " test_name >> outfile
                print "MESSAGE: " msg >> outfile
                print "STACKTRACE: " st >> outfile
                close(outfile)
            }
            s = $0
            sub(/^[[:space:]]+Failed /, "", s)
            sub(/ \[.*\]$/, "", s)
            test_name = s
            msg = ""; st = ""; in_msg = 0; in_st = 0
        }

        /^[[:space:]]+Error Message:/ { in_msg = 1; in_st = 0; next }
        /^[[:space:]]+Stack Trace:/ { in_st = 1; in_msg = 0; next }
        /^$/ || /^[[:space:]]+Failed!/ || /^[[:space:]]+Passed!/ { in_msg = 0; in_st = 0 }

        in_msg {
            s = $0; sub(/^[[:space:]]+/, "", s)
            msg = msg (msg != "" ? " " : "") s
        }

        in_st && st == "" {
            s = $0; sub(/^[[:space:]]+/, "", s)
            st = s
        }

        END {
            if (test_name != "") {
                idx++
                num = sprintf("%03d", idx)
                outfile = dir "/test_" project "_" num ".txt"
                print "PROJECT: " project > outfile
                print "TEST: " test_name >> outfile
                print "MESSAGE: " msg >> outfile
                print "STACKTRACE: " st >> outfile
                close(outfile)
            }
        }
        ' "$LOG_FILE"
    fi

    rm -f "$LOG_FILE"
done

FAILURE_COUNT=$(find artifacts/pending -name "test_*.txt" 2>/dev/null | wc -l)
FAILURE_COUNT=${FAILURE_COUNT//[^0-9]/}

"$SCRIPT_DIR/generate-pending-summary.sh" 2>/dev/null || true

if [ "$OVERALL_FAILED" -eq 1 ]; then
    echo "TESTS: FAILED ($FAILURE_COUNT failures)"
    exit 1
fi

echo "TESTS: SUCCESS"
exit 0
