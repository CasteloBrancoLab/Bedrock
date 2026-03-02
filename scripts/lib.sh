#!/bin/bash
# lib.sh - Shared functions for Bedrock scripts
# Source this file from other scripts: source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

# ========================================
# INITIALIZATION
# ========================================

lib_init() {
    set -e
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[1]}")" && pwd)"
    ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
    cd "$ROOT_DIR"
    mkdir -p artifacts/pending
}

# ========================================
# UTILITIES
# ========================================

count_pending() {
    local pattern="${1:-*.txt}"
    local count
    count=$(find artifacts/pending -name "$pattern" ! -name "SUMMARY.txt" 2>/dev/null | wc -l)
    echo "${count//[^0-9]/}"
}

extract_field() {
    local file="$1" field="$2"
    grep "^${field}:" "$file" | cut -d: -f2- | xargs
}

timer_start() {
    date +%s%3N
}

timer_elapsed() {
    local start="$1"
    local now
    now=$(date +%s%3N)
    echo $((now - start))
}

clean_pending() {
    local pattern="$1"
    rm -f artifacts/pending/${pattern}_*.txt
}

# Derive short name from project path
# tests/UnitTests/BuildingBlocks/Core/*.csproj -> BuildingBlocks.Core
# tests/IntegrationTests/BuildingBlocks/Persistence/*.csproj -> BuildingBlocks.Persistence
derive_short_name() {
    local project="$1" base_dir="$2"
    echo "$project" | sed "s|${base_dir}/||" | sed 's|/[^/]*\.csproj$||' | sed 's|/|.|g'
}

# ========================================
# COVERAGE EXCLUSIONS (single source of truth)
# ========================================

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

# Convert COVERAGE_EXCLUSIONS glob patterns to pipe-delimited regex for awk
build_exclusion_regex() {
    local regex=""
    for pat in "${COVERAGE_EXCLUSIONS[@]}"; do
        local r
        r=$(printf '%s' "$pat" | sed 's/[.]/[.]/g; s/\*\*/__DBLSTAR__/g; s/\*/[^\/]*/g; s/__DBLSTAR__/.*/g')
        r="^${r}$"
        if [ -n "$regex" ]; then
            regex="${regex}|${r}"
        else
            regex="$r"
        fi
    done
    echo "$regex"
}

# ========================================
# PARSERS
# ========================================

# Parse dotnet test console output for failed tests
# Usage: parse_test_failures "$LOG_FILE" "$short_name" "test|integration"
parse_test_failures() {
    local log_file="$1" project="$2" prefix="${3:-test}"

    awk -v project="$project" -v dir="artifacts/pending" -v prefix="$prefix" '
    BEGIN { idx = 0; test_name = ""; msg = ""; st = ""; in_msg = 0; in_st = 0 }

    /^[[:space:]]+Failed / {
        if (test_name != "") {
            idx++
            num = sprintf("%03d", idx)
            outfile = dir "/" prefix "_" project "_" num ".txt"
            print "PROJECT: " project > outfile
            print "TEST: " test_name >> outfile
            print "MESSAGE: " msg >> outfile
            print "STACKTRACE: " st >> outfile
            close(outfile)
        }
        s = $0
        sub(/^[[:space:]]+Failed /, "", s)
        sub(/ \[.*\]$/, "", s)
        test_name = s
        msg = ""; st = ""; in_msg = 0; in_st = 0
    }

    /^[[:space:]]+Error Message:/ { in_msg = 1; in_st = 0; next }
    /^[[:space:]]+Stack Trace:/ { in_st = 1; in_msg = 0; next }
    /^$/ || /^[[:space:]]+Failed!/ || /^[[:space:]]+Passed!/ { in_msg = 0; in_st = 0 }

    in_msg {
        s = $0; sub(/^[[:space:]]+/, "", s)
        msg = msg (msg != "" ? " " : "") s
    }

    in_st && st == "" {
        s = $0; sub(/^[[:space:]]+/, "", s)
        st = s
    }

    END {
        if (test_name != "") {
            idx++
            num = sprintf("%03d", idx)
            outfile = dir "/" prefix "_" project "_" num ".txt"
            print "PROJECT: " project > outfile
            print "TEST: " test_name >> outfile
            print "MESSAGE: " msg >> outfile
            print "STACKTRACE: " st >> outfile
            close(outfile)
        }
    }
    ' "$log_file"
}

# Parse Cobertura XML for coverage gaps
# Usage: parse_coverage_gaps "$cobertura_file" "$project_name" "$exclusion_regex"
parse_coverage_gaps() {
    local cobertura="$1" project_name="$2" excl_regex="$3"

    awk -F'"' -v project_name="$project_name" -v excl_regex="$excl_regex" '
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
        if (total > 0 && uncov_count > 0 && !skip_class) {
            line_rate = (total > 0) ? sprintf("%.1f", covered / total * 100) : "100.0"
            print pkg "|" rel_path "|" line_rate "|" total "|" covered "|" uncov
        }

        cls_file = ""
        for (i = 1; i <= NF; i++) {
            if ($(i) ~ /filename=$/) { cls_file = $(i+1); break }
        }

        gsub(/\\/, "/", cls_file)

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
    ' "$cobertura"
}

# Parse Stryker JSON report for surviving mutants
# Usage: parse_surviving_mutants "$report_file" "$project_name"
parse_surviving_mutants() {
    local report="$1" project="$2"

    grep -v '^[[:space:]]*"source"[[:space:]]*:' "$report" | awk -v project="$project" -v dir="artifacts/pending" '
    BEGIN {
        idx = 0; file = ""; m_file = ""
        m_mutator = ""; m_line = ""; m_status = ""; m_desc = ""
        in_start = 0; had_mutant = 0
    }

    /^[[:space:]]*"[^"]+[.][a-zA-Z]+"[[:space:]]*:[[:space:]]*\{/ {
        s = $0; sub(/^[[:space:]]*"/, "", s); sub(/".*/, "", s); file = s
    }

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
        m_file = file; m_mutator = ""; m_line = ""; m_status = ""; m_desc = ""
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
    }
    ' || true
}

# Check if a source project has .cs files to mutate
# Usage: has_source_files "$stryker_config_path"
# Returns 0 (true) if source files exist, 1 (false) if not
has_source_files() {
    local config="$1"
    local src_project src_csproj src_dir cs_count

    src_project=$(grep -o '"project":.*"' "$config" | head -1 | sed 's/"project":[[:space:]]*"//;s/"$//')
    if [ -n "$src_project" ]; then
        src_csproj=$(find "$ROOT_DIR" -name "$src_project" -not -path "*/bin/*" -not -path "*/obj/*" | head -1)
        if [ -n "$src_csproj" ]; then
            src_dir=$(dirname "$src_csproj")
            cs_count=$(find "$src_dir" -name "*.cs" ! -name "GlobalUsings.cs" -not -path "*/obj/*" -not -path "*/bin/*" 2>/dev/null | wc -l)
            cs_count=${cs_count//[^0-9]/}
            if [ "$cs_count" -eq 0 ]; then
                return 1
            fi
        fi
    fi
    return 0
}
