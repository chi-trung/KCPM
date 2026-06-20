# Jira Automation — Create Epic & Tasks from jira.md

This automation creates Jira issues using the `jira.md` file in the repository root.

Setup (GitHub repository):

1. Add the following repository secrets (Settings → Secrets → Actions):
   - `JIRA_BASE_URL` — e.g. `JIRA_BASE_URL`
   - `JIRA_API_EMAIL` — Jira account email used to create an API token
   - `JIRA_API_TOKEN` — Jira API token
   - `JIRA_PROJECT_KEY` — Jira project key (e.g. `KIEM`)

2. Optional: provide an existing Epic issue key when running the workflow (workflow_dispatch input `epic_issue_key`). If left blank, the workflow will create a new Epic (or fallback to a Task if Epic creation requires a custom field).

How to run (manual):

- Go to the repository Actions → "Create Jira Issues" workflow → Run workflow.
- Optionally provide the `epic_issue_key` input.

Notes & limitations:
- The script avoids assigning issues because Jira Cloud now requires `accountId` for assignees; mapping emails to accountId requires admin API calls.
- The script attempts to create an Epic using a common Epic Name custom field (`customfield_10011`). If that fails it falls back to creating a `Task` labeled as the Epic.
- Links between created tasks and the Epic are created using an issue link of type "Relates".
- Review created issues after the run and adjust assignees or Epic-Link as your Jira instance requires.

Files added:
- `scripts/create_jira_issues.py` — Python script that parses `jira.md` and creates issues via Jira REST API.
- `.github/workflows/create-jira-issues.yml` — workflow to run the script manually.
