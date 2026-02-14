#!/bin/bash
# summarize.sh - Extrai pendências locais (mutação, cobertura, arquitetura) para artifacts/pending
# SonarCloud: ver sonar-check.sh
# Sumário consolidado: ver generate-pending-summary.sh
set -e

ARTIFACTS_DIR="artifacts"
PENDING_DIR="$ARTIFACTS_DIR/pending"

echo "=== SUMMARIZE ==="

# Cria diretório de pendências (limpa apenas arquivos locais, preserva sonar_*.txt de execuções anteriores)
mkdir -p "$PENDING_DIR"
rm -f "$PENDING_DIR"/mutant_*.txt "$PENDING_DIR"/SUMMARY.txt 2>/dev/null || true

# ========================================
# VIOLAÇÕES DE ARQUITETURA
# ========================================
echo "Counting architecture violations..."

ARCH_PENDING=$(find "$PENDING_DIR" -name "architecture_*.txt" 2>/dev/null | wc -l)
ARCH_PENDING=${ARCH_PENDING//[^0-9]/}
echo "Found $ARCH_PENDING architecture violations"

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
MUTATION_PENDING=${MUTATION_PENDING//[^0-9]/}
echo "Found $MUTATION_PENDING surviving mutants"

# ========================================
# COBERTURA (delegada ao SonarCloud)
# ========================================
# Coverage is not extracted locally because Coverlet reports include transitive
# dependencies, producing false positives. SonarCloud is the authority on coverage.
echo "Coverage: delegated to SonarCloud (see sonar-check.sh)"

echo "Done: Local pending items extracted to $PENDING_DIR/"
