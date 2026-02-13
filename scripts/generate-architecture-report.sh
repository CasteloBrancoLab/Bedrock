#!/bin/bash
# Gera relatório HTML a partir dos JSONs de testes de arquitetura
# Usa o projeto ArchitectureReportGenerator em tools/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

JSON_DIR="artifacts/architecture"
REPORT_DIR="artifacts/architecture-report"
OUTPUT_FILE="$REPORT_DIR/index.html"
GENERATOR_PROJECT="tools/ArchitectureReportGenerator/ArchitectureReportGenerator.csproj"

echo ">>> Gerando Relatório de Testes de Arquitetura..."

# Criar diretório do relatório
mkdir -p "$REPORT_DIR"

# Verificar se existem JSONs nas subpastas
JSON_COUNT=$(find "$JSON_DIR" -mindepth 2 -name "architecture-report.json" 2>/dev/null | wc -l)

if [ "$JSON_COUNT" -eq 0 ]; then
    echo "Nenhum relatório JSON encontrado em subpastas de $JSON_DIR"
    echo "Pulando geração de relatório."
    exit 0
fi

echo "Encontrados $JSON_COUNT relatórios JSON."

# Obter informações do git
GIT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")
GIT_COMMIT=$(git rev-parse HEAD 2>/dev/null || echo "unknown")

# Executar o gerador de relatório (passa o diretório com subpastas)
echo "Executando gerador de relatório..."
dotnet run --project "$GENERATOR_PROJECT" --configuration Release -- "$JSON_DIR" "$OUTPUT_FILE" "$GIT_BRANCH" "$GIT_COMMIT"

echo ">>> Relatório gerado: $OUTPUT_FILE"
