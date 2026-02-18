---
description: Execute build and unit tests in a loop until all tests pass with full coverage (max 10 attempts).
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). The user may specify:
- A specific project to test (e.g., `BuildingBlocks.Core`)
- A specific test project path
- Additional context about what was changed

If no input is provided, run against **all** projects.

## Goal

Execute build and unit tests in a loop, identifying and fixing failing tests until **all tests pass with 100% code coverage**. This command focuses **exclusively** on:

1. Building the solution
2. Running unit tests with coverage
3. Verifying 100% line coverage and fixing gaps

This command does **NOT** run: mutation tests, architecture tests, SonarCloud checks, integration tests, PR creation, or any other pipeline step. It assumes **architecture tests are already passing** (run `/bedrock.architecture` first).

> **Recommended flow:** `/bedrock.implement` → `/bedrock.architecture` → `/bedrock.unittests` → `/bedrock.mutation` → `/bedrock.sonar` → `/bedrock.integration`. This is the **third step** — architecture must be correct before testing.

## Execution Steps

### 1. Clean Artifacts

```bash
./scripts/clean-artifacts.sh
```

### 2. Build → Test Loop

Set `ATTEMPT = 1` and `MAX_ATTEMPTS = 10`.

**For each attempt:**

#### 2a. Build

```bash
./scripts/build.sh
```

If build fails: read the build output, identify the errors, fix them, and restart from step 2a (same attempt, do not increment counter).

#### 2b. Unit Tests

```bash
./scripts/test.sh
```

#### 2c. Check Test Results

- **If tests fail**:
  1. Read the test output to identify failing tests
  2. For each failing test:
     - Understand the failure (test name, assertion, expected vs actual)
     - Read the test file
     - Read the source file under test
     - Fix the test or the source code as appropriate
  3. Run `dotnet build` to verify the fix compiles
  4. Increment `ATTEMPT`
  5. If `ATTEMPT > MAX_ATTEMPTS`: **stop and report** (see section 4)
  6. Clean artifacts: `./scripts/clean-artifacts.sh`
  7. Go back to step 2a
- **If all tests pass**: Continue to step 2d (coverage check).

#### 2d. Extract Coverage

Run the summarize script to extract coverage pending items from the Cobertura XML reports:

```bash
./scripts/summarize.sh
./scripts/generate-pending-summary.sh
```

#### 2e. Check Coverage Results

Read `artifacts/pending/SUMMARY.txt` and check the `COBERTURA PENDENTE` count.

- **If `COBERTURA PENDENTE: 0`**: All files have 100% coverage. Report SUCCESS and stop.
- **If `COBERTURA PENDENTE` > 0**: Coverage gaps exist.
  1. Read each `artifacts/pending/coverage_*.txt` file to identify gaps. Each file contains:
     ```
     PROJECT: <package-name>
     FILE: <relative-path>
     LINE_RATE: <percentage>%
     TOTAL_LINES: <count>
     COVERED_LINES: <count>
     UNCOVERED_LINES: <comma-separated-line-numbers>
     ```
  2. For each file with uncovered lines:
     - Read the **source file** and identify the uncovered lines (from `UNCOVERED_LINES`)
     - Understand **what code paths** are not being exercised
     - Read the **corresponding test file**
     - Write new tests or extend existing tests to cover the missing lines
     - If the uncovered code is **genuinely untestable** (e.g., spin-wait, overflow), apply `[ExcludeFromCodeCoverage]` with justification and Stryker comments as documented in CLAUDE.md
  3. Run `dotnet build` to verify the fix compiles
  4. Increment `ATTEMPT`
  5. If `ATTEMPT > MAX_ATTEMPTS`: **stop and report** (see section 4)
  6. Clean artifacts: `./scripts/clean-artifacts.sh`
  7. Go back to step 2a (full re-run: build, tests, coverage)

### 3. Coverage Fix Guidelines

When fixing coverage gaps:

1. **Read the uncovered lines first** — understand the code before writing tests
2. **Prefer testing through public API** — don't make methods public just for testing
3. **Cover edge cases**: null inputs, boundary values, error paths, guard clauses
4. **One test per uncovered path** — keep tests focused and named descriptively
5. **Do NOT blindly exclude** — `[ExcludeFromCodeCoverage]` is a last resort for genuinely untestable code only
6. **Verify locally** — after writing tests, run `dotnet test <project>` to confirm they pass before running the full cycle

### 4. On Reaching Max Attempts

If 10 attempts are exhausted without all tests passing **and** 100% coverage, **stop immediately** and report:

- Number of attempts made (10/10)
- **Failing tests** (if any):
  - Test name and project
  - Failure reason (assertion, exception, etc.)
  - What was tried to fix it
- **Coverage gaps** (if any):
  - Files still below 100% with line rates
  - Uncovered line numbers
  - What was tried to cover them
- Request human intervention to decide next steps

### 5. On Success

When all tests pass **and** coverage is 100%, report:

- Number of attempts needed
- Summary of fixes applied per attempt (test fixes and coverage additions)
- Confirm: "All unit tests passing with 100% coverage."

## Rules for Writing/Fixing Tests

When writing or fixing tests, follow these rules:

1. **Use AAA pattern** (Arrange, Act, Assert) — mandatory
2. **Use project conventions**: xUnit, Shouldly, Moq, Bogus
3. **Inherit from TestBase** and use `LogArrange`, `LogAct`, `LogAssert`
4. **Prefer specific assertions** that directly exercise the code path under test
5. **Run `dotnet build`** after writing/fixing tests to verify compilation before running the full test script

## Operating Principles

- **Focused scope**: Only build, unit tests, and coverage. Nothing else.
- **Fast feedback**: Use `dotnet build` and `dotnet test <project>` for quick validation of fixes before running the full scripts.
- **Minimal changes**: Only add or modify what is needed to make tests pass and reach 100% coverage. Do not refactor or "improve" unrelated code.
- **Respect retry limit**: Never exceed 10 attempts. Stop and report.
- **Transparency**: Report progress after each attempt (attempt N/10, X tests failing, Y coverage gaps).
- **Coverage is mandatory**: Tests passing alone is NOT sufficient. The loop only ends when tests pass **AND** coverage is 100%.
