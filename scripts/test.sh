#!/bin/bash
# Executa testes unitários com cobertura em formato Cobertura (XML)

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== TEST ==="

mkdir -p artifacts/coverage/raw
mkdir -p artifacts/test-results

TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Find and run all test projects under tests/UnitTests
for project in $(find tests/UnitTests -name "*.csproj"); do
    name=$(basename "$(dirname "$project")")
    echo "Running tests for: $name"

    dotnet test "$project" \
        --no-build \
        --collect:"XPlat Code Coverage" \
        --results-directory "artifacts/coverage/raw/$name" \
        --logger "trx;LogFileName=results.trx" \
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura \
           DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[Bedrock.BuildingBlocks.Testing]*,[Bedrock.Templates.*]*,[Bedrock.Samples.*]*"

    # Copy coverage file to a known location
    coverage_file=$(find "artifacts/coverage/raw/$name" -name "coverage.cobertura.xml" 2>/dev/null | head -1)
    if [ -n "$coverage_file" ]; then
        cp "$coverage_file" "artifacts/coverage/raw/$name.cobertura.xml"
    fi
done

echo "Done: Tests completed"
echo "Coverage reports available in artifacts/coverage/raw/"
