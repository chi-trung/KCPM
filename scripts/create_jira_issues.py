#!/usr/bin/env python3
"""
Create Jira Epic and Tasks from jira.md

Reads `jira.md` at repo root, creates an Epic (or fallback task) and creates Task issues
for each `Mã công việc` entry. Links created issues to the Epic using an issue link (Relates).

Environment variables used (set as GitHub Actions secrets):
  JIRA_BASE_URL       e.g. https://your-domain.atlassian.net
  JIRA_API_EMAIL      Jira account email for API
  JIRA_API_TOKEN      Jira API token
  JIRA_PROJECT_KEY    Jira project key (e.g. KIEM)
  EPIC_ISSUE_KEY      (optional) existing epic key to reuse

This script intentionally avoids assigning issues to users (accountId mapping varies).
"""
import os
import sys
import re
import json

import requests


def read_jira_md(path="jira.md"):
    with open(path, encoding="utf-8") as f:
        return f.read()


def parse_epic_title(text):
    # Find line starting with '# 🟦 EPIC:'
    m = re.search(r"^#\s*🟦\s*EPIC:\s*(.+)$", text, re.MULTILINE)
    if m:
        return m.group(1).strip()
    return None


def parse_tasks(text):
    tasks = []
    lines = text.splitlines()
    i = 0
    while i < len(lines):
        line = lines[i]
        if "**Mã công việc:**" in line:
            # attempt parse code and title
            m = re.search(r"\*\*Mã công việc:\*\*\s*`(?P<code>[^`]+)`\s*—\s*(?P<title>.+)", line)
            if m:
                code = m.group("code").strip()
                title = m.group("title").strip()
            else:
                # fallback: take whole line
                parts = line.split("**Mã công việc:**", 1)[1].strip()
                code = parts.split()[0].strip('`')
                title = "".join(parts.split()[1:]).strip(' -–—')

            # collect following description lines until blank or next '##'
            desc_lines = []
            j = i + 1
            while j < len(lines) and not lines[j].startswith("##") and lines[j].strip() != "":
                desc_lines.append(lines[j])
                j += 1

            description = "\n".join(desc_lines).strip()
            tasks.append({"code": code, "title": title, "description": description})
            i = j
        else:
            i += 1
    return tasks


def jira_request(base_url, email, token, method, path, payload=None):
    url = base_url.rstrip("/") + path
    auth = (email, token)
    headers = {"Accept": "application/json", "Content-Type": "application/json"}
    if payload is not None:
        r = requests.request(method, url, auth=auth, headers=headers, data=json.dumps(payload))
    else:
        r = requests.request(method, url, auth=auth, headers=headers)
    try:
        data = r.json()
    except Exception:
        data = r.text
    if not (200 <= r.status_code < 300):
        raise SystemExit(f"Jira API error {r.status_code} {r.text}")
    return data


def create_issue(base_url, email, token, project_key, summary, description, issuetype="Task"):
    payload = {
        "fields": {
            "project": {"key": project_key},
            "summary": summary,
            "description": description or "",
            "issuetype": {"name": issuetype},
            "labels": ["auto-jira"]
        }
    }
    return jira_request(base_url, email, token, "POST", "/rest/api/3/issue", payload)


def create_issue_link(base_url, email, token, inward_issue_key, outward_issue_key, link_type_name="Relates"):
    payload = {
        "type": {"name": link_type_name},
        "inwardIssue": {"key": inward_issue_key},
        "outwardIssue": {"key": outward_issue_key}
    }
    return jira_request(base_url, email, token, "POST", "/rest/api/3/issueLink", payload)


def main():
    base_url = os.getenv("JIRA_BASE_URL")
    email = os.getenv("JIRA_API_EMAIL")
    token = os.getenv("JIRA_API_TOKEN")
    project_key = os.getenv("JIRA_PROJECT_KEY")
    epic_issue_key = os.getenv("EPIC_ISSUE_KEY")

    if not base_url or not email or not token or not project_key:
        print("Missing required env vars. Please set JIRA_BASE_URL, JIRA_API_EMAIL, JIRA_API_TOKEN, JIRA_PROJECT_KEY")
        sys.exit(1)

    text = read_jira_md()
    epic_title = parse_epic_title(text) or f"{project_key} - Automated Epic"
    print(f"Epic title parsed: {epic_title}")

    if epic_issue_key:
        print(f"Using provided EPIC_ISSUE_KEY={epic_issue_key}")
    else:
        # Try create an Epic issue. If it fails due to missing epic name field, fallback to Task labeled EPIC
        try:
            # many Jira instances require a customfield for Epic Name; try common id customfield_10011
            payload = {
                "fields": {
                    "project": {"key": project_key},
                    "summary": epic_title,
                    "issuetype": {"name": "Epic"},
                    "description": "Automated Epic created from jira.md",
                    "labels": ["auto-jira"],
                    "customfield_10011": epic_title
                }
            }
            data = jira_request(base_url, email, token, "POST", "/rest/api/3/issue", payload)
            epic_issue_key = data.get("key")
            print(f"Created Epic: {epic_issue_key}")
        except SystemExit as e:
            print(f"Epic creation with Epic type failed, falling back to Task: {e}")
            data = create_issue(base_url, email, token, project_key, f"EPIC: {epic_title}", "Fallback Epic (Task)", issuetype="Task")
            epic_issue_key = data.get("key")
            print(f"Created fallback Epic-task: {epic_issue_key}")

    tasks = parse_tasks(text)
    print(f"Found {len(tasks)} tasks to create")
    created = []
    for t in tasks:
        summary = f"{t['code']} - {t['title']}"
        desc = t.get("description") or "Imported from jira.md"
        print(f"Creating issue: {summary}")
        data = create_issue(base_url, email, token, project_key, summary, desc, issuetype="Task")
        key = data.get("key")
        created.append((t['code'], key))
        print(f"Created {key}")
        try:
            create_issue_link(base_url, email, token, epic_issue_key, key)
            print(f"Linked {key} to epic {epic_issue_key}")
        except SystemExit as e:
            print(f"Failed to link {key} to epic: {e}")

    print("Done. Created issues:")
    for code, key in created:
        print(f"  {code} -> {key}")


if __name__ == "__main__":
    main()
