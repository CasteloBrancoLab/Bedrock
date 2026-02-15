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

    # Parse mutation report JSON using awk (pure bash, cross-platform)
    # Strip "source" lines first — they contain the full file source code with braces/quotes
    # that would confuse the parser
    grep -v '^[[:space:]]*"source"[[:space:]]*:' "$report" | awk -v project="$project" -v dir="$PENDING_DIR" '
    BEGIN {
        idx = 0; file = ""; m_file = ""
        m_mutator = ""; m_line = ""; m_status = ""; m_desc = ""
        in_start = 0; had_mutant = 0
    }

    # File path key under "files" object: "path/to/file.ext": {
    /^[[:space:]]*"[^"]+[.][a-zA-Z]+"[[:space:]]*:[[:space:]]*\{/ {
        s = $0
        sub(/^[[:space:]]*"/, "", s)
        sub(/".*/, "", s)
        file = s
    }

    # New mutant detected by "id" field — output previous if surviving
    /[[:space:]]*"id"[[:space:]]*:[[:space:]]*"/ {
        if (had_mutant && (m_status == "Survived" || m_status == "NoCoverage")) {
            idx++
            num = sprintf("%03d", idx)
            outfile = dir "/mutant_" project "_" num ".txt"
            print "PROJECT: " project > outfile
            print "FILE: " m_file >> outfile
            print "LINE: " m_line >> outfile
            print "STATUS: " m_status >> outfile
            print "MUTATOR: " m_mutator >> outfile
            print "DESCRIPTION: " m_desc >> outfile
            close(outfile)
        }
        m_file = file
        m_mutator = ""; m_line = ""; m_status = ""; m_desc = ""
        in_start = 0; had_mutant = 1
    }

    /"mutatorName"[[:space:]]*:/ {
        s = $0; sub(/.*"mutatorName"[[:space:]]*:[[:space:]]*"/, "", s); sub(/".*/, "", s)
        m_mutator = s
    }

    /"start"[[:space:]]*:[[:space:]]*\{/ { in_start = 1 }
    /"end"[[:space:]]*:[[:space:]]*\{/ { in_start = 0 }

    /"line"[[:space:]]*:/ && in_start {
        s = $0; sub(/.*"line"[[:space:]]*:[[:space:]]*/, "", s); sub(/[^0-9].*/, "", s)
        if (s ~ /^[0-9]+$/) m_line = s
    }

    /"status"[[:space:]]*:/ {
        s = $0; sub(/.*"status"[[:space:]]*:[[:space:]]*"/, "", s); sub(/".*/, "", s)
        m_status = s
    }

    /"description"[[:space:]]*:/ {
        s = $0; sub(/.*"description"[[:space:]]*:[[:space:]]*"/, "", s); sub(/".*/, "", s)
        m_desc = s
    }

    END {
        if (had_mutant && (m_status == "Survived" || m_status == "NoCoverage")) {
            idx++
            num = sprintf("%03d", idx)
            outfile = dir "/mutant_" project "_" num ".txt"
            print "PROJECT: " project > outfile
            print "FILE: " m_file >> outfile
            print "LINE: " m_line >> outfile
            print "STATUS: " m_status >> outfile
            print "MUTATOR: " m_mutator >> outfile
            print "DESCRIPTION: " m_desc >> outfile
            close(outfile)
        }
        print idx
    }
    ' || echo "0"
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

# Convert glob patterns to pipe-delimited regex for awk
EXCL_REGEX=""
for pat in "${COVERAGE_EXCLUSIONS[@]}"; do
    r=$(printf '%s' "$pat" | sed 's/[.]/[.]/g; s/\*\*/__DBLSTAR__/g; s/\*/[^\/]*/g; s/__DBLSTAR__/.*/g')
    r="^${r}$"
    if [ -n "$EXCL_REGEX" ]; then
        EXCL_REGEX="${EXCL_REGEX}|${r}"
    else
        EXCL_REGEX="$r"
    fi
done

COVERAGE_FILE_INDEX=0

for cobertura in "$ARTIFACTS_DIR"/coverage/raw/*.cobertura.xml; do
    if [ ! -f "$cobertura" ]; then
        continue
    fi

    project_name=$(basename "$cobertura" .cobertura.xml)

    # Parse cobertura XML using awk (pure bash, cross-platform)
    awk -F'"' -v project_name="$project_name" -v excl_regex="$EXCL_REGEX" '
    BEGIN {
        suffix = "." project_name
        pkg = ""; cls_file = ""; rel_path = ""
        total = 0; covered = 0; uncov = ""; uncov_count = 0
        in_matching_pkg = 0; skip_class = 1
    }

    /<package[[:space:]]/ {
        for (i = 1; i <= NF; i++) {
            if ($(i) ~ /name=$/) { pkg = $(i+1); break }
        }
        if (pkg == project_name || substr(pkg, length(pkg) - length(suffix) + 1) == suffix) {
            in_matching_pkg = 1
        } else {
            in_matching_pkg = 0
        }
    }

    /<\/package>/ { in_matching_pkg = 0 }

    in_matching_pkg && /<class[[:space:]]/ {
        # Output previous class if it had uncovered lines
        if (total > 0 && uncov_count > 0 && !skip_class) {
            line_rate = (total > 0) ? sprintf("%.1f", covered / total * 100) : "100.0"
            print pkg "|" rel_path "|" line_rate "|" total "|" covered "|" uncov
        }

        # Parse new class
        cls_file = ""
        for (i = 1; i <= NF; i++) {
            if ($(i) ~ /filename=$/) { cls_file = $(i+1); break }
        }

        # Normalize backslashes to forward slashes
        gsub(/\\/, "/", cls_file)

        # Extract relative path from /src/
        src_idx = index(cls_file, "/src/")
        if (src_idx == 0) {
            skip_class = 1
        } else {
            rel_path_full = substr(cls_file, src_idx)
            rel_path = substr(cls_file, src_idx + 1)
            if (excl_regex != "" && rel_path_full ~ excl_regex) {
                skip_class = 1
            } else {
                skip_class = 0
            }
        }

        total = 0; covered = 0; uncov = ""; uncov_count = 0
    }

    in_matching_pkg && !skip_class && /<line[[:space:]]/ {
        linenum = ""; hits = ""
        for (i = 1; i <= NF; i++) {
            if ($(i) ~ /number=$/) linenum = $(i+1)
            if ($(i) ~ /hits=$/) hits = $(i+1)
        }
        if (linenum != "" && hits != "") {
            total++
            if (hits + 0 == 0) {
                uncov = uncov (uncov ? "," : "") linenum
                uncov_count++
            } else {
                covered++
            }
        }
    }

    END {
        if (total > 0 && uncov_count > 0 && !skip_class) {
            line_rate = (total > 0) ? sprintf("%.1f", covered / total * 100) : "100.0"
            print pkg "|" rel_path "|" line_rate "|" total "|" covered "|" uncov
        }
    }
    ' "$cobertura" | while IFS='|' read -r pkg filePath lineRate total coveredCount uncoveredLines; do
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
