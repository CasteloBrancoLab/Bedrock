#!/bin/bash
# sonar-check.sh - Busca issues abertas do SonarCloud e gera sonar_*.txt em artifacts/pending
set -e

ARTIFACTS_DIR="artifacts"
PENDING_DIR="$ARTIFACTS_DIR/pending"

# Configuração SonarCloud
SONAR_PROJECT="CasteloBrancoLab_Bedrock"
SONAR_API_URL="https://sonarcloud.io/api/issues/search"

echo "=== SONARCLOUD CHECK ==="

mkdir -p "$PENDING_DIR"

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

echo "Done: SonarCloud check completed"
