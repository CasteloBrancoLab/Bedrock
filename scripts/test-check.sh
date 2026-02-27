#!/bin/bash
# test-check.sh - Runs unit tests and extracts failures + coverage gaps to artifacts/pending/
# Usage: ./scripts/test-check.sh [project.csproj]
#   No args: runs all unit test projects under tests/UnitTests/
#   With arg: runs only the specified test project
# Output: artifacts/pending/test_<project>_<NNN>.txt (per failure)
#         artifacts/pending/coverage_<project>_<NNN>.txt (per uncovered file)
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== TEST CHECK ==="

mkdir -p artifacts/pending artifacts/test-results artifacts/coverage/raw
rm -f artifacts/pending/test_*.txt artifacts/pending/coverage_*.txt

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
    "$SCRIPT_DIR/generate-pending-summary.sh" 2>/dev/null || true
    exit 0
fi

OVERALL_FAILED=0

for project in $PROJECTS; do
    # Build unique name: tests/UnitTests/BuildingBlocks/Core/*.csproj -> BuildingBlocks.Core
    short_name=$(echo "$project" | sed 's|tests/UnitTests/||' | sed 's|/[^/]*\.csproj$||' | sed 's|/|.|g')
    echo "  Testing: $short_name"

    LOG_FILE=$(mktemp)
    RESULTS_DIR="artifacts/test-results/$short_name"
    mkdir -p "$RESULTS_DIR"

    # Run tests with coverage collection
    if ! dotnet test "$project" --verbosity normal \
        --collect:"XPlat Code Coverage" \
        --results-directory "$RESULTS_DIR" \
        > "$LOG_FILE" 2>&1; then
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

    # Copy cobertura XML to artifacts/coverage/raw/
    for cob in $(find "$RESULTS_DIR" -name "coverage.cobertura.xml" 2>/dev/null); do
        cp "$cob" "artifacts/coverage/raw/${short_name}.cobertura.xml"
    done
done

FAILURE_COUNT=$(find artifacts/pending -name "test_*.txt" 2>/dev/null | wc -l)
FAILURE_COUNT=${FAILURE_COUNT//[^0-9]/}

# ========================================
# COVERAGE GAP EXTRACTION
# ========================================
echo "Extracting coverage gaps..."

# Coverage exclusion patterns (aligned with sonar.coverage.exclusions in ci.yml)
COVERAGE_EXCLUSIONS=(
    "**/tests/**/*"
    "**/samples/**/*"
    "**/templates/**/*"
    "**/tools/**/*"
    "**/playground/**/*"
    "**/src/BuildingBlocks/Testing/Benchmarks/**/*"
    "**/src/BuildingBlocks/Testing/Integration/**/*"
    "**/src/BuildingBlocks/Testing/Attributes/**/*"
    "**/src/BuildingBlocks/Testing/TestBase.cs"
    "**/src/BuildingBlocks/Testing/ServiceCollectionFixture.cs"
)

# Convert glob patterns to pipe-delimited regex for awk
EXCL_REGEX=""
for pat in "${COVERAGE_EXCLUSIONS[@]}"; do
    r=$(printf '%s' "$pat" | sed 's/[.]/[.]/g; s/\*\*/__DBLSTAR__/g; s/\*/[^\/]*/g; s/__DBLSTAR__/.*/g')
    r="^${r}$"
    if [ -n "$EXCL_REGEX" ]; then
        EXCL_REGEX="${EXCL_REGEX}|${r}"
    else
        EXCL_REGEX="$r"
    fi
done

COVERAGE_FILE_INDEX=0

for cobertura in artifacts/coverage/raw/*.cobertura.xml; do
    if [ ! -f "$cobertura" ]; then
        continue
    fi

    project_name=$(basename "$cobertura" .cobertura.xml)

    # Parse cobertura XML using awk (pure bash, cross-platform)
    awk -F'"' -v project_name="$project_name" -v excl_regex="$EXCL_REGEX" '
    BEGIN {
        suffix = "." project_name
        pkg = ""; cls_file = ""; rel_path = ""
        total = 0; covered = 0; uncov = ""; uncov_count = 0
        in_matching_pkg = 0; skip_class = 1
    }

    /<package[[:space:]]/ {
        for (i = 1; i <= NF; i++) {
            if ($(i) ~ /name=$/) { pkg = $(i+1); break }
        }
        if (pkg == project_name || substr(pkg, length(pkg) - length(suffix) + 1) == suffix) {
            in_matching_pkg = 1
        } else {
            in_matching_pkg = 0
        }
    }

    /<\/package>/ { in_matching_pkg = 0 }

    in_matching_pkg && /<class[[:space:]]/ {
        # Output previous class if it had uncovered lines
        if (total > 0 && uncov_count > 0 && !skip_class) {
            line_rate = (total > 0) ? sprintf("%.1f", covered / total * 100) : "100.0"
            print pkg "|" rel_path "|" line_rate "|" total "|" covered "|" uncov
        }

        # Parse new class
        cls_file = ""
        for (i = 1; i <= NF; i++) {
            if ($(i) ~ /filename=$/) { cls_file = $(i+1); break }
        }

        # Normalize backslashes to forward slashes
        gsub(/\\/, "/", cls_file)

        # Extract relative path from /src/
        src_idx = index(cls_file, "/src/")
        if (src_idx == 0) {
            skip_class = 1
        } else {
            rel_path_full = substr(cls_file, src_idx)
            rel_path = substr(cls_file, src_idx + 1)
            if (excl_regex != "" && rel_path_full ~ excl_regex) {
                skip_class = 1
            } else {
                skip_class = 0
            }
        }

        total = 0; covered = 0; uncov = ""; uncov_count = 0
    }

    in_matching_pkg && !skip_class && /<line[[:space:]]/ {
        linenum = ""; hits = ""
        for (i = 1; i <= NF; i++) {
            if ($(i) ~ /number=$/) linenum = $(i+1)
            if ($(i) ~ /hits=$/) hits = $(i+1)
        }
        if (linenum != "" && hits != "") {
            total++
            if (hits + 0 == 0) {
                uncov = uncov (uncov ? "," : "") linenum
                uncov_count++
            } else {
                covered++
            }
        }
    }

    END {
        if (total > 0 && uncov_count > 0 && !skip_class) {
            line_rate = (total > 0) ? sprintf("%.1f", covered / total * 100) : "100.0"
            print pkg "|" rel_path "|" line_rate "|" total "|" covered "|" uncov
        }
    }
    ' "$cobertura" | while IFS='|' read -r pkg filePath lineRate total coveredCount uncoveredLines; do
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

COVERAGE_PENDING=$(find artifacts/pending -name "coverage_*.txt" 2>/dev/null | wc -l)
COVERAGE_PENDING=${COVERAGE_PENDING//[^0-9]/}
echo "Coverage gaps: $COVERAGE_PENDING files with uncovered lines"

"$SCRIPT_DIR/generate-pending-summary.sh" 2>/dev/null || true

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
