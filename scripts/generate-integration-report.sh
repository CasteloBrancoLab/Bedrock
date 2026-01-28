#!/bin/bash
# Gera relatório HTML a partir de arquivos TRX de testes de integração
# Usa o projeto IntegrationReportGenerator em tools/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

REPORT_DIR="artifacts/integration-report"
TRX_DIR="artifacts/test-results"
OUTPUT_FILE="$REPORT_DIR/index.html"
GENERATOR_PROJECT="tools/IntegrationReportGenerator/IntegrationReportGenerator.csproj"

echo ">>> Gerando Relatório de Testes de Integração..."

# Criar diretório do relatório
mkdir -p "$REPORT_DIR"

# Encontrar arquivos TRX de testes de integração
TRX_FILES=$(find "$TRX_DIR" -name "integration-*.trx" 2>/dev/null | tr '\n' ';')

if [ -z "$TRX_FILES" ]; then
    echo "Nenhum arquivo TRX de testes de integração encontrado em $TRX_DIR"
    echo "Pulando geração de relatório."
    exit 0
fi

echo "Arquivos TRX encontrados: $TRX_FILES"

# Obter informações do git
GIT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
GIT_COMMIT=$(git rev-parse HEAD 2>/dev/null || echo "unknown")

# Executar o gerador de relatório
echo "Executando gerador de relatório..."
dotnet run --project "$GENERATOR_PROJECT" --configuration Release -- "$TRX_FILES" "$OUTPUT_FILE" "$GIT_BRANCH" "$GIT_COMMIT"

echo ">>> Relatório gerado: $OUTPUT_FILE"
