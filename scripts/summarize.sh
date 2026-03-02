#!/bin/bash
# summarize.sh - Generates SUMMARY.txt from all files in artifacts/pending/
# Usage: ./scripts/summarize.sh

source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
lib_init

PENDING_DIR="artifacts/pending"
SUMMARY_FILE="$PENDING_DIR/SUMMARY.txt"

echo "=== GENERATE PENDING SUMMARY ==="

# Count pending items by category
BUILD_PENDING=$(count_pending "build_*.txt")
ARCH_PENDING=$(count_pending "architecture_*.txt")
TEST_PENDING=$(count_pending "test_*.txt")
MUTATION_PENDING=$(count_pending "mutant_*.txt")
INTEGRATION_PENDING=$(count_pending "integration_*.txt")
SONAR_PENDING=$(count_pending "sonar_*.txt")
COVERAGE_PENDING=$(count_pending "coverage_*.txt")

# Generate header
{
    echo "========================================"
    echo "  PENDENCIAS - $(date '+%Y-%m-%d %H:%M:%S')"
    echo "========================================"
    echo ""
    echo "ERROS DE BUILD: $BUILD_PENDING"
    echo "VIOLACOES ARQUITETURA: $ARCH_PENDING"
    echo "TESTES FALHANDO: $TEST_PENDING"
    echo "MUTANTES PENDENTES: $MUTATION_PENDING"
    echo "TESTES INTEGRACAO FALHANDO: $INTEGRATION_PENDING"
    echo "COBERTURA PENDENTE: $COVERAGE_PENDING"
    echo "SONARCLOUD ISSUES: $SONAR_PENDING"
} > "$SUMMARY_FILE"

# === BUILD ERRORS ===
{
    echo ""
    echo "----------------------------------------"
    echo "ERROS DE BUILD:"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

build_files=$(find "$PENDING_DIR" -name "build_*.txt" 2>/dev/null | sort || true)
for f in $build_files; do
    if [ -f "$f" ]; then
        while IFS= read -r line; do
            case "$line" in
                "FILE: "*)   file="${line#FILE: }" ;;
                "LINE: "*)   line_num="${line#LINE: }" ;;
                "CODE: "*)   code="${line#CODE: }" ;;
                "MESSAGE: "*) msg="${line#MESSAGE: }"; echo "  [$code] $file:$line_num - $msg" >> "$SUMMARY_FILE" ;;
            esac
        done < "$f"
    fi
done

# === ARCHITECTURE VIOLATIONS ===
{
    echo ""
    echo "----------------------------------------"
    echo "VIOLACOES DE ARQUITETURA:"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

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

# === TEST FAILURES ===
{
    echo ""
    echo "----------------------------------------"
    echo "TESTES FALHANDO:"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

test_files=$(find "$PENDING_DIR" -name "test_*.txt" 2>/dev/null | sort || true)
for f in $test_files; do
    if [ -f "$f" ]; then
        project=$(grep "^PROJECT:" "$f" | cut -d: -f2- | xargs)
        test_name=$(grep "^TEST:" "$f" | cut -d: -f2- | xargs)
        msg=$(grep "^MESSAGE:" "$f" | cut -d: -f2- | xargs)
        echo "  [$project] $test_name" >> "$SUMMARY_FILE"
    fi
done

# === SURVIVING MUTANTS ===
{
    echo ""
    echo "----------------------------------------"
    echo "MUTANTES SOBREVIVENTES:"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

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

# === INTEGRATION TEST FAILURES ===
{
    echo ""
    echo "----------------------------------------"
    echo "TESTES INTEGRACAO FALHANDO:"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

integration_files=$(find "$PENDING_DIR" -name "integration_*.txt" 2>/dev/null | sort || true)
for f in $integration_files; do
    if [ -f "$f" ]; then
        project=$(grep "^PROJECT:" "$f" | cut -d: -f2- | xargs)
        test_name=$(grep "^TEST:" "$f" | cut -d: -f2- | xargs)
        echo "  [$project] $test_name" >> "$SUMMARY_FILE"
    fi
done

# === COVERAGE ===
{
    echo ""
    echo "----------------------------------------"
    echo "COBERTURA (arquivos < 100%):"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

coverage_files=$(find "$PENDING_DIR" -name "coverage_*.txt" 2>/dev/null | sort || true)
for f in $coverage_files; do
    if [ -f "$f" ]; then
        file=$(grep "^FILE:" "$f" | cut -d: -f2- | xargs)
        lineRate=$(grep "^LINE_RATE:" "$f" | cut -d: -f2- | xargs)
        uncovered=$(grep "^UNCOVERED_LINES:" "$f" | cut -d: -f2- | xargs)
        uncoveredCount=$(echo "$uncovered" | tr ',' '\n' | wc -l)
        uncoveredCount=${uncoveredCount//[^0-9]/}
        echo "  [$lineRate] $file ($uncoveredCount uncovered lines)" >> "$SUMMARY_FILE"
    fi
done

# === SONARCLOUD ===
{
    echo ""
    echo "----------------------------------------"
    echo "ISSUES DO SONARCLOUD:"
    echo "----------------------------------------"
} >> "$SUMMARY_FILE"

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
