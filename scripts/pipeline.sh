#!/bin/bash
# pipeline.sh - Full local pipeline: clean → build → arch → test → mutate → integration
# Usage: ./scripts/pipeline.sh [--quiet]
#   --quiet: suppresses verbose output, shows only SUMMARY.txt at the end
# Output: artifacts/summary.json
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
lib_init

QUIET=false
if [ "${1:-}" = "--quiet" ]; then
    QUIET=true
fi

# Redirect output when --quiet
if [ "$QUIET" = true ]; then
    LOG_FILE=$(mktemp)
    exec 3>&1 4>&2
    exec > "$LOG_FILE" 2>&1
fi

START_TIME=$(timer_start)

echo "========================================"
echo "  BEDROCK LOCAL PIPELINE"
echo "========================================"
echo ""

# === CLEAN ===
echo ">>> Step 1/6: Clean"
"$SCRIPT_DIR/clean.sh"
echo ""

# === BUILD ===
echo ">>> Step 2/6: Build"
BUILD_START=$(timer_start)
BUILD_FAILED=0
"$SCRIPT_DIR/build.sh" || BUILD_FAILED=1
BUILD_DURATION=$(timer_elapsed "$BUILD_START")
echo ""

if [ "$BUILD_FAILED" -eq 1 ]; then
    echo ">>> Steps 3-6 SKIPPED (build failed)"
    echo ""

    TOTAL_DURATION=$(timer_elapsed "$START_TIME")
    mkdir -p artifacts

    cat > artifacts/summary.json << EOF
{
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "duration_ms": $TOTAL_DURATION,
  "build": { "success": false, "duration_ms": $BUILD_DURATION },
  "architecture": { "success": false, "skipped": true, "duration_ms": 0 },
  "tests": { "success": false, "skipped": true, "duration_ms": 0 },
  "mutation": { "success": false, "skipped": true, "duration_ms": 0 },
  "integration": { "success": false, "skipped": true, "duration_ms": 0 }
}
EOF

    if [ "$QUIET" = true ]; then
        exec 1>&3 2>&4
        echo "PIPELINE: FAILED"
        echo ""
        cat artifacts/pending/SUMMARY.txt 2>/dev/null || echo "Check artifacts/summary.json for details."
        rm -f "$LOG_FILE"
    fi
    exit 1
fi

# === ARCH ===
echo ">>> Step 3/6: Architecture Tests"
ARCH_START=$(timer_start)
ARCH_FAILED=0
"$SCRIPT_DIR/arch.sh" || ARCH_FAILED=1
ARCH_DURATION=$(timer_elapsed "$ARCH_START")
echo ""

if [ "$ARCH_FAILED" -eq 1 ]; then
    echo ">>> Steps 4-6 SKIPPED (architecture tests failed)"
    echo ""

    TOTAL_DURATION=$(timer_elapsed "$START_TIME")
    mkdir -p artifacts

    cat > artifacts/summary.json << EOF
{
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "duration_ms": $TOTAL_DURATION,
  "build": { "success": true, "duration_ms": $BUILD_DURATION },
  "architecture": { "success": false, "duration_ms": $ARCH_DURATION },
  "tests": { "success": false, "skipped": true, "duration_ms": 0 },
  "mutation": { "success": false, "skipped": true, "duration_ms": 0 },
  "integration": { "success": false, "skipped": true, "duration_ms": 0 }
}
EOF

    echo ""
    echo "========================================"
    echo "  PIPELINE COMPLETE"
    echo "========================================"
    echo ""
    echo "Duration: ${TOTAL_DURATION}ms"
    echo "Summary:  artifacts/summary.json"
    echo ""
    echo "STATUS: FAILED (architecture tests failed)"

    if [ "$QUIET" = true ]; then
        exec 1>&3 2>&4
        echo "PIPELINE: FAILED"
        echo ""
        cat artifacts/pending/SUMMARY.txt 2>/dev/null || echo "Check artifacts/summary.json for details."
        rm -f "$LOG_FILE"
    fi
    exit 1
fi

# === TEST ===
echo ">>> Step 4/6: Test"
TEST_START=$(timer_start)
"$SCRIPT_DIR/test.sh"
TEST_DURATION=$(timer_elapsed "$TEST_START")
echo ""

