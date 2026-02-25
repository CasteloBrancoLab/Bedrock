#!/bin/bash
# Executa a pipeline completa: clean → build → architecture → test → mutate → integration
# Gera artifacts/summary.json com resultados consolidados

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

START_TIME=$(date +%s%3N)

echo "========================================"
echo "  BEDROCK LOCAL PIPELINE"
echo "========================================"
echo ""

# === CLEAN ===
echo ">>> Step 1/6: Clean"
"$SCRIPT_DIR/clean.sh"
"$SCRIPT_DIR/clean-artifacts.sh"
echo ""

# === BUILD ===
echo ">>> Step 2/6: Build"
BUILD_START=$(date +%s%3N)
"$SCRIPT_DIR/build.sh"
BUILD_END=$(date +%s%3N)
BUILD_DURATION=$((BUILD_END - BUILD_START))
echo ""

# === ARCH ===
echo ">>> Step 3/6: Architecture Tests"
ARCH_START=$(date +%s%3N)
ARCH_FAILED=0
"$SCRIPT_DIR/architecture.sh" || ARCH_FAILED=1
ARCH_END=$(date +%s%3N)
ARCH_DURATION=$((ARCH_END - ARCH_START))
echo ""

if [ $ARCH_FAILED -eq 1 ]; then
    echo ">>> Steps 4-6 SKIPPED (architecture tests failed)"
    echo ""

    END_TIME=$(date +%s%3N)
    TOTAL_DURATION=$((END_TIME - START_TIME))

    # Generate summary
    echo ">>> Generating summary..."
    mkdir -p artifacts

    cat > artifacts/summary.json << ARCH_FAIL_EOF
{
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "duration_ms": $TOTAL_DURATION,
  "build": {
    "success": true,
    "duration_ms": $BUILD_DURATION
  },
  "architecture": {
    "success": false,
    "duration_ms": $ARCH_DURATION
  },
  "tests": {
    "success": false,
    "skipped": true,
    "duration_ms": 0
  },
  "mutation": {
    "success": false,
    "skipped": true,
    "duration_ms": 0
  },
  "integration": {
    "success": false,
    "skipped": true,
    "projects_count": 0,
    "duration_ms": 0
  }
}
ARCH_FAIL_EOF

    echo ""
    echo "========================================"
    echo "  PIPELINE COMPLETE"
    echo "========================================"
    echo ""
    echo "Duration: ${TOTAL_DURATION}ms"
    echo "Summary:  artifacts/summary.json"
    if [ -f "artifacts/architecture-report/index.html" ]; then
        echo "Architecture Report: artifacts/architecture-report/index.html"
    fi
    echo ""
    echo ">>> Extracting pending items..."
    "$SCRIPT_DIR/summarize.sh"
    "$SCRIPT_DIR/generate-pending-summary.sh"
    echo ""
    echo "STATUS: FAILED (architecture tests failed)"
    echo ""
    echo "Next steps:"
    echo "  1. Read artifacts/pending/SUMMARY.txt for overview"
    echo "  2. Check individual files in artifacts/pending/architecture_*.txt for details"
    echo "  3. Fix architecture violations"
    echo "  4. Run pipeline again"
    exit 1
fi

# === TEST ===
echo ">>> Step 4/6: Test"
TEST_START=$(date +%s%3N)
"$SCRIPT_DIR/test.sh"
TEST_END=$(date +%s%3N)
TEST_DURATION=$((TEST_END - TEST_START))
echo ""

# === MUTATE ===
echo ">>> Step 5/6: Mutate"
MUTATE_START=$(date +%s%3N)
MUTATION_FAILED=0
"$SCRIPT_DIR/mutate.sh" || MUTATION_FAILED=1
MUTATE_END=$(date +%s%3N)
MUTATE_DURATION=$((MUTATE_END - MUTATE_START))
echo ""

# === INTEGRATION TESTS ===
INTEGRATION_FAILED=0
INTEGRATION_DURATION=0
INTEGRATION_PROJECTS_COUNT=0

if [ $MUTATION_FAILED -eq 0 ]; then
    echo ">>> Step 6/6: Integration Tests"
    INTEGRATION_START=$(date +%s%3N)

    # Find all integration test projects dynamically
    INTEGRATION_PROJECTS=$(find tests/IntegrationTests -name "*.csproj" 2>/dev/null)

    if [ -n "$INTEGRATION_PROJECTS" ]; then
        for project in $INTEGRATION_PROJECTS; do
            INTEGRATION_PROJECTS_COUNT=$((INTEGRATION_PROJECTS_COUNT + 1))
            name=$(basename "$(dirname "$project")")
            echo "Running integration tests for: $name"

            if ! dotnet test "$project" --no-build --logger "trx;LogFileName=integration-$name.trx" --results-directory "artifacts/test-results"; then
                INTEGRATION_FAILED=1
                echo "FAILED: Integration tests failed for $name"
            fi
        done
    else
        echo "No integration test projects found in tests/IntegrationTests"
    fi

    INTEGRATION_END=$(date +%s%3N)
    INTEGRATION_DURATION=$((INTEGRATION_END - INTEGRATION_START))
    echo ""
