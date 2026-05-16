# Backend Testing Playbook

This repository now has a backend testing setup designed to be easy to explain in class and easy to prove in GitHub.

## Stack

- Unit tests: xUnit
- Assertions: FluentAssertions
- Mocking: Moq
- Coverage: coverlet.collector
- CI: GitHub Actions workflow at `.github/workflows/backend-tests.yml`
- Postman CI smoke: GitHub Actions workflow at `.github/workflows/postman-smoke.yml`
- Manual API smoke tests: Postman collection in `postman/WastePlatform.postman_collection.json`

## Linking GitHub, Jira, and Postman

Use the same Jira key everywhere:

- Branch name: `feature/WASTE-TEST-001-unit-tests`
- Commit message: `WASTE-TEST-001: add backend unit tests`
- GitHub PR title: `WASTE-TEST-001 backend test suite`
- Postman collection folder/request names: `[WASTE-TEST-001] ...`

That gives you a clean trace from Jira issue to GitHub commit to Postman request.

## What is already included

- A real backend test project at `backend/tests/WastePlatform.Tests`
- Sample unit tests for domain state transitions and an application command handler
- A GitHub Actions workflow that runs on push and pull request
- A Postman collection with health and auth smoke requests

## Suggested next steps

1. Add controller tests with `WebApplicationFactory`.
2. Add integration tests with Testcontainers if you want DB-backed proof.
3. Add a PR template requiring Jira key and Postman evidence.