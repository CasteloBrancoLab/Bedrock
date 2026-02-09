#!/bin/bash
# Executa testes de mutação com Stryker, gerando JSON

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== MUTATE ==="

mkdir -p artifacts/mutation

FAILED=0

for config in $(find tests/MutationTests -name "stryker-config.json"); do
    dir=$(dirname "$config")
    name=$(basename "$dir")
    echo "Running mutation tests for: $name"

    # Verificar se o projeto src tem código antes de rodar Stryker
    src_project=$(grep -o '"project":.*"' "$ROOT_DIR/$config" | head -1 | sed 's/"project":[[:space:]]*"//;s/"$//')
    if [ -n "$src_project" ]; then
        src_csproj=$(find "$ROOT_DIR" -name "$src_project" -not -path "*/bin/*" -not -path "*/obj/*" | head -1)
        if [ -n "$src_csproj" ]; then
            src_dir=$(dirname "$src_csproj")
            cs_count=$(find "$src_dir" -name "*.cs" ! -name "GlobalUsings.cs" -not -path "*/obj/*" -not -path "*/bin/*" 2>/dev/null | wc -l)
            if [ "$cs_count" -eq 0 ]; then
                echo "  $name: SKIPPED (no source files to mutate)"
                continue
            fi
        fi
    fi

    cd "$dir"

    # Usar reporter JSON em vez de HTML para consumo pelo code agent
    if dotnet stryker \
        -O "$ROOT_DIR/artifacts/mutation/$name" \
        --reporter json \
        --reporter progress; then
        echo "  $name: PASSED"
    else
        echo "  $name: FAILED (threshold not met)"
        FAILED=1
    fi

    cd "$ROOT_DIR"
done

echo ""
echo "Done: Mutation tests completed"
echo "Reports available in artifacts/mutation/"

if [ $FAILED -eq 1 ]; then
    echo "WARNING: One or more mutation test projects failed threshold requirements"
    exit 1
fi
