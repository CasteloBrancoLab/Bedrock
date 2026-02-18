#!/bin/bash
# generate-pending-summary.sh - Gera SUMMARY.txt consolidado a partir de todos os arquivos em artifacts/pending
set -e

PENDING_DIR="artifacts/pending"
SUMMARY_FILE="$PENDING_DIR/SUMMARY.txt"

echo "=== GENERATE PENDING SUMMARY ==="

mkdir -p "$PENDING_DIR"

# Conta pendências por categoria
ARCH_PENDING=$(find "$PENDING_DIR" -name "architecture_*.txt" 2>/dev/null | wc -l)
ARCH_PENDING=${ARCH_PENDING//[^0-9]/}

MUTATION_PENDING=$(find "$PENDING_DIR" -name "mutant_*.txt" 2>/dev/null | wc -l)
MUTATION_PENDING=${MUTATION_PENDING//[^0-9]/}

SONAR_PENDING=$(find "$PENDING_DIR" -name "sonar_*.txt" 2>/dev/null | wc -l)
SONAR_PENDING=${SONAR_PENDING//[^0-9]/}

COVERAGE_PENDING=$(find "$PENDING_DIR" -name "coverage_*.txt" 2>/dev/null | wc -l)
COVERAGE_PENDING=${COVERAGE_PENDING//[^0-9]/}

# Gera cabeçalho
{
    echo "========================================"
    echo "  PENDENCIAS - $(date '+%Y-%m-%d %H:%M:%S')"
    echo "========================================"
    echo ""
    echo "VIOLACOES ARQUITETURA: $ARCH_PENDING"
    echo "MUTANTES PENDENTES: $MUTATION_PENDING"
    echo "COBERTURA PENDENTE: $COVERAGE_PENDING"
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
    echo "----------------------------------------"
    echo "COBERTURA (arquivos < 100%):"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

# Lista arquivos com cobertura incompleta
coverage_files=$(find "$PENDING_DIR" -name "coverage_*.txt" 2>/dev/null | sort || true)
for f in $coverage_files; do
    if [ -f "$f" ]; then
        file=$(grep "^FILE:" "$f" | cut -d: -f2- | xargs)
        lineRate=$(grep "^LINE_RATE:" "$f" | cut -d: -f2- | xargs)
        uncovered=$(grep "^UNCOVERED_LINES:" "$f" | cut -d: -f2- | xargs)
        # Count uncovered lines
        uncoveredCount=$(echo "$uncovered" | tr ',' '\n' | wc -l)
        uncoveredCount=${uncoveredCount//[^0-9]/}
        echo "  [$lineRate] $file ($uncoveredCount uncovered lines)" >> "$SUMMARY_FILE"
    fi
done

{
    echo ""
    echo "----------------------------------------"
    echo "VIOLACOES DE ARQUITETURA:"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

# Lista architecture violations resumidas
architecture_files=$(find "$PENDING_DIR" -name "architecture_*.txt" 2>/dev/null | sort || true)
for f in $architecture_files; do
    if [ -f "$f" ]; then
        rule=$(grep "^RULE:" "$f" | cut -d: -f2 | tr -d ' ')
        severity=$(grep "^SEVERITY:" "$f" | cut -d: -f2 | tr -d ' ')
        file=$(grep "^FILE:" "$f" | cut -d: -f2- | xargs)
        line=$(grep "^LINE:" "$f" | cut -d: -f2 | tr -d ' ')
        echo "  [$severity] $rule - $file:$line" >> "$SUMMARY_FILE"
    fi
done

{
    echo ""
    echo "Detalhes em: $PENDING_DIR/"
} >> "$SUMMARY_FILE"

echo "Done: Summary generated at $SUMMARY_FILE"
echo ""
cat "$SUMMARY_FILE"