else
    echo ">>> Step 6/6: Integration Tests (SKIPPED - mutation tests failed)"
    echo ""
fi

# === GENERATE REPORTS ===
# Reports are generated after all steps so all artifacts (coverage, mutation, integration) are available
echo ">>> Generating Reports..."
echo "  Unit Test Report..."
"$SCRIPT_DIR/generate-unittest-report.sh" || echo "Warning: Unit test report generation failed"

if [ $INTEGRATION_FAILED -eq 0 ] && [ $MUTATION_FAILED -eq 0 ]; then
    echo "  Integration Test Report..."
    "$SCRIPT_DIR/generate-integration-report.sh" || echo "Warning: Integration report generation failed"
fi
echo ""

END_TIME=$(date +%s%3N)
TOTAL_DURATION=$((END_TIME - START_TIME))

# === GENERATE SUMMARY ===
echo ">>> Generating summary..."
mkdir -p artifacts

# Parse coverage from cobertura XML files
COVERAGE_JSON="[]"
for coverage_file in $(find artifacts/coverage/raw -name "*.cobertura.xml" 2>/dev/null); do
    if [ -f "$coverage_file" ]; then
        # Extract line-rate from coverage file
        LINE_RATE=$(grep -o 'line-rate="[^"]*"' "$coverage_file" | head -1 | sed 's/line-rate="//;s/"//')
        BRANCH_RATE=$(grep -o 'branch-rate="[^"]*"' "$coverage_file" | head -1 | sed 's/branch-rate="//;s/"//')
        if [ -n "$LINE_RATE" ]; then
            COVERAGE_JSON="{\"line_rate\": $LINE_RATE, \"branch_rate\": ${BRANCH_RATE:-0}}"
        fi
    fi
done

# Parse mutation results from JSON files
MUTATION_JSON="[]"
for mutation_file in $(find artifacts/mutation -name "*.json" 2>/dev/null | head -1); do
    if [ -f "$mutation_file" ]; then
        MUTATION_JSON=$(cat "$mutation_file")
    fi
done

# Generate summary.json
cat > artifacts/summary.json << SUMMARY_EOF
{
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "duration_ms": $TOTAL_DURATION,
  "build": {
    "success": true,
    "duration_ms": $BUILD_DURATION
  },
  "architecture": {
    "success": true,
    "duration_ms": $ARCH_DURATION
  },
  "tests": {
    "success": true,
    "duration_ms": $TEST_DURATION
  },
  "coverage": $COVERAGE_JSON,
  "mutation": {
    "success": $([ $MUTATION_FAILED -eq 0 ] && echo "true" || echo "false"),
    "duration_ms": $MUTATE_DURATION
  },
  "integration": {
    "success": $([ $INTEGRATION_FAILED -eq 0 ] && echo "true" || echo "false"),
    "skipped": $([ $MUTATION_FAILED -eq 1 ] && echo "true" || echo "false"),
    "projects_count": $INTEGRATION_PROJECTS_COUNT,
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
    echo "Report:   artifacts/integration-report/index.html"
fi
echo ""

# === EXTRACT ALL PENDING ITEMS (always runs) ===
echo ">>> Extracting pending items..."
echo "  Local data (mutants, coverage, architecture)..."
"$SCRIPT_DIR/summarize.sh"
echo ""
echo "  Generating consolidated summary..."
"$SCRIPT_DIR/generate-pending-summary.sh"
echo ""

# === UNIVERSAL GATE ===
# Gate checks: architecture violations, surviving mutants, coverage gaps, and SonarCloud issues
PENDING_COUNT=$(find artifacts/pending -name "*.txt" ! -name "SUMMARY.txt" 2>/dev/null | wc -l)
PENDING_COUNT=${PENDING_COUNT//[^0-9]/}

if [ "$PENDING_COUNT" -gt 0 ]; then
    echo "STATUS: FAILED ($PENDING_COUNT pending items)"
    echo ""
    echo "Next steps:"
    echo "  1. Read artifacts/pending/SUMMARY.txt for overview"
    echo "  2. Check individual files in artifacts/pending/ for details"
    echo "  3. Fix all pending items"
    echo "  4. Run pipeline again"
    exit 1
else
    echo "STATUS: SUCCESS"
fi