# === MUTATE ===
echo ">>> Step 5/6: Mutate"
MUTATE_START=$(timer_start)
MUTATION_FAILED=0
"$SCRIPT_DIR/mutate.sh" || MUTATION_FAILED=1
MUTATE_DURATION=$(timer_elapsed "$MUTATE_START")
echo ""

# === INTEGRATION ===
echo ">>> Step 6/6: Integration Tests"
INTEGRATION_START=$(timer_start)
INTEGRATION_FAILED=0
"$SCRIPT_DIR/integration.sh" || INTEGRATION_FAILED=1
INTEGRATION_DURATION=$(timer_elapsed "$INTEGRATION_START")
echo ""

# === GENERATE REPORTS ===
echo ">>> Generating Reports..."
echo "  Unit Test Report..."
"$SCRIPT_DIR/report-unittest.sh" || echo "Warning: Unit test report generation failed"
echo "  Integration Test Report..."
"$SCRIPT_DIR/report-integration.sh" || echo "Warning: Integration report generation failed"
echo ""

TOTAL_DURATION=$(timer_elapsed "$START_TIME")

# === GENERATE SUMMARY JSON ===
echo ">>> Generating summary..."
mkdir -p artifacts

# Parse coverage from cobertura XML files
COVERAGE_JSON="[]"
for coverage_file in $(find artifacts/coverage/raw -name "*.cobertura.xml" 2>/dev/null); do
    if [ -f "$coverage_file" ]; then
        LINE_RATE=$(grep -o 'line-rate="[^"]*"' "$coverage_file" | head -1 | sed 's/line-rate="//;s/"//')
        BRANCH_RATE=$(grep -o 'branch-rate="[^"]*"' "$coverage_file" | head -1 | sed 's/branch-rate="//;s/"//')
        if [ -n "$LINE_RATE" ]; then
            COVERAGE_JSON="{\"line_rate\": $LINE_RATE, \"branch_rate\": ${BRANCH_RATE:-0}}"
        fi
    fi
done

cat > artifacts/summary.json << SUMMARY_EOF
{
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "duration_ms": $TOTAL_DURATION,
  "build": { "success": true, "duration_ms": $BUILD_DURATION },
  "architecture": { "success": true, "duration_ms": $ARCH_DURATION },
  "tests": { "success": true, "duration_ms": $TEST_DURATION },
  "coverage": $COVERAGE_JSON,
  "mutation": {
    "success": $([ $MUTATION_FAILED -eq 0 ] && echo "true" || echo "false"),
    "duration_ms": $MUTATE_DURATION
  },
  "integration": {
    "success": $([ $INTEGRATION_FAILED -eq 0 ] && echo "true" || echo "false"),
    "duration_ms": $INTEGRATION_DURATION
  }
}
SUMMARY_EOF

echo ""
echo "========================================"
echo "  PIPELINE COMPLETE"
echo "========================================"
echo ""
echo "Duration: ${TOTAL_DURATION}ms"
echo "Summary:  artifacts/summary.json"
echo "Architecture: artifacts/architecture-report/index.html"
echo "Unit Tests:   artifacts/unittest-report/index.html"
echo "Coverage: artifacts/coverage/"
echo "Mutation: artifacts/mutation/"
if [ -f "artifacts/integration-report/index.html" ]; then
    echo "Integration:  artifacts/integration-report/index.html"
fi
echo ""

# === UNIVERSAL GATE ===
PENDING_COUNT=$(count_pending)

if [ "$PENDING_COUNT" -gt 0 ]; then
    echo "STATUS: FAILED ($PENDING_COUNT pending items)"
    echo ""
    echo "Next steps:"
    echo "  1. Read artifacts/pending/SUMMARY.txt for overview"
    echo "  2. Check individual files in artifacts/pending/ for details"
    echo "  3. Fix all pending items"
    echo "  4. Run pipeline again"

    if [ "$QUIET" = true ]; then
        exec 1>&3 2>&4
        echo "PIPELINE: FAILED"
        echo ""
        cat artifacts/pending/SUMMARY.txt 2>/dev/null || echo "Check artifacts/summary.json for details."
        rm -f "$LOG_FILE"
    fi
    exit 1
else
    echo "STATUS: SUCCESS"

    if [ "$QUIET" = true ]; then
        exec 1>&3 2>&4
        echo "PIPELINE: SUCCESS"
        echo ""
        cat artifacts/pending/SUMMARY.txt 2>/dev/null || echo "No pending items."
        rm -f "$LOG_FILE"
    fi
fi
