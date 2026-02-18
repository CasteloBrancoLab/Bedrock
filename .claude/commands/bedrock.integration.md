---
description: Execute integration tests in a loop, fixing failures until all pass (max 10 attempts). Requires Docker for Testcontainers.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). The user may specify:
- A specific integration test project (e.g., `Persistence.PostgreSql`)
- Additional context about what was changed

If no input is provided, run against **all** integration test projects in `tests/IntegrationTests/`.

## Goal

Execute integration tests and fix failures in a loop until **all integration tests pass** or the retry limit is reached. This command focuses **exclusively** on:

1. Running integration tests (Testcontainers, PostgreSQL, etc.)
2. Identifying test failures
3. Fixing source code or test code to make them pass

This command does **NOT** run: architecture tests, unit tests, mutation tests, SonarCloud checks, PR creation, or any other pipeline step. It assumes **architecture, unit tests, mutation tests, and SonarCloud are already passing**.

> **Recommended flow:** `/bedrock.implement` → `/bedrock.architecture` → `/bedrock.unittests` → `/bedrock.mutation` → `/bedrock.sonar` → `/bedrock.integration`. This is the **sixth and final step**.

## Prerequisites

- **Docker must be running** — integration tests use Testcontainers (PostgreSQL containers)
- Build must be passing

## Execution Steps

### 1. Pre-check: Verify Docker and Build

```bash
docker info > /dev/null 2>&1
```

If Docker is not running: **stop immediately**. Tell the user to start Docker.

```bash
dotnet build
```

If build fails: **stop immediately**. Tell the user to fix build errors first.

### 2. Discover Integration Test Projects

Find all `.csproj` files under `tests/IntegrationTests/`:

```bash
find tests/IntegrationTests -name "*.csproj"
```

If no projects found: report "No integration test projects found" and stop.

If user specified a project, filter to only that project.

### 3. Test → Fix Loop

Set `ATTEMPT = 1` and `MAX_ATTEMPTS = 10`.

**For each attempt:**

#### 3a. Run Integration Tests

For each discovered project:

```bash
dotnet test <project> --logger "trx;LogFileName=integration-<name>.trx" --results-directory "artifacts/test-results"
```

#### 3b. Check Results

- **If all tests pass**: Report SUCCESS and stop.
- **If tests fail**:
  1. Read the test output to identify failing tests
  2. For each failing test:
     - Understand the failure (test name, exception, expected vs actual)
     - Read the test file
     - Read the source code under test (repositories, services, migrations, etc.)
     - Determine if the fix belongs in the test or the source code
     - Fix accordingly
  3. Verify fixes compile: `dotnet build`
  4. Increment `ATTEMPT`
  5. If `ATTEMPT > MAX_ATTEMPTS`: **stop and report** (see section 4)
  6. Go back to step 3a

### 4. On Reaching Max Attempts

If 10 attempts are exhausted without all tests passing, **stop immediately** and report:

- Number of attempts made (10/10)
- List of still-failing tests with details:
  - Test name and project
  - Failure reason (exception, assertion, timeout, etc.)
  - What was tried to fix it
- Request human intervention to decide next steps

### 5. On Success

When all integration tests pass, report:

- Number of attempts needed
- Number of integration test projects executed
- Summary of fixes applied per attempt
- Confirm: "All integration tests passing."

## Rules for Fixing Integration Tests

1. **Understand the infrastructure** — integration tests use real databases (Testcontainers PostgreSQL), not mocks
2. **Check migrations** — failures often relate to schema mismatches between migrations and entity mappings
3. **Check connection strings** — Testcontainers generate dynamic ports; ensure tests use the container-provided connection
4. **Check test isolation** — each test should set up and tear down its own data; shared state causes flaky tests
5. **Do NOT mock infrastructure** in integration tests — the whole point is testing real integration
6. **Run `dotnet build`** after each fix to verify compilation
7. **Use AAA pattern** (Arrange, Act, Assert) — mandatory
8. **Use project conventions**: xUnit, Shouldly, Moq (only for non-infrastructure dependencies), Bogus

## Common Integration Test Issues

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| Connection refused | Docker not running or container failed to start | Start Docker, check Testcontainers logs |
| Table not found | Missing or outdated migration | Add/fix FluentMigrator migration |
| Column mismatch | Entity mapping differs from migration schema | Align entity properties with migration columns |
| Timeout | Slow container startup or long-running query | Increase timeout, optimize query |
| Data conflict | Tests sharing state | Ensure test isolation with unique data per test |

## Operating Principles

- **Focused scope**: Only integration tests. Nothing else.
- **Fast feedback**: Run only the failing test project during fixes, not all projects.
- **Minimal changes**: Only fix what is needed to make tests pass. Do not refactor or "improve" unrelated code.
- **Respect retry limit**: Never exceed 10 attempts. Stop and report.
- **Transparency**: Report progress after each attempt (attempt N/10, X tests failing).
