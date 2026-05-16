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
7. If Postman passes:
   - A Jira comment is added automatically.
   - The Jira issue is transitioned to a Done-like status when possible.
8. If Postman fails:
   - A Jira comment is added automatically.
   - The Jira issue stays open for the member to fix and push again.

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
