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
