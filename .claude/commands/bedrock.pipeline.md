---
description: Execute the full Bedrock quality pipeline in sequence — implement, architecture, unit tests, mutation, sonar, integration — then create PR and merge.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). The user may specify:
- A GitHub issue number (e.g., `#42`) — passed to `/bedrock.implement`
- A description of what to implement
- `skip:implement` — skip the implement step (code already written)
- `skip:sonar` — skip the SonarCloud step (no SONAR_TOKEN)
- `skip:integration` — skip integration tests (no Docker)

Multiple skip flags can be combined (e.g., `skip:implement skip:sonar`).

## Goal

Execute the **complete Bedrock quality pipeline** by invoking each step in sequence. If any step fails after its max attempts, **stop the entire pipeline** and report.

## Pipeline Steps

```
/bedrock.implement       (1o) Write production code and tests
        ↓
/bedrock.architecture    (2o) Validate architecture compliance
        ↓
/bedrock.unittests       (3o) Ensure all unit tests pass
        ↓
/bedrock.mutation        (4o) Achieve 100% mutation score
        ↓
/bedrock.sonar           (5o) Resolve SonarCloud issues
        ↓
/bedrock.integration     (6o) Ensure all integration tests pass
        ↓
Commit, Push, PR, Merge  (7o) Deliver the code
```

## Execution Steps

### Step 1: Implement (skip if `skip:implement`)

Invoke `/bedrock.implement` with the user's arguments (issue number or description).

If it fails (max attempts reached): **stop pipeline**. Report which step failed.

### Step 2: Architecture

Invoke `/bedrock.architecture`.

If it fails (max attempts reached): **stop pipeline**. Report which step failed.

### Step 3: Unit Tests

Invoke `/bedrock.unittests`.

If it fails (max attempts reached): **stop pipeline**. Report which step failed.

### Step 4: Mutation

Invoke `/bedrock.mutation`.

If it fails (max attempts reached): **stop pipeline**. Report which step failed.

### Step 5: SonarCloud (skip if `skip:sonar`)

Invoke `/bedrock.sonar`.

If it fails (max attempts reached): **stop pipeline**. Report which step failed.

### Step 6: Integration (skip if `skip:integration`)

Invoke `/bedrock.integration`.

If it fails (max attempts reached): **stop pipeline**. Report which step failed.

### Step 7: Deliver

After all quality gates pass:

1. **Commit** all changes:
   - Stage relevant files (do NOT stage `.env`, credentials, or large binaries)
   - Create a descriptive commit message following project conventions

2. **Push** to the remote branch:
   ```bash
   git push -u origin HEAD
   ```

3. **Create PR** using `gh pr create`:
   - Title: descriptive, following project conventions
   - Body: include `Closes #<issue>` if an issue was provided
   - Follow the PR format from CLAUDE.md

4. **Wait for GitHub Actions pipeline**:
   ```bash
   gh pr checks <number> --watch
   ```

5. **If GitHub Actions passes**:
   ```bash
   gh pr merge <number> --squash --delete-branch
   git checkout main && git pull
   ```

6. **If GitHub Actions fails** (max 5 attempts):
   - Analyze: `gh run view <run-id> --log-failed`
   - Fix locally
   - Commit and push
   - Repeat until it passes or limit is reached

## On Pipeline Failure

When any step fails after its max attempts, **stop immediately** and report:

- Which step failed (e.g., "Step 4: Mutation")
- Steps completed successfully before the failure
- Summary of the failure (from the failed step's report)
- Suggest: "Fix the reported issues and re-run `/bedrock.pipeline skip:implement` to continue from architecture."

## On Pipeline Success

When the full pipeline completes and PR is merged, report:

- All steps completed successfully
- PR number and URL
- Issue closed (if applicable)
- Confirm: "Pipeline complete. Code delivered to main."

## Operating Principles

- **Sequential execution**: Each step must pass before the next begins.
- **Fail fast**: Stop the entire pipeline on first step failure. Do not skip failing steps.
- **Skip flags**: Respect user's skip flags for optional steps.
- **Respect all retry limits**: Each step has its own max attempts (10). GitHub Actions has 5.
- **Transparency**: Report which step is running and its outcome before moving to the next.
