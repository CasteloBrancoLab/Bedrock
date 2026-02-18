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

Execute build and unit tests in a loop, identifying and fixing failing tests until **all tests pass**. This command focuses **exclusively** on:

1. Building the solution
2. Running unit tests with coverage

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

#### 2c. Check Results

- **If all tests pass**: Report SUCCESS and stop.
- **If tests fail**:
  1. Read the test output to identify failing tests
  2. For each failing test:
     - Understand the failure (test name, assertion, expected vs actual)
     - Read the test file
     - Read the source file under test
     - Fix the test or the source code as appropriate
  3. Run `dotnet build` to verify the fix compiles
  4. Increment `ATTEMPT`
  5. If `ATTEMPT > MAX_ATTEMPTS`: **stop and report** (see section 3)
  6. Clean artifacts: `./scripts/clean-artifacts.sh`
  7. Go back to step 2a

### 3. On Reaching Max Attempts

If 10 attempts are exhausted without all tests passing, **stop immediately** and report:

- Number of attempts made (10/10)
- List of still-failing tests with details:
  - Test name and project
  - Failure reason (assertion, exception, etc.)
  - What was tried to fix it
- Request human intervention to decide next steps

### 4. On Success

When all tests pass, report:

- Number of attempts needed
- Summary of fixes applied per attempt
- Confirm: "All unit tests passing."

## Rules for Writing/Fixing Tests

When writing or fixing tests, follow these rules:

1. **Use AAA pattern** (Arrange, Act, Assert) — mandatory
2. **Use project conventions**: xUnit, Shouldly, Moq, Bogus
3. **Inherit from TestBase** and use `LogArrange`, `LogAct`, `LogAssert`
4. **Prefer specific assertions** that directly exercise the code path under test
5. **Run `dotnet build`** after writing/fixing tests to verify compilation before running the full test script

## Operating Principles

- **Focused scope**: Only build and unit tests. Nothing else.
- **Fast feedback**: Use `dotnet build` and `dotnet test <project>` for quick validation of fixes before running the full scripts.
- **Minimal changes**: Only add or modify what is needed to make tests pass. Do not refactor or "improve" unrelated code.
- **Respect retry limit**: Never exceed 10 attempts. Stop and report.
- **Transparency**: Report progress after each attempt (attempt N/10, X tests failing).
