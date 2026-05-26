# Team Quick Start

Use this when you just want to start one task fast and keep the report clean.

## 1. Pick the right branch

- One Jira key, one branch.
- Switch to the exact branch that already exists for that task.
- Do not create a second branch for the same key.
- Do not delete the old branch, because it keeps your work history.

## 2. Sync before you work

```bash
git fetch origin
git checkout your-branch
git merge origin/main
```

If your team prefers rebase:

```bash
git fetch origin
git checkout your-branch
git rebase origin/main
```

## 3. Test by layer

- Controller task: test HTTP status, auth, request body, response body.
- Application task: test command handler or service logic.
- Domain task: test entity rules, transitions, and boundaries.
- Infrastructure task: test repository, persistence, or side effects.

## 4. Keep traceability

- Put the Jira key in the test name, label, or link.
- Keep the branch name, commit message, and PR title tied to the same Jira key.
- Run the test so Allure captures it.

## 5. Read the report

- Hub: open the GitHub Pages landing page.
- Main report: open the Allure report.
- Validation page: check CI and owner sync.
- Owner report: verify the assignee name from Jira.

## 6. What to do before PR

- Run the related xUnit tests.
- Check the result in Allure.
- Confirm the owner is mapped correctly.
- Push the branch and open the PR into `main`.

## One-line rule

If the test does not show up in Allure, it is not done yet.
