#!/bin/bash
# test.sh - Runs unit tests and extracts failures + coverage gaps to artifacts/pending/
# Usage: ./scripts/test.sh [project.csproj]
#   No args: runs all unit test projects under tests/UnitTests/
#   With arg: runs only the specified test project
# Output: artifacts/pending/test_<project>_<NNN>.txt (per failure)
#         artifacts/pending/coverage_<project>_<NNN>.txt (per uncovered file)
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
lib_init

echo "=== TEST ==="

mkdir -p artifacts/test-results artifacts/coverage/raw
clean_pending "test"
clean_pending "coverage"

# Clean old coverage results
rm -rf artifacts/test-results/*
rm -f artifacts/coverage/raw/*.cobertura.xml

# Determine projects to test
if [ -n "${1:-}" ]; then
    PROJECTS="$1"
else
    PROJECTS=$(find tests/UnitTests -name "*.csproj" 2>/dev/null)
fi

if [ -z "$PROJECTS" ]; then
    echo "No test projects found"
    "$SCRIPT_DIR/summarize.sh" 2>/dev/null || true
    exit 0
fi

OVERALL_FAILED=0

for project in $PROJECTS; do
    short_name=$(derive_short_name "$project" "tests/UnitTests")
    echo "  Testing: $short_name"

    LOG_FILE=$(mktemp)
    RESULTS_DIR="artifacts/test-results/$short_name"
    mkdir -p "$RESULTS_DIR"

    # Run tests with coverage collection (Coverlet)
    if ! dotnet test "$project" --verbosity normal \
        --collect:"XPlat Code Coverage" \
        --results-directory "$RESULTS_DIR" \
        > "$LOG_FILE" 2>&1; then
        OVERALL_FAILED=1
        parse_test_failures "$LOG_FILE" "$short_name" "test"
    fi

    rm -f "$LOG_FILE"

    # Copy cobertura XML to artifacts/coverage/raw/
    for cob in $(find "$RESULTS_DIR" -name "coverage.cobertura.xml" 2>/dev/null); do
        cp "$cob" "artifacts/coverage/raw/${short_name}.cobertura.xml"
    done
done

FAILURE_COUNT=$(count_pending "test_*.txt")

# ========================================
# COVERAGE GAP EXTRACTION
# ========================================
echo "Extracting coverage gaps..."

EXCL_REGEX=$(build_exclusion_regex)
COVERAGE_FILE_INDEX=0

for cobertura in artifacts/coverage/raw/*.cobertura.xml; do
    if [ ! -f "$cobertura" ]; then
        continue
    fi

    project_name=$(basename "$cobertura" .cobertura.xml)

    parse_coverage_gaps "$cobertura" "$project_name" "$EXCL_REGEX" | while IFS='|' read -r pkg filePath lineRate total coveredCount uncoveredLines; do
        if [ -z "$filePath" ]; then continue; fi
        COVERAGE_FILE_INDEX=$((COVERAGE_FILE_INDEX + 1))
        idx=$(printf "%03d" "$COVERAGE_FILE_INDEX")

        cat > "artifacts/pending/coverage_${project_name}_${idx}.txt" << COVEOF
PROJECT: $pkg
FILE: $filePath
LINE_RATE: ${lineRate}%
TOTAL_LINES: $total
COVERED_LINES: $coveredCount
UNCOVERED_LINES: $uncoveredLines
COVEOF
    done
done

COVERAGE_PENDING=$(count_pending "coverage_*.txt")
echo "Coverage gaps: $COVERAGE_PENDING files with uncovered lines"

"$SCRIPT_DIR/summarize.sh" 2>/dev/null || true

if [ "$OVERALL_FAILED" -eq 1 ]; then
    echo "TESTS: FAILED ($FAILURE_COUNT failures, $COVERAGE_PENDING coverage gaps)"
    exit 1
fi

if [ "$COVERAGE_PENDING" -gt 0 ]; then
    echo "TESTS: PASSED ($COVERAGE_PENDING coverage gaps)"
else
    echo "TESTS: SUCCESS"
fi
exit 0
