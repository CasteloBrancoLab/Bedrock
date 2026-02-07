#!/bin/bash
# Gera relatorio HTML a partir dos resultados de benchmarks
# Usa o projeto BenchmarkReportGenerator em tools/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

BENCHMARK_DIR="artifacts/benchmark"
PENDING_DIR="artifacts/pending"
REPORT_DIR="artifacts/benchmark-report"
OUTPUT_FILE="$REPORT_DIR/index.html"
GENERATOR_PROJECT="tools/BenchmarkReportGenerator/BenchmarkReportGenerator.csproj"

echo ">>> Gerando Relatorio de Benchmarks..."

# Criar diretorio do relatorio
mkdir -p "$REPORT_DIR"

# Verificar se ha resultados de benchmarks
# SustainedBenchmarkRunner outputs to pending/ locally, but CI copies to benchmark/
HAS_PENDING=$(find "$PENDING_DIR" -name "benchmark_*.txt" 2>/dev/null | head -1)
HAS_BENCHMARK=$(find "$BENCHMARK_DIR" -name "benchmark_*.txt" 2>/dev/null | head -1)
HAS_BDN=$(find "$BENCHMARK_DIR" -name "*-report-full.json" 2>/dev/null | head -1)

if [ -z "$HAS_PENDING" ] && [ -z "$HAS_BENCHMARK" ] && [ -z "$HAS_BDN" ]; then
    echo "Nenhum resultado de benchmark encontrado."
    echo "  Diretorio de pendencias: $PENDING_DIR"
    echo "  Diretorio de benchmarks: $BENCHMARK_DIR"
    echo "Pulando geracao de relatorio."
    exit 0
fi

# Obter informacoes do git
GIT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
GIT_COMMIT=$(git rev-parse HEAD 2>/dev/null || echo "unknown")

# Executar o gerador de relatorio
echo "Executando gerador de relatorio..."
dotnet run --project "$GENERATOR_PROJECT" --configuration Release -- "$BENCHMARK_DIR" "$PENDING_DIR" "$OUTPUT_FILE" "$GIT_BRANCH" "$GIT_COMMIT"

echo ">>> Relatorio gerado: $OUTPUT_FILE"
