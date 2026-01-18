#!/bin/bash
# summarize.sh - Extrai pendências de cobertura, mutação e SonarCloud para arquivos digestíveis
set -e

ARTIFACTS_DIR="artifacts"
PENDING_DIR="$ARTIFACTS_DIR/pending"
SUMMARY_FILE="$PENDING_DIR/SUMMARY.txt"

# Configuração SonarCloud
SONAR_PROJECT="CasteloBrancoLab_Bedrock"
SONAR_API_URL="https://sonarcloud.io/api/issues/search"

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
# ISSUES DO SONARCLOUD
# ========================================
echo "Fetching SonarCloud issues..."

SONAR_PENDING=0
TEMP_JSON="$ARTIFACTS_DIR/.sonar_response.json"

# Carrega SONAR_TOKEN do .env se existir
if [ -f ".env" ]; then
    # shellcheck disable=SC1091
    source .env 2>/dev/null || true
fi

# Verifica se SONAR_TOKEN está disponível
if [ -z "${SONAR_TOKEN:-}" ]; then
    echo "Warning: SONAR_TOKEN not set. Skipping SonarCloud issues fetch."
    echo "To enable SonarCloud integration, create a .env file with SONAR_TOKEN=<your-token>"
    SONAR_PENDING=0
else
    # Busca todas as issues abertas do SonarCloud (paginado)
    page=1
    page_size=100
    total=0
    issue_index=0

    while true; do
        # Busca issues e salva em arquivo temporário (com autenticação)
        curl -s -u "${SONAR_TOKEN}:" "${SONAR_API_URL}?componentKeys=${SONAR_PROJECT}&resolved=false&ps=${page_size}&p=${page}" > "$TEMP_JSON" 2>/dev/null || echo "{}" > "$TEMP_JSON"

        # Verifica se a resposta é válida
        if ! grep -q '"issues"' "$TEMP_JSON"; then
            echo "Warning: Could not fetch SonarCloud issues (page $page)"
            break
        fi

        # Extrai total de issues na primeira página
        if [ $page -eq 1 ]; then
            total=$(powershell -NoProfile -Command "
                \$json = Get-Content '$TEMP_JSON' -Raw | ConvertFrom-Json
                Write-Host \$json.total
            " 2>/dev/null || echo "0")
            total=${total//[^0-9]/}
            [ -z "$total" ] && total=0
            echo "Total SonarCloud issues: $total"
        fi

        # Processa cada issue da página
        issues_processed=$(powershell -NoProfile -Command "
            \$json = Get-Content '$TEMP_JSON' -Raw | ConvertFrom-Json
            \$issueIndex = $issue_index

            foreach (\$issue in \$json.issues) {
                \$issueIndex++

                \$type = \$issue.type
                \$severity = \$issue.severity
                \$component = \$issue.component -replace '^${SONAR_PROJECT}:', ''
                \$line = if (\$issue.line) { \$issue.line } else { 'N/A' }
                \$message = \$issue.message -replace '[\r\n]+', ' '
                \$rule = \$issue.rule
                \$effort = if (\$issue.effort) { \$issue.effort } else { 'N/A' }

                \$typePrefix = switch (\$type) {
                    'BUG' { 'bug' }
                    'VULNERABILITY' { 'vuln' }
                    'CODE_SMELL' { 'smell' }
                    'SECURITY_HOTSPOT' { 'hotspot' }
                    default { 'issue' }
                }

                \$outFile = '$PENDING_DIR/sonar_' + \$typePrefix + '_' + \$issueIndex.ToString('D3') + '.txt'

                @(
                    \"TYPE: \$type\",
                    \"SEVERITY: \$severity\",
                    \"FILE: \$component\",
                    \"LINE: \$line\",
                    \"RULE: \$rule\",
                    \"EFFORT: \$effort\",
                    \"MESSAGE: \$message\"
                ) | Out-File -FilePath \$outFile -Encoding UTF8
            }

            Write-Host \$issueIndex
        " 2>/dev/null || echo "$issue_index")

        issues_processed=${issues_processed//[^0-9]/}
        [ -z "$issues_processed" ] && issues_processed=$issue_index

        # Se não processou novas issues, sai do loop
        if [ "$issues_processed" -eq "$issue_index" ]; then
            break
        fi

        issue_index=$issues_processed

        # Verifica se há mais páginas
        if [ "$issue_index" -ge "$total" ]; then
            break
        fi

        page=$((page + 1))
    done

    # Limpa arquivo temporário
    rm -f "$TEMP_JSON"

    SONAR_PENDING=$(find "$PENDING_DIR" -name "sonar_*.txt" 2>/dev/null | wc -l)
    SONAR_PENDING=${SONAR_PENDING//[^0-9]/}

    echo "Found $SONAR_PENDING SonarCloud issues"
fi

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
    echo "SONARCLOUD ISSUES: $SONAR_PENDING"
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
    echo "----------------------------------------"
    echo "ISSUES DO SONARCLOUD:"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

# Lista issues do SonarCloud resumidas
sonar_files=$(find "$PENDING_DIR" -name "sonar_*.txt" 2>/dev/null | sort || true)
for f in $sonar_files; do
    if [ -f "$f" ]; then
        type=$(grep "^TYPE:" "$f" | cut -d: -f2 | tr -d ' ')
        severity=$(grep "^SEVERITY:" "$f" | cut -d: -f2 | tr -d ' ')
        file=$(grep "^FILE:" "$f" | cut -d: -f2- | xargs)
        line=$(grep "^LINE:" "$f" | cut -d: -f2 | tr -d ' ')
        rule=$(grep "^RULE:" "$f" | cut -d: -f2 | tr -d ' ')
        echo "  [$severity] $type - $file:$line ($rule)" >> "$SUMMARY_FILE"
    fi
done

{
    echo ""
    echo "Detalhes em: $PENDING_DIR/"
} >> "$SUMMARY_FILE"

echo "Done: Summary generated at $SUMMARY_FILE"
echo ""
cat "$SUMMARY_FILE"
