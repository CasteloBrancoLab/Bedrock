#!/bin/bash
# Executa testes de arquitetura via Roslyn SDK
# Gera artefatos em artifacts/architecture/ e violations em artifacts/pending/architecture_*.txt

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== ARCHITECTURE ==="

mkdir -p artifacts/architecture
mkdir -p artifacts/pending

# Descobrir todos os projetos de testes de arquitetura
ARCH_PROJECTS=$(find tests/ArchitectureTests -name "*.csproj" 2>/dev/null)

if [ -z "$ARCH_PROJECTS" ]; then
    echo "Nenhum projeto de testes de arquitetura encontrado em tests/ArchitectureTests/"
    exit 1
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

# Verificar se há violations pendentes
ARCH_PENDING=$(find "artifacts/pending" -name "architecture_*.txt" 2>/dev/null | wc -l)
ARCH_PENDING=${ARCH_PENDING//[^0-9]/}

echo ""
echo "Done: Architecture tests completed"
echo "  Violations: $ARCH_PENDING"
echo "  Report JSON: artifacts/architecture/architecture-report.json"

# Gerar relatório HTML (mesmo com violações)
if [ -f "artifacts/architecture/architecture-report.json" ]; then
    "$SCRIPT_DIR/generate-architecture-report.sh" || echo "Warning: Architecture report generation failed"
fi

if [ "$ARCH_FAILED" -eq 1 ]; then
    echo "WARNING: Architecture tests failed - violations found"
    exit 1
fi
