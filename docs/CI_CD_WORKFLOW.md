# CI/CD Workflow Guide

This repository uses a simple evidence-driven workflow for software verification coursework.

## Standard Team Flow

1. Create or receive a Jira issue.
2. Assign the Jira issue to a specific team member.
3. The member creates a branch that includes the Jira key.
   - Example: `feature/KIEM-24-fix-auth-login`
4. Every commit must include the Jira key.
   - Example: `KIEM-24: fix login validation`
5. The PR title must include the Jira key.
   - Example: `KIEM-24 - Fix login validation`
6. GitHub Actions runs tests and Postman automatically.
7. If a member pushes code:
   - The Jira issue is transitioned to an In Progress-like status when the workflow finds a matching transition.
   - A Jira comment is added automatically when the workflow can resolve the issue key.
8. If the Pull Request passes the Postman smoke workflow:
   - A Jira comment is added automatically.
   - The Jira issue is transitioned to a Done-like status when possible.
9. If the workflow fails:
   - A Jira comment is added automatically.
   - The Jira issue stays open for the member to fix and push again.

## Workflow Notes

- `backend-tests.yml` runs the .NET test suite only.
- `postman-smoke.yml` handles Jira comments and status transitions.
- Push events attempt to move the Jira issue to an In Progress-like status.
- Successful Pull Request runs attempt to move the Jira issue to a Done-like status.
- If Jira has different transition names, the workflow selects the closest matching transition automatically.

## What Each System Does

### Jira
- Tracks the task and assignment.
- Holds the evidence comments from automation.
- Stores the final status of the task.

### GitHub
- Enforces Jira key rules on PRs and commits.
- Runs backend tests, Postman smoke tests, and deploy workflows.
- Stores run history and artifacts as evidence.

### Postman
- Validates API behavior after each change.
- Provides PASS/FAIL evidence per run.
- Sends result feedback back into Jira through GitHub Actions.

## Evidence Required for Submission

For each Jira issue, keep these links ready:
- Jira issue link
- Branch link
- PR link
- GitHub Actions run link
- Test artifact link

## Member Rule

A task is not considered finished until:
- the code is pushed,
- tests pass,
- Jira has the latest comment/status,
- and the PR is merged or ready to merge.

## Test Case Report Guide

Use the report format in [test-cases/reports/README.md](../test-cases/reports/README.md) when creating a new `.md` test report.
