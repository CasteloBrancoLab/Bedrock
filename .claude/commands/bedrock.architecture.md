---
description: Execute architecture tests (Roslyn SDK) in a loop, fixing violations until all pass (max 10 attempts).
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). The user may specify:
- A specific violation or rule to focus on
- Additional context about what was changed

If no input is provided, run against **all** architecture test projects.

## Goal

Execute architecture tests and fix violations in a loop until **all architecture rules pass** or the retry limit is reached. This command focuses **exclusively** on:

1. Running architecture tests (Roslyn SDK via `tests/ArchitectureTests/`)
2. Identifying architecture violations
3. Fixing the source code to comply with the rules

This command does **NOT** run: unit tests, mutation tests, SonarCloud checks, integration tests, PR creation, or any other pipeline step.

> **Recommended flow:** `/bedrock.implement` → `/bedrock.architecture` → `/bedrock.unittests` → `/bedrock.mutation` → `/bedrock.sonar` → `/bedrock.integration`. This is the **second step** — fix architecture before running tests.

## Execution Steps

### 1. Pre-check: Verify Build Passes

```bash
dotnet build
```

If build fails: **stop immediately**. Tell the user to fix build errors first.

### 2. Clean Artifacts

```bash
cmd //c "bash ./scripts/clean-artifacts.sh"
```

### 3. Architecture → Fix Loop

Set `ATTEMPT = 1` and `MAX_ATTEMPTS = 10`.

**For each attempt:**

#### 3a. Architecture Tests

```bash
cmd //c "bash ./scripts/architecture.sh"
```

#### 3b. Extract Pending Items

```bash
cmd //c "bash ./scripts/generate-pending-summary.sh"
```

#### 3c. Check Results

Read `artifacts/pending/SUMMARY.txt` and check the `VIOLACOES ARQUITETURA` count.

- **If `VIOLACOES ARQUITETURA: 0`**: Report SUCCESS and stop.
- **If violations found**:
  1. Read each `artifacts/pending/architecture_*.txt` file
  2. For each violation:
     - Understand the rule being violated
     - Read the source file at the indicated location
     - Read the architecture rule definition in `tests/ArchitectureTests/` to understand the expected pattern
     - Fix the source code to comply with the rule
  3. Verify fixes compile: `dotnet build`
  4. Increment `ATTEMPT`
  5. If `ATTEMPT > MAX_ATTEMPTS`: **stop and report** (see section 4)
  6. Clean artifacts: `cmd //c "bash ./scripts/clean-artifacts.sh"`
  7. Go back to step 3a

### 4. On Reaching Max Attempts

If 10 attempts are exhausted without all rules passing, **stop immediately** and report:

- Number of attempts made (10/10)
- List of still-open violations with details:
  - Rule name, file, line
  - Description of the violation
  - What was tried to fix it
- Request human intervention to decide next steps

### 5. On Success

When all architecture tests pass, report:

- Number of attempts needed
- Summary of violations fixed per attempt
- Confirm: "All architecture rules passing."

## Rules for Fixing Violations

When fixing architecture violations, follow these rules:

1. **Fix the source code**, not the architecture rules — the rules define the intended architecture
2. **Read the rule definition** in `tests/ArchitectureTests/` before attempting a fix to understand the exact constraint
3. **Do NOT suppress or disable architecture rules** unless the user explicitly authorizes it
4. **Preserve existing behavior** — architectural fixes should not change runtime semantics
5. **Run `dotnet build`** after each fix to verify compilation before proceeding

## Operating Principles

- **Focused scope**: Only architecture tests and fixing violations. Nothing else.
- **Fast feedback**: Use `dotnet build` for quick compilation checks after each fix.
- **Minimal changes**: Only modify what is needed to comply with the architecture rules. Do not refactor or "improve" unrelated code.
- **Respect retry limit**: Never exceed 10 attempts. Stop and report.
- **Transparency**: Report progress after each attempt (attempt N/10, X violations remaining).
