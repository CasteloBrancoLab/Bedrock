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
            total=$(grep -o '"total"[[:space:]]*:[[:space:]]*[0-9]*' "$TEMP_JSON" | head -1 | grep -o '[0-9]*$')
            total=${total:-0}
            total=${total//[^0-9]/}
            echo "Total SonarCloud issues: $total"
        fi

        # Processa cada issue da página usando bash puro
        # Flatten JSON: coloca cada issue em uma linha separada
        prev_index=$issue_index

        while IFS= read -r issue_line; do
            [ -z "$issue_line" ] && continue

            type=$(printf '%s' "$issue_line" | grep -o '"type":"[^"]*"' | head -1 | cut -d'"' -f4)
            [ -z "$type" ] && continue

            severity=$(printf '%s' "$issue_line" | grep -o '"severity":"[^"]*"' | head -1 | cut -d'"' -f4)
            component=$(printf '%s' "$issue_line" | grep -o '"component":"[^"]*"' | head -1 | cut -d'"' -f4)
            rule=$(printf '%s' "$issue_line" | grep -o '"rule":"[^"]*"' | head -1 | cut -d'"' -f4)
            message=$(printf '%s' "$issue_line" | grep -o '"message":"[^"]*"' | head -1 | cut -d'"' -f4)
            effort=$(printf '%s' "$issue_line" | grep -o '"effort":"[^"]*"' | head -1 | cut -d'"' -f4)
            i_line=$(printf '%s' "$issue_line" | grep -o '"line":[0-9]*' | head -1 | grep -o '[0-9]*$')

            [ -z "$i_line" ] && i_line="N/A"
            [ -z "$effort" ] && effort="N/A"

            # Strip SonarCloud project prefix from component
            component="${component#${SONAR_PROJECT}:}"

            # Determine type prefix
            case "$type" in
                BUG) prefix="bug" ;;
                VULNERABILITY) prefix="vuln" ;;
                CODE_SMELL) prefix="smell" ;;
                SECURITY_HOTSPOT) prefix="hotspot" ;;
                *) prefix="issue" ;;
            esac

            issue_index=$((issue_index + 1))
            idx=$(printf "%03d" "$issue_index")

            cat > "$PENDING_DIR/sonar_${prefix}_${idx}.txt" << EOF
TYPE: $type
SEVERITY: $severity
FILE: $component
LINE: $i_line
RULE: $rule
EFFORT: $effort
MESSAGE: $message
EOF
        done < <(tr '\n' ' ' < "$TEMP_JSON" | sed 's/{"key"/\n{"key"/g' | grep '{"key"')

        # Se não processou novas issues, sai do loop
        if [ "$issue_index" -eq "$prev_index" ]; then
            break
        fi

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
