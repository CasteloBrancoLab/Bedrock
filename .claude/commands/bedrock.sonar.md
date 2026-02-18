---
description: Fetch and fix SonarCloud issues and coverage gaps in a loop (max 10 attempts). Runs after mutation tests pass.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). The user may specify:
- `pr` or a PR number — check issues on the current PR branch
- `main` — check issues on the main branch
- A specific issue type to focus on (e.g., `bugs`, `smells`, `vulnerabilities`)

If no input is provided, check issues for the **current branch**.

## Goal

Fetch open SonarCloud issues and coverage gaps, then fix them in a loop until **zero actionable issues remain** or the retry limit is reached. This command focuses **exclusively** on:

1. Fetching SonarCloud issues (bugs, vulnerabilities, code smells, security hotspots)
2. Analyzing SonarCloud coverage data (which differs from local Coverlet due to exclusions)
3. Fixing issues and improving coverage in source code and tests

This command does **NOT** run: architecture tests, unit tests, mutation tests, integration tests, PR creation, or any other pipeline step. It assumes **architecture, unit tests, and mutation tests are already passing**.

> **Recommended flow:** `/bedrock.implement` → `/bedrock.architecture` → `/bedrock.unittests` → `/bedrock.mutation` → `/bedrock.sonar` → `/bedrock.integration`. This is the **fifth step** — SonarCloud analysis after local quality gates pass.

## Why SonarCloud After Local Tests

SonarCloud coverage and issues differ from local results because:
- SonarCloud applies its own **exclusion rules** (e.g., generated code, test helpers)
- Coverage calculation includes **all branches and conditions**, not just line coverage
- SonarCloud detects issues that local tools miss (security, maintainability, reliability)
- `[ExcludeFromCodeCoverage]` and Stryker disable comments affect local metrics but SonarCloud has its own analysis

## Execution Steps

### 1. Pre-check: Verify SONAR_TOKEN

Check that `.env` contains `SONAR_TOKEN`. If not:
- **Stop immediately**
- Tell the user to configure `SONAR_TOKEN` in `.env` (see CLAUDE.md for instructions)

### 2. Clean Sonar Artifacts

```bash
rm -f artifacts/pending/sonar_*.txt
```

### 3. Fetch → Fix Loop

Set `ATTEMPT = 1` and `MAX_ATTEMPTS = 10`.

**For each attempt:**

#### 3a. Fetch SonarCloud Issues

```bash
./scripts/sonar-check.sh
```

#### 3b. Check Results

Read `artifacts/pending/SUMMARY.txt` (if exists) or count `artifacts/pending/sonar_*.txt` files.

- **If no sonar issues**: Report SUCCESS and stop.
- **If issues exist**:
  1. Read each `artifacts/pending/sonar_*.txt` file
  2. **Triage each issue** (see Triage Rules below)
  3. For each actionable issue:
     - Read the source file at the indicated line
     - Understand the SonarCloud rule being violated
     - Fix the source code to resolve the issue
  4. For each non-actionable issue:
     - Log it as "Won't Fix" with justification
  5. Verify fixes compile: `dotnet build`
  6. Verify unit tests still pass: `dotnet test`
  7. If fixes were made, commit and push so SonarCloud re-analyzes:
     - **Ask the user before pushing** — do not push automatically
  8. If user approved push, wait for SonarCloud to re-analyze (typically 1-2 minutes after CI runs)
  9. Increment `ATTEMPT`
  10. If `ATTEMPT > MAX_ATTEMPTS`: **stop and report** (see section 4)
  11. Clean sonar artifacts: `rm -f artifacts/pending/sonar_*.txt`
  12. Go back to step 3a

### 4. On Reaching Max Attempts

If 10 attempts are exhausted without resolving all issues, **stop immediately** and report:

- Number of attempts made (10/10)
- List of remaining issues with details:
  - Type, severity, file, line, rule
  - Whether it was triaged as actionable or "Won't Fix"
  - What was tried to fix it (if actionable)
- Request human intervention to decide next steps

### 5. On Success

When all actionable SonarCloud issues are resolved, report:

- Number of attempts needed
- Summary of issues fixed per attempt
- List of issues marked as "Won't Fix" with justifications
- Confirm: "All actionable SonarCloud issues resolved."

## Triage Rules

Not all SonarCloud issues should be fixed. **Evaluate each issue critically:**

### Fix (Actionable)
- Bugs with clear impact on correctness
- Vulnerabilities and security hotspots with real risk
- Code smells that degrade maintainability and are easy to fix
- Coverage gaps in production code that should have tests

### Won't Fix (Non-Actionable)
- Issues that **contradict documented ADRs** (Architecture Decision Records)
- **False positives**: generated code, test helpers, mocks, intentional patterns
- Issues in code **excluded by design** (e.g., `[ExcludeFromCodeCoverage]` with valid justification)
- Coverage gaps in code that is **genuinely untestable** (already documented with Stryker disable + `[ExcludeFromCodeCoverage]`)
- Code smells with **trivial severity** that would introduce unnecessary churn

### When Unsure
- **Stop and ask the user** before deciding
- Present the issue details and your recommendation
- Let the user decide: fix, won't fix, or defer

## Rules for Fixing Issues

1. **Fix the root cause**, not the symptom — understand the SonarCloud rule before applying a fix
2. **Do NOT suppress warnings** with `#pragma` or `[SuppressMessage]` unless the user explicitly authorizes it
3. **Preserve existing behavior** — fixes should not change runtime semantics
4. **Run `dotnet build` and `dotnet test`** after each fix to verify nothing broke
5. **Do NOT modify test infrastructure** (TestBase, fixtures) to improve coverage numbers

## Operating Principles

- **Focused scope**: Only SonarCloud issues and coverage. Nothing else.
- **Critical evaluation**: Not every SonarCloud issue is worth fixing. Triage before acting.
- **Transparency**: Report every issue — fixed, deferred, or won't-fix — with justification.
- **No silent pushes**: Always ask the user before pushing changes for SonarCloud re-analysis.
- **Respect retry limit**: Never exceed 10 attempts. Stop and report.
- **Transparency**: Report progress after each attempt (attempt N/10, X issues remaining).
