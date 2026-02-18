---
description: Implement a feature or fix, writing production code and unit tests iteratively until build and tests pass (max 10 attempts).
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). The user may specify:
- A GitHub issue number (e.g., `#42`) — read the issue for requirements
- A description of what to implement
- A file or area of the codebase to work on

If a GitHub issue number is provided, fetch it with `gh issue view <number>` to understand the full requirements.

## Goal

Implement the requested feature or fix by writing **production code and corresponding unit tests** iteratively until the solution **compiles and all tests pass**. This command focuses **exclusively** on:

1. Understanding the requirements (from issue, user input, or codebase context)
2. Writing production code (entities, services, repositories, value objects, etc.)
3. Writing unit tests for the new code
4. Iterating until `dotnet build` and `dotnet test` pass

This command does **NOT** run: architecture tests, mutation tests, SonarCloud checks, integration tests, full pipeline scripts, PR creation, or any other pipeline step. Those are handled by subsequent commands.

> **Recommended flow:** `/bedrock.implement` → `/bedrock.architecture` → `/bedrock.unittests` → `/bedrock.mutation` → `/bedrock.sonar` → `/bedrock.integration`. This is the **first step** — write the code before validating it.

## Execution Steps

### 1. Understand Requirements

- If a GitHub issue was provided: read it with `gh issue view <number>`
- Read relevant existing source code to understand patterns, conventions, and dependencies
- Identify which projects/namespaces will be affected
- Identify which test projects need new tests

### 2. Plan the Implementation

Before writing code, briefly outline:
- What files will be created or modified
- What tests will be written
- What dependencies are needed (NuGet packages, project references)

Report this plan to the user before proceeding.

### 3. Implement → Build → Test Loop

Set `ATTEMPT = 1` and `MAX_ATTEMPTS = 10`.

**For each attempt:**

#### 3a. Write Code

- Write or modify production code following project conventions
- Write or modify unit tests for the new/changed code

#### 3b. Build

```bash
dotnet build
```

If build fails: read the errors, fix them, and retry build (same attempt, do not increment counter).

#### 3c. Run Affected Tests

```bash
dotnet test <affected-test-project>
```

If tests fail:
1. Read the test output to identify failures
2. Fix the production code or test code as appropriate
3. Go back to step 3b (same attempt, do not increment counter)

#### 3d. Check Progress

- **If all new code compiles and all tests pass**: Report SUCCESS and stop.
- **If implementation is incomplete** (more features/tests to write):
  - Continue writing code (go back to step 3a, same attempt)
- **If stuck on a failure after multiple fix attempts**:
  - Increment `ATTEMPT`
  - If `ATTEMPT > MAX_ATTEMPTS`: **stop and report** (see section 4)
  - Re-evaluate the approach and try a different strategy

### 4. On Reaching Max Attempts

If 10 attempts are exhausted, **stop immediately** and report:

- Number of attempts made (10/10)
- What was implemented successfully
- What is still failing or incomplete
- Request human intervention to decide next steps

### 5. On Success

When implementation is complete (builds and tests pass), report:

- Summary of what was implemented
- Files created or modified
- Tests created or modified
- Confirm: "Implementation complete. Build and tests passing."
- Suggest: "Run `/bedrock.architecture` to validate architecture compliance."

## Coding Conventions

### Production Code
- **Namespace**: `Bedrock.*` following existing project structure
- **Target Framework**: .NET 10.0
- Follow existing patterns in the codebase (read similar files first)
- Use BuildingBlocks base classes where appropriate (Entity, ValueObject, AggregateRoot, etc.)

### Unit Tests
- **Use AAA pattern** (Arrange, Act, Assert) — mandatory
- **Use project conventions**: xUnit, Shouldly, Moq, Bogus
- **Inherit from TestBase** and use `LogArrange`, `LogAct`, `LogAssert`
- **1:1 relationship** between `src` project and `tests/UnitTests` project
- **Naming**: `Bedrock.UnitTests.<namespace-do-src>`
- Write tests **alongside** production code, not after — TDD when possible

### Project References
- If creating a new project, add it to the solution: `dotnet sln add <project>`
- If adding NuGet packages, use: `dotnet add <project> package <package>`
- If adding project references, use: `dotnet add <project> reference <referenced-project>`

## Operating Principles

- **Focused scope**: Only implement the requested feature/fix. Do not fix unrelated issues.
- **Fast feedback**: Use `dotnet build` frequently. Run only affected test projects, not all.
- **Read before write**: Always read existing code to understand patterns before writing new code.
- **Incremental progress**: Write small pieces, build, test, repeat. Do not write everything at once.
- **Respect retry limit**: Never exceed 10 attempts. Stop and report.
- **Transparency**: Report what you are implementing at each step.
