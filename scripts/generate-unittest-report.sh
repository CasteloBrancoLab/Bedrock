#!/bin/bash
# Gera relatório HTML a partir dos resultados de testes unitários e cobertura
# Usa o projeto UnitTestReportGenerator em tools/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

COVERAGE_DIR="artifacts/coverage/raw"
REPORT_DIR="artifacts/unittest-report"
OUTPUT_FILE="$REPORT_DIR/index.html"
GENERATOR_PROJECT="tools/UnitTestReportGenerator/UnitTestReportGenerator.csproj"
CI_WORKFLOW="$ROOT_DIR/.github/workflows/ci.yml"

echo ">>> Gerando Relatório de Testes Unitários..."

# Criar diretório do relatório
mkdir -p "$REPORT_DIR"

# Verificar se há resultados de cobertura
HAS_COVERAGE=$(find "$COVERAGE_DIR" -name "*.cobertura.xml" -maxdepth 1 2>/dev/null | head -1)

if [ -z "$HAS_COVERAGE" ]; then
    echo "Nenhum arquivo de cobertura encontrado em $COVERAGE_DIR"
    echo "Pulando geração de relatório."
    exit 0
fi

# Obter informações do git
GIT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
GIT_COMMIT=$(git rev-parse HEAD 2>/dev/null || echo "unknown")

# Executar o gerador de relatório
echo "Executando gerador de relatório..."
dotnet run --project "$GENERATOR_PROJECT" --configuration Release -- "$COVERAGE_DIR" "$OUTPUT_FILE" "$GIT_BRANCH" "$GIT_COMMIT" "$CI_WORKFLOW"

echo ">>> Relatório gerado: $OUTPUT_FILE"
