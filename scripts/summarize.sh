#!/bin/bash
# summarize.sh - Extrai pendências de cobertura e mutação para arquivos digestíveis
set -e

ARTIFACTS_DIR="artifacts"
PENDING_DIR="$ARTIFACTS_DIR/pending"
SUMMARY_FILE="$PENDING_DIR/SUMMARY.txt"

echo "=== SUMMARIZE ==="

# Cria diretório de pendências
mkdir -p "$PENDING_DIR"
rm -rf "${PENDING_DIR:?}"/*

# ========================================
# MUTANTES SOBREVIVENTES
# ========================================
echo "Extracting surviving mutants..."

for report in "$ARTIFACTS_DIR"/mutation/*/reports/mutation-report.json; do
    if [ ! -f "$report" ]; then
        continue
    fi

    project=$(basename "$(dirname "$(dirname "$report")")")

    # Usa PowerShell para processar JSON
    powershell -NoProfile -Command "
        \$json = Get-Content '$report' | ConvertFrom-Json
        \$files = \$json.files.PSObject.Properties
        \$mutantIndex = 0

        foreach (\$file in \$files) {
            \$filePath = \$file.Name
            foreach (\$mutant in \$file.Value.mutants) {
                if (\$mutant.status -eq 'Survived' -or \$mutant.status -eq 'NoCoverage') {
                    \$mutantIndex++
                    \$line = \$mutant.location.start.line
                    \$mutator = \$mutant.mutatorName
                    \$status = \$mutant.status
                    \$desc = \$mutant.description

                    \$outFile = '$PENDING_DIR/mutant_${project}_' + \$mutantIndex.ToString('D3') + '.txt'

                    \"PROJECT: $project\" | Out-File -FilePath \$outFile -Encoding UTF8
                    \"FILE: \$filePath\" | Out-File -FilePath \$outFile -Append -Encoding UTF8
                    \"LINE: \$line\" | Out-File -FilePath \$outFile -Append -Encoding UTF8
                    \"STATUS: \$status\" | Out-File -FilePath \$outFile -Append -Encoding UTF8
                    \"MUTATOR: \$mutator\" | Out-File -FilePath \$outFile -Append -Encoding UTF8
                    \"DESCRIPTION: \$desc\" | Out-File -FilePath \$outFile -Append -Encoding UTF8
                }
            }
        }
        Write-Host \$mutantIndex
    " 2>/dev/null || echo "0"
done

# Conta mutantes pendentes
MUTATION_PENDING=$(find "$PENDING_DIR" -name "mutant_*.txt" 2>/dev/null | wc -l)

# ========================================
# COBERTURA INSUFICIENTE
# ========================================
echo "Extracting uncovered lines..."

# Encontra arquivos de cobertura
coverage_files=$(find "$ARTIFACTS_DIR/coverage/raw" -name "coverage.cobertura.xml" 2>/dev/null || true)

for coverage in $coverage_files; do
    if [ ! -f "$coverage" ]; then
        continue
    fi

    # Determina o projeto a partir do path
    project_dir=$(dirname "$coverage")
    project=$(basename "$(dirname "$project_dir")")

    # Extrai linhas não cobertas usando PowerShell
    powershell -NoProfile -Command "
        [xml]\$xml = Get-Content '$coverage'
        \$uncoveredIndex = 0

        foreach (\$pkg in \$xml.coverage.packages.package) {
            foreach (\$class in \$pkg.classes.class) {
                \$fileName = \$class.filename
                \$uncoveredLines = @()

                foreach (\$line in \$class.lines.line) {
                    if (\$line.hits -eq '0') {
                        \$uncoveredLines += \$line.number
                    }
                }

                if (\$uncoveredLines.Count -gt 0) {
                    \$uncoveredIndex++
                    \$outFile = '$PENDING_DIR/coverage_${project}_' + \$uncoveredIndex.ToString('D3') + '.txt'

                    \"PROJECT: $project\" | Out-File -FilePath \$outFile -Encoding UTF8
                    \"FILE: \$fileName\" | Out-File -FilePath \$outFile -Append -Encoding UTF8
                    \"UNCOVERED_LINES: \$(\$uncoveredLines -join ', ')\" | Out-File -FilePath \$outFile -Append -Encoding UTF8
                    \"COUNT: \$(\$uncoveredLines.Count)\" | Out-File -FilePath \$outFile -Append -Encoding UTF8
                }
            }
        }
        Write-Host \$uncoveredIndex
    " 2>/dev/null || echo "0"
done

# Conta arquivos de cobertura pendente
COVERAGE_PENDING=$(find "$PENDING_DIR" -name "coverage_*.txt" 2>/dev/null | wc -l)

# ========================================
# GERA SUMÁRIO CONSOLIDADO
# ========================================
echo "Generating summary..."

{
    echo "========================================"
    echo "  PENDENCIAS - $(date '+%Y-%m-%d %H:%M:%S')"
    echo "========================================"
    echo ""
    echo "MUTANTES PENDENTES: $MUTATION_PENDING"
    echo "COBERTURA PENDENTE: $COVERAGE_PENDING arquivos"
    echo ""
    echo "----------------------------------------"
    echo "MUTANTES SOBREVIVENTES:"
    echo "----------------------------------------"
} > "$SUMMARY_FILE"

# Lista mutantes resumidos
mutant_files=$(find "$PENDING_DIR" -name "mutant_*.txt" 2>/dev/null | sort || true)
for f in $mutant_files; do
    if [ -f "$f" ]; then
        file=$(grep "^FILE:" "$f" | cut -d: -f2- | xargs)
        line=$(grep "^LINE:" "$f" | cut -d: -f2 | tr -d ' ')
        status=$(grep "^STATUS:" "$f" | cut -d: -f2 | tr -d ' ')
        mutator=$(grep "^MUTATOR:" "$f" | cut -d: -f2 | tr -d ' ')
        echo "  [$status] $file:$line - $mutator" >> "$SUMMARY_FILE"
    fi
done

{
    echo ""
    echo "----------------------------------------"
    echo "ARQUIVOS COM COBERTURA INSUFICIENTE:"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

# Lista cobertura resumida
coverage_files_pending=$(find "$PENDING_DIR" -name "coverage_*.txt" 2>/dev/null | sort || true)
for f in $coverage_files_pending; do
    if [ -f "$f" ]; then
        file=$(grep "^FILE:" "$f" | cut -d: -f2- | xargs)
        count=$(grep "^COUNT:" "$f" | cut -d: -f2 | tr -d ' ')
        echo "  $file ($count linhas)" >> "$SUMMARY_FILE"
    fi
done

{
    echo ""
    echo "Detalhes em: $PENDING_DIR/"
} >> "$SUMMARY_FILE"

echo "Done: Summary generated at $SUMMARY_FILE"
echo ""
cat "$SUMMARY_FILE"
