#!/bin/bash
# Executa testes unitários com cobertura em formato Cobertura (XML)
# Prefere dotnet-coverage (igual ao CI) para detectar gaps em lambdas geradas pelo compilador.
# Fallback para Coverlet caso dotnet-coverage não esteja instalado.

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$ROOT_DIR"

echo "=== TEST ==="

mkdir -p artifacts/coverage/raw
mkdir -p artifacts/test-results

# Detect coverage collector: dotnet-coverage matches CI behavior exactly.
# Coverlet propagates [ExcludeFromCodeCoverage] to compiler-generated lambdas,
# but dotnet-coverage does NOT — so using dotnet-coverage locally catches the same
# coverage gaps that SonarCloud will report.
USE_DOTNET_COVERAGE=false
if command -v dotnet-coverage &>/dev/null; then
    USE_DOTNET_COVERAGE=true
    echo "Using dotnet-coverage (matches CI behavior)"
else
    echo "Warning: dotnet-coverage not found, falling back to Coverlet"
    echo "  Install with: dotnet tool install --global dotnet-coverage"
    echo "  Note: Coverlet may mask coverage gaps that CI/SonarCloud will detect"
fi

COVERLET_EXCLUDE="[Bedrock.BuildingBlocks.Testing]Bedrock.BuildingBlocks.Testing.TestBase,[Bedrock.BuildingBlocks.Testing]Bedrock.BuildingBlocks.Testing.ServiceCollectionFixture,[Bedrock.BuildingBlocks.Testing]Bedrock.BuildingBlocks.Testing.Attributes.*,[Bedrock.BuildingBlocks.Testing]Bedrock.BuildingBlocks.Testing.Benchmarks.*,[Bedrock.BuildingBlocks.Testing]Bedrock.BuildingBlocks.Testing.Integration.*,[Bedrock.Templates.*]*,[Bedrock.Samples.*]*"

# Find and run all test projects under tests/UnitTests
for project in $(find tests/UnitTests -name "*.csproj"); do
    # Build unique name from path: tests/UnitTests/BuildingBlocks/Core/*.csproj -> BuildingBlocks.Core
    # This avoids collisions (e.g., BuildingBlocks/Domain.Entities vs ShopDemo/Auth/Domain.Entities)
    name=$(echo "$project" | sed 's|tests/UnitTests/||' | sed 's|/[^/]*\.csproj$||' | sed 's|/|.|g')
    echo "Running tests for: $name"

    if [ "$USE_DOTNET_COVERAGE" = true ]; then
        # dotnet-coverage collect: same collector as CI pipeline (ci.yml)
        # This catches [ExcludeFromCodeCoverage] gaps in compiler-generated lambdas
        dotnet-coverage collect \
            "dotnet test --no-build $project --logger trx;LogFileName=results.trx --results-directory artifacts/coverage/raw/$name" \
            -f cobertura \
            -o "artifacts/coverage/raw/$name.cobertura.xml"
    else
        # Coverlet fallback: may not detect all coverage gaps
        dotnet test "$project" \
            --no-build \
            --collect:"XPlat Code Coverage" \
            --results-directory "artifacts/coverage/raw/$name" \
            --logger "trx;LogFileName=results.trx" \
            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura \
               DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="$COVERLET_EXCLUDE"

        # Copy coverage file to a known location
        coverage_file=$(find "artifacts/coverage/raw/$name" -name "coverage.cobertura.xml" 2>/dev/null | head -1)
        if [ -n "$coverage_file" ]; then
            cp "$coverage_file" "artifacts/coverage/raw/$name.cobertura.xml"
        fi
    fi
done

echo "Done: Tests completed"
echo "Coverage reports available in artifacts/coverage/raw/"
