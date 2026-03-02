#!/bin/bash
# sonar.sh - Fetches open SonarCloud issues and generates sonar_*.txt in artifacts/pending/
# Usage: ./scripts/sonar.sh
# Output: artifacts/pending/sonar_*.txt (per issue)
#         artifacts/pending/SUMMARY.txt
# Cross-OS: Windows (Git Bash/WSL), macOS, Linux

source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
lib_init

# SonarCloud configuration
SONAR_PROJECT="CasteloBrancoLab_Bedrock"
SONAR_API_URL="https://sonarcloud.io/api/issues/search"

echo "=== SONARCLOUD CHECK ==="

SONAR_PENDING=0
TEMP_JSON="artifacts/.sonar_response.json"

# Load SONAR_TOKEN from .env if available
if [ -f ".env" ]; then
    # shellcheck disable=SC1091
    source .env 2>/dev/null || true
fi

# Check if SONAR_TOKEN is set
if [ -z "${SONAR_TOKEN:-}" ]; then
    echo "Warning: SONAR_TOKEN not set. Skipping SonarCloud issues fetch."
    echo "To enable SonarCloud integration, create a .env file with SONAR_TOKEN=<your-token>"
    SONAR_PENDING=0
else
    # Fetch all open SonarCloud issues (paginated)
    page=1
    page_size=100
    total=0
    issue_index=0

    while true; do
        curl -s -u "${SONAR_TOKEN}:" "${SONAR_API_URL}?componentKeys=${SONAR_PROJECT}&resolved=false&ps=${page_size}&p=${page}" > "$TEMP_JSON" 2>/dev/null || echo "{}" > "$TEMP_JSON"

        if ! grep -q '"issues"' "$TEMP_JSON"; then
            echo "Warning: Could not fetch SonarCloud issues (page $page)"
            break
        fi

        if [ $page -eq 1 ]; then
            total=$(grep -o '"total"[[:space:]]*:[[:space:]]*[0-9]*' "$TEMP_JSON" | head -1 | grep -o '[0-9]*$')
            total=${total:-0}
            total=${total//[^0-9]/}
            echo "Total SonarCloud issues: $total"
        fi

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

            cat > "artifacts/pending/sonar_${prefix}_${idx}.txt" << EOF
TYPE: $type
SEVERITY: $severity
FILE: $component
LINE: $i_line
RULE: $rule
EFFORT: $effort
MESSAGE: $message
EOF
        done < <(tr '\n' ' ' < "$TEMP_JSON" | sed 's/{"key"/\n{"key"/g' | grep '{"key"')

        if [ "$issue_index" -eq "$prev_index" ]; then
            break
        fi

        if [ "$issue_index" -ge "$total" ]; then
            break
        fi

        page=$((page + 1))
    done

    rm -f "$TEMP_JSON"

    SONAR_PENDING=$(count_pending "sonar_*.txt")
    echo "Found $SONAR_PENDING SonarCloud issues"
fi

"$SCRIPT_DIR/summarize.sh" 2>/dev/null || true

echo "Done: SonarCloud check completed"
