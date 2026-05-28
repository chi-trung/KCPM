# Manual Owner Mapping Fallback

Use this when Jira sync cannot resolve every issue key, but the report still needs the correct assignee name.

## When To Use

- Jira API returns 404, 410, or permission errors for one or more keys.
- The CI run succeeds, but `Owner` still shows a placeholder like `auth` or `backend`.
- A teammate needs the report to show the real Jira assignee name now, without waiting for a Jira permission fix.

## Source Of Truth

The report pipeline uses `Waste-Recycling-Platform/allure-results/local-owner-map.json` as a fallback map.

Each entry should map a Jira key to the assignee display name:

```json
{
  "KIEM-4": { "displayName": "Nguyễn Chí Trung", "accountId": null, "unassigned": false },
  "KIEM-14": { "displayName": "Nguyễn Chí Trung", "accountId": null, "unassigned": false }
}
```

If you also use a raw label like `auth`, add it as an alias only when the report still needs it.

## What To Update

1. Update `local-owner-map.json` with the real Jira `displayName`.
2. Keep the Jira key list aligned with the issue keys in the Allure results.
3. Run the pipeline steps again:
   - `sync_jira_owners.py`
   - `inject_owners_into_results.py`
   - `build_validation_page.py`
   - `build_site_index.py`
4. Push so CI can publish the report to GitHub Pages.

## Rules

- Do not guess owners from branch names or module names.
- Do not leave `displayName` empty if you already know the Jira assignee.
- Do not remove the Jira issue key from the test metadata; owner sync still needs it.
- If Jira later becomes reachable, keep the local fallback only as a safety net.

## Good Practice

- Keep one key per line in the map.
- Use the exact Jira display name that appears in the Jira issue JSON.
- Keep the file small and explicit so the team can review it quickly.

## Example Workflow For A New Task

1. Read the Jira issue.
2. Confirm the assignee display name from Jira.
3. Add or update the key in `local-owner-map.json`.
4. Rebuild the report.
5. Verify the published Pages site shows the right `Owner`.

## When To Remove It

Remove a fallback entry only after the live Jira sync reliably resolves that key in CI.
