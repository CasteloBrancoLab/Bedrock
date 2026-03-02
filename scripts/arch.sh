#!/bin/bash
# arch.sh - Runs architecture tests and extracts violations to artifacts/pending/
# Usage: ./scripts/arch.sh
# Output: artifacts/pending/architecture_*.txt (if violations)
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
lib_init

echo "=== ARCHITECTURE ==="

clean_pending "architecture"
mkdir -p artifacts/architecture

# Build silently (architecture tests use --no-build)
echo "Building solution..."
dotnet build > /dev/null 2>&1

# Discover all architecture test projects
ARCH_PROJECTS=$(find tests/ArchitectureTests -name "*.csproj" 2>/dev/null)

if [ -z "$ARCH_PROJECTS" ]; then
    echo "No architecture test projects found in tests/ArchitectureTests/"
    "$SCRIPT_DIR/summarize.sh" 2>/dev/null || true
    exit 0
fi

echo "Running architecture tests..."

ARCH_FAILED=0
while IFS= read -r project; do
    project_name=$(basename "$project" .csproj)
    echo "  Testing: $project_name"
    dotnet test "$project" \
        --no-build \
        --logger "trx;LogFileName=architecture-${project_name}.trx" \
        --results-directory "artifacts/architecture" \
        || ARCH_FAILED=1
done <<< "$ARCH_PROJECTS"

# Generate HTML report if JSON artifacts exist
ARCH_JSON_COUNT=$(find "artifacts/architecture" -mindepth 2 -name "architecture-report.json" 2>/dev/null | wc -l)
ARCH_JSON_COUNT=${ARCH_JSON_COUNT//[^0-9]/}
if [ "$ARCH_JSON_COUNT" -gt 0 ]; then
    "$SCRIPT_DIR/report-architecture.sh" || echo "Warning: Architecture report generation failed"
fi

# Count violations
VIOLATION_COUNT=$(count_pending "architecture_*.txt")

"$SCRIPT_DIR/summarize.sh" 2>/dev/null || true

if [ "$ARCH_FAILED" -eq 1 ]; then
    echo "ARCHITECTURE: FAILED ($VIOLATION_COUNT violations)"
    exit 1
fi

echo "ARCHITECTURE: SUCCESS"
exit 0
