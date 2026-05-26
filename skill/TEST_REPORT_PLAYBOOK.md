# Test Report Playbook

This file explains how to run a task, keep the evidence in Allure, and let the rest of the team reproduce the result.

## What This Repo Uses for Evidence

- xUnit for backend tests.
- Newman/Postman for API smoke checks.
- Allure report pages for saved evidence.
- GitHub Pages for the published report.

## How To Work On One Task

1. Switch to the exact Jira branch for the task.
2. Pull the latest `main` into that branch.
3. Make the test or code change only for that Jira key.
4. Run the relevant test project locally.
5. Confirm the result appears in Allure.
6. Push the branch and open a PR into `main`.
7. Wait for CI and verify the Pages report.

## What To Test By Layer

- Controller tests:
  - check HTTP status codes,
  - check request validation,
  - check auth and role behavior,
  - check response body.
- Application tests:
  - check command handlers and services,
  - mock repositories and external services,
  - verify business rules.
- Domain tests:
  - check entity transitions,
  - check invariants,
  - check boundary and invalid cases.
- Infrastructure tests:
  - check repository logic,
  - check persistence behavior,
  - check side effects like SignalR or storage access.

## What Evidence Should Look Like

Keep these items visible in the report whenever possible:

- Jira key.
- Task name.
- Branch name.
- Test class name.
- Expected behavior.
- Owner mapping.

## How To Read the Published Report

- Root hub: the landing page at the GitHub Pages site.
- Main report: overall Allure execution.
- Validation page: quick checklist for the pipeline.
- Owner report: task evidence grouped by assignee.

If the owner is wrong:

1. Check whether the test result contains the Jira key.
2. Check whether Jira sync wrote the owner map.
3. Check whether the inject step updated the Allure JSON.
4. Re-run CI if the published Pages site is stale.

## Practical Rule For This Course

This is a verification exercise, so the important part is not a hand-in file. The important part is that the test exists, passes, and leaves a trace in the report that another teammate can open and verify.

## Quick Checklist Before Pushing

- Did I stay on the right Jira branch?
- Did I test only one task?
- Did I run the right xUnit suite?
- Did Allure capture the run?
- Did the report show the Jira key or owner?
- Did I avoid deleting the task branch history?
