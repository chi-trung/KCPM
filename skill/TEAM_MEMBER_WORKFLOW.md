# Team Member Workflow Guide

This guide is for every team member who pulls the repo and works on one task per branch.

## Goal

- Keep one task on one branch.
- Always sync the latest `main` before starting work.
- Write tests for the assigned task.
- Push code so GitHub Actions can build the report and publish it to GitHub Pages.
- Keep Jira, branch names, commit messages, and PR titles traceable.

## 1. Branch Rule

- One task equals one branch.
- Example branch names:
  - `feature/KIEM-19-add-xunit-tests`
  - `bugfix/KIEM-4-fix-owner-sync`
  - `test/KIEM-16-postman-check`
- Do not mix multiple Jira tasks in one branch.

## 2. Sync From `main`

Before you start work, pull the latest changes from `main` into your branch.

Recommended flow:

```bash
git fetch origin
git checkout your-branch
git merge origin/main
```

If your team prefers rebase, use this instead:

```bash
git fetch origin
git checkout your-branch
git rebase origin/main
```

If there is a conflict, fix it immediately, run the tests again, and only then push.

## 3. What Each Member Should Do

### If you are assigned to write tests

- Write xUnit tests for backend behavior.
- Add integration tests when the flow spans more than one layer.
- Add Postman/Newman checks if the task is API-focused.
- Keep the test name clear and tied to the Jira key when possible.

### If you are assigned to implement code

- Implement only the task on your Jira branch.
- Do not silently change unrelated logic.
- Keep commits small and traceable.
- Fix the failing test or defect, then rerun the same checks.

### If you are reviewing

- Check that the branch was synced from `main`.
- Check that tests passed.
- Check that the Jira key appears in the branch, commit, or PR title.

## 4. Test Order

Use this order whenever possible:

1. Unit tests
2. Integration tests
3. System tests
4. Acceptance tests

For this repository, that usually means:

- xUnit for backend logic
- API smoke checks with Newman/Postman
- Allure for reporting results

## 5. Allure and GitHub Pages

- GitHub Actions will generate the report.
- Allure is the presentation layer for test results.
- The published GitHub Pages report should show the real assignee name when Jira owner sync is working.
- If the owner looks wrong on Pages, check the CI logs first, then check the raw Allure result JSON.

## 6. Commit and PR Rules

- Put the Jira key in commit messages when possible.
- Put the Jira key in PR titles.
- Keep PRs focused on one task.
- Do not merge until the checks and review are complete.

## 7. When You Start a New Round of Work

1. Pull the latest `main` into your branch.
2. Re-run the relevant tests.
3. Update the branch with any new changes from `main`.
4. Push again so CI and Pages stay aligned.

## 8. For Copilot or Other Agents

If an agent is helping you in this repo, follow this file first:

- Read the Jira key.
- Sync from `main`.
- Run only the tests related to the assigned task.
- Keep the branch clean and traceable.
- Publish through the same CI path so the report and owner labels stay consistent.

## 9. Short Reminder

If you are a member working on a task, do this in order:

- pull `main`
- work on one branch
- write tests
- push
- wait for CI
- verify Allure and GitHub Pages

## 10. How Allure Gets Owner From Jira

Allure does not guess the owner by branch name or module name. It needs a Jira key in the test result.

- Put the Jira key like `KIEM-10` in the test metadata, label, link, or test name when possible.
- The CI workflow will scan Allure result JSON and extract keys such as `KIEM-...` automatically.
- After that, the workflow queries Jira to get the assignee display name.
- The report then shows that assignee in `Owner`.

Practical rule:

- If a test result has no Jira key, the workflow cannot map it to an owner.
- If the Jira key is present, owner sync should work without manual editing.

Quick checklist before you push:

- Does the test case have a Jira key?
- Does the branch name include the Jira key?
- Did you sync latest `main`?
- Did you run the relevant tests?
- Did CI generate Allure results with the key still visible?
