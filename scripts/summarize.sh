#!/bin/bash
# summarize.sh - Extrai pendências locais (mutação, cobertura, arquitetura) para artifacts/pending
# SonarCloud: ver sonar-check.sh
# Sumário consolidado: ver generate-pending-summary.sh
set -e

ARTIFACTS_DIR="artifacts"
PENDING_DIR="$ARTIFACTS_DIR/pending"

echo "=== SUMMARIZE ==="

# Cria diretório de pendências (limpa apenas arquivos locais, preserva sonar_*.txt de execuções anteriores)
mkdir -p "$PENDING_DIR"
rm -f "$PENDING_DIR"/mutant_*.txt "$PENDING_DIR"/coverage_*.txt "$PENDING_DIR"/SUMMARY.txt 2>/dev/null || true

# ========================================
# VIOLAÇÕES DE ARQUITETURA
# ========================================
echo "Counting architecture violations..."

ARCH_PENDING=$(find "$PENDING_DIR" -name "architecture_*.txt" 2>/dev/null | wc -l)
ARCH_PENDING=${ARCH_PENDING//[^0-9]/}
echo "Found $ARCH_PENDING architecture violations"

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
MUTATION_PENDING=${MUTATION_PENDING//[^0-9]/}
echo "Found $MUTATION_PENDING surviving mutants"

# ========================================
# COBERTURA (linhas não cobertas)
# ========================================
echo "Extracting uncovered lines..."

# Coverage exclusion patterns (aligned with sonar.coverage.exclusions in ci.yml)
COVERAGE_EXCLUSIONS=(
    "**/tests/**/*"
    "**/samples/**/*"
    "**/templates/**/*"
    "**/tools/**/*"
    "**/playground/**/*"
    "**/src/BuildingBlocks/Testing/Benchmarks/**/*"
    "**/src/BuildingBlocks/Testing/Integration/**/*"
    "**/src/BuildingBlocks/Testing/Attributes/**/*"
    "**/src/BuildingBlocks/Testing/TestBase.cs"
    "**/src/BuildingBlocks/Testing/ServiceCollectionFixture.cs"
)

COVERAGE_FILE_INDEX=0

for cobertura in "$ARTIFACTS_DIR"/coverage/raw/*.cobertura.xml; do
    if [ ! -f "$cobertura" ]; then
        continue
    fi

    project_name=$(basename "$cobertura" .cobertura.xml)

    # Extract uncovered files using PowerShell (filters transitive deps by package name suffix)
    powershell -NoProfile -Command "
        [xml]\$xml = Get-Content '$cobertura'
        \$suffix = '.$project_name'
        \$exclusions = @($(printf "'%s'," "${COVERAGE_EXCLUSIONS[@]}" | sed 's/,$//'))

        function Test-Excluded(\$path) {
            foreach (\$pattern in \$exclusions) {
                \$regex = '^' + [regex]::Escape(\$pattern).Replace('\*\*', '§§').Replace('\*', '[^/]*').Replace('§§', '.*') + '$'
                if (\$path -match \$regex) { return \$true }
            }
            return \$false
        }

        foreach (\$pkg in \$xml.coverage.packages.package) {
            \$pkgName = \$pkg.name
            if (-not (\$pkgName.EndsWith(\$suffix) -or \$pkgName -eq \$project_name)) { continue }

            foreach (\$cls in \$pkg.classes.class) {
                \$filename = \$cls.filename -replace '\\\\', '/'

                # Build relative path from src/
                \$srcIdx = \$filename.IndexOf('/src/')
                if (\$srcIdx -lt 0) { continue }
                \$relPath = \$filename.Substring(\$srcIdx + 1)

                # Check exclusions
                if (Test-Excluded \$relPath) { continue }

                # Collect uncovered lines (hits=0)
                \$uncovered = @()
                \$total = 0
                \$covered = 0
                foreach (\$line in \$cls.lines.line) {
                    \$total++
                    if ([int]\$line.hits -eq 0) {
                        \$uncovered += \$line.number
                    } else {
                        \$covered++
                    }
                }

                if (\$uncovered.Count -eq 0) { continue }

                \$lineRate = if (\$total -gt 0) { [math]::Round(\$covered / \$total * 100, 1) } else { 100 }

                # Output one line per uncovered file: PROJECT|FILE|LINE_RATE|TOTAL|COVERED|UNCOVERED_LINES
                Write-Output \"\$pkgName|\$relPath|\$lineRate|\$total|\$covered|\$(\$uncovered -join ',')\";
            }
        }
    " 2>/dev/null | while IFS='|' read -r pkg filePath lineRate total coveredCount uncoveredLines; do
        if [ -z "$filePath" ]; then continue; fi
        COVERAGE_FILE_INDEX=$((COVERAGE_FILE_INDEX + 1))
        idx=$(printf "%03d" "$COVERAGE_FILE_INDEX")

        cat > "$PENDING_DIR/coverage_${project_name}_${idx}.txt" << COVEOF
PROJECT: $pkg
FILE: $filePath
LINE_RATE: ${lineRate}%
TOTAL_LINES: $total
COVERED_LINES: $coveredCount
UNCOVERED_LINES: $uncoveredLines
COVEOF
    done
done

COVERAGE_PENDING=$(find "$PENDING_DIR" -name "coverage_*.txt" 2>/dev/null | wc -l)
COVERAGE_PENDING=${COVERAGE_PENDING//[^0-9]/}
echo "Found $COVERAGE_PENDING files with uncovered lines"

echo "Done: Local pending items extracted to $PENDING_DIR/"
