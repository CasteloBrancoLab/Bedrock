#!/bin/bash
# Executa benchmarks de performance sustentados e gera relatorios
# Uso: ./scripts/benchmark.sh [--filter <pattern>]
#   Sem argumentos: executa todos os benchmarks
#   --filter '*Insert*' : executa benchmarks que correspondem ao padrao

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

ARTIFACTS_DIR="artifacts"
PENDING_DIR="$ARTIFACTS_DIR/pending"

echo "========================================"
echo "  BEDROCK BENCHMARKS"
echo "========================================"
echo ""

# Criar diretorios
mkdir -p "$PENDING_DIR"

# Encontrar projetos de performance tests
PERF_PROJECTS=$(find tests/PerformanceTests -name "*.csproj" 2>/dev/null || true)

if [ -z "$PERF_PROJECTS" ]; then
    echo "Nenhum projeto de performance tests encontrado em tests/PerformanceTests/"
    exit 0
fi

# Capturar argumentos como array para preservar wildcards
RUNNER_ARGS=()
if [ "$1" = "--filter" ] && [ -n "$2" ]; then
    RUNNER_ARGS=("--filter" "$2")
    echo "Filtro: $2"
else
    echo "Executando todos os benchmarks"
fi

echo ""

# Build em Release
echo ">>> Compilando projetos de benchmark..."
for project in $PERF_PROJECTS; do
    echo "  Building: $project"
    dotnet build "$project" -c Release --nologo -v quiet
done
echo ""

# Executar benchmarks
TOTAL_PROJECTS=0
FAILED_PROJECTS=0

for project in $PERF_PROJECTS; do
    TOTAL_PROJECTS=$((TOTAL_PROJECTS + 1))
    project_dir=$(dirname "$project")
    project_name=$(basename "$project_dir")

    echo ">>> Executando benchmarks: $project_name"
    echo "    Projeto: $project"
    echo ""

    if dotnet run --project "$project" -c Release --no-build -- \
        "${RUNNER_ARGS[@]}"; then
        echo ""
        echo "  [OK] $project_name concluido"
    else
        echo ""
        echo "  [FALHOU] $project_name"
        FAILED_PROJECTS=$((FAILED_PROJECTS + 1))
    fi
    echo ""
done

# Resumo
echo "========================================"
echo "  RESUMO"
echo "========================================"
echo "  Projetos: $TOTAL_PROJECTS"
echo "  Falhas: $FAILED_PROJECTS"

# Verificar arquivos de pendencias
BENCHMARK_COUNT=$(find "$PENDING_DIR" -name "benchmark_*.txt" 2>/dev/null | wc -l | tr -d ' ')
WARN_COUNT=$(grep -rl "STATUS: WARN" "$PENDING_DIR"/benchmark_*.txt 2>/dev/null | wc -l | tr -d ' ')

echo "  Benchmarks: $BENCHMARK_COUNT"
echo "  Warnings (memory growth): $WARN_COUNT"
echo ""

if [ "$FAILED_PROJECTS" -gt 0 ]; then
    echo "RESULTADO: FALHOU ($FAILED_PROJECTS projetos com erro)"
    exit 1
fi

echo "RESULTADO: OK"
echo ""
echo "Artefatos:"
echo "  Pendencias (LLM-friendly): $PENDING_DIR/benchmark_*.txt"
echo "  Samples (timeline): $PENDING_DIR/benchmark_*_samples.json"
echo "  Resumo: $PENDING_DIR/SUMMARY.txt"

# Gerar relatório HTML
echo ""
echo ">>> Gerando relatório de benchmarks..."
"$SCRIPT_DIR/generate-benchmark-report.sh"
