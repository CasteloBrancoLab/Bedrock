---
description: Execute mutation tests (Stryker.NET) in a loop, fixing surviving mutants until 100% mutation score is achieved (max 10 attempts).
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). The user may specify:
- A specific project to test (e.g., `BuildingBlocks.Core`)
- Additional context about what was changed

If no input is provided, run against **all** projects.

## Goal

Execute mutation tests and fix surviving mutants in a loop until **100% mutation score** is achieved or the retry limit is reached. This command focuses **exclusively** on:

1. Running mutation tests (Stryker.NET)
2. Identifying surviving mutants
3. Writing or improving unit tests to kill them

This command does **NOT** run: build script, unit test script, architecture tests, SonarCloud checks, integration tests, PR creation, or any other pipeline step. It assumes **architecture and unit tests are already passing** (run `/bedrock.architecture` then `/bedrock.unittests` first).

> **Recommended flow:** `/bedrock.implement` → `/bedrock.architecture` → `/bedrock.unittests` → `/bedrock.mutation` → `/bedrock.sonar` → `/bedrock.integration`. This is the **fourth step**.

## Execution Steps

### 1. Pre-check: Verify Build and Tests Pass

Run a quick build and test to confirm the baseline is green:

```bash
dotnet build
```

If build fails: **stop immediately**. Tell the user to run `/bedrock.architecture` then `/bedrock.unittests` first.

```bash
dotnet test
```

If tests fail: **stop immediately**. Tell the user to run `/bedrock.unittests` first.

### 2. Clean Artifacts

```bash
cmd //c "bash ./scripts/clean-artifacts.sh"
```

### 3. Mutate → Fix Loop

Set `ATTEMPT = 1` and `MAX_ATTEMPTS = 10`.

**For each attempt:**

#### 3a. Mutation Tests

```bash
cmd //c "bash ./scripts/mutate.sh"
```

#### 3b. Extract Pending Items

```bash
cmd //c "bash ./scripts/summarize.sh"
cmd //c "bash ./scripts/generate-pending-summary.sh"
```

#### 3c. Check Results

Read `artifacts/pending/SUMMARY.txt` and check the `MUTANTES PENDENTES` count.

- **If `MUTANTES PENDENTES: 0`**: Report SUCCESS and stop.
- **If surviving mutants exist**:
  1. Read each `artifacts/pending/mutant_*.txt` file
  2. For each surviving mutant:
     - Understand the mutation (file, line, mutator, description)
     - Read the source file at the indicated line
     - Read the corresponding test file
     - Write or improve tests to kill the mutant
  3. Verify fixes compile: `dotnet build`
  4. Quick-check the new tests pass: `dotnet test <affected-test-project>`
  5. Increment `ATTEMPT`
  6. If `ATTEMPT > MAX_ATTEMPTS`: **stop and report** (see section 4)
  7. Clean artifacts: `cmd //c "bash ./scripts/clean-artifacts.sh"`
  8. Go back to step 3a

### 4. On Reaching Max Attempts

If 10 attempts are exhausted without reaching 100%, **stop immediately** and report:

- Number of attempts made (10/10)
- List of still-surviving mutants with details:
  - Project, file, line
  - Mutator type and description
  - What was tried to kill it
- Request human intervention to decide next steps

### 5. On Success

When 100% mutation score is achieved, report:

- Number of attempts needed
- Summary of mutants killed per attempt
- Confirm: "All mutants killed. 100% mutation score achieved."

## Rules for Fixing Mutants

When writing tests to kill surviving mutants, follow these rules:

1. **Use AAA pattern** (Arrange, Act, Assert) — mandatory
2. **Use project conventions**: xUnit, Shouldly, Moq, Bogus
3. **Inherit from TestBase** and use `LogArrange`, `LogAct`, `LogAssert`
4. **Prefer specific assertions** that directly exercise the mutated code path
5. **Do NOT use `[ExcludeFromCodeCoverage]` or Stryker disable comments** unless the code is genuinely impossible to test (e.g., spin-wait, overflow requiring millions of iterations). If you believe an exclusion is warranted, **stop and ask the user first**.
6. **Do NOT modify source code** to make mutants easier to kill — fix the tests, not the production code
7. **Run `dotnet build`** after writing tests to verify compilation before proceeding to the next mutation cycle

## Operating Principles

- **Focused scope**: Only mutation tests and fixing surviving mutants. Nothing else.
- **Fast feedback**: Use `dotnet build` and `dotnet test <project>` for quick validation of fixes before running the full `mutate.sh`.
- **Minimal changes**: Only add or modify tests needed to kill surviving mutants. Do not refactor or "improve" unrelated tests.
- **Respect retry limit**: Never exceed 10 attempts. Stop and report.
- **Transparency**: Report progress after each attempt (attempt N/10, X mutants remaining).
