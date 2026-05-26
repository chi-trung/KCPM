# Allure xUnit Owner Sync Guide

This guide explains how to write xUnit tests so GitHub Pages, Allure, and Jira owner sync can read the result correctly.

## Why This Exists

Several newly pulled test files look valid as xUnit tests, but they do not feed the Allure pipeline the fields it needs.

The report pipeline in this repo does not guess ownership from the branch name or the module name. It reads Jira keys from Allure result metadata, then maps those keys to assignees.

If a test does not expose a Jira key in a supported Allure field, the workflow cannot resolve the owner.

## What The Workflow Reads

The owner sync logic looks for Jira keys in these places:

- `AllureIssue("https://.../browse/KIEM-xx")`
- `labels` or `links` in the raw Allure JSON where the label name is `issue`, `Issue`, or `ISSUE`
- issue-type links that contain the Jira key

It does not read custom label names like `KIEM` or `WRP` unless another script explicitly maps them.

## Required Class Metadata

Use a consistent set of class-level labels for every xUnit test class:

- `AllureEpic(...)`
- `AllureFeature(...)`
- `AllureLabel("story", ...)`
- `AllureLabel("parentSuite", "xUnit Backend Tests")`
- `AllureLabel("suite", "Controllers" | "Application" | "Domain")`
- `AllureLabel("subSuite", ...)`
- `AllureLabel("package", "WastePlatform.Tests....")`
- `AllureOwner("auth" | "backend" | "qa")`
- `AllureSeverity(...)`
- `AllureTag(...)`
- `AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-xx")`

## The Most Important Rule

If the test belongs to a Jira task, put the Jira key in a supported issue field.

Good:

```csharp
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-4")]
```

Not enough for owner sync:

```csharp
[Allure.Net.Commons.Attributes.AllureLabel("KIEM", "KIEM-13")]
[Allure.Net.Commons.Attributes.AllureLabel("WRP", "WRP-BE-TESTS-013")]
```

Those labels may be useful for humans, but the current sync flow does not treat them as Jira keys.

## Good Example

`Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Controllers/AuthControllerTests.cs` is the right pattern:

- It has `AllureOwner("auth")`.
- It has a real Jira issue link via `AllureIssue(...)`.
- It has clear suite metadata.
- Attachments use unique names so the report stays readable.

## Common Problems In New Tests

### 1. Missing `AllureOwner`

Example problem:

- A new report test has `story`, `suite`, and `package`, but no `AllureOwner`.

Result:

- The report may render, but the owner card is empty or wrong.

Fix:

- Add a real owner label such as `AllureOwner("backend")` or `AllureOwner("qa")`.

### 2. Missing `AllureIssue`

Example problem:

- A new test has a branch-related label like `KIEM-13` in a generic `AllureLabel`, but not an issue link.

Result:

- Jira sync does not find the key.
- The workflow cannot map the test to an assignee.

Fix:

- Add `AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-13")`.

### 3. Using The Wrong Label Name

Example problem:

```csharp
[Allure.Net.Commons.Attributes.AllureLabel("KIEM", "KIEM-13")]
[Allure.Net.Commons.Attributes.AllureLabel("WRP", "WRP-BE-TESTS-013")]
```

Result:

- These labels are visible to humans, but the owner sync script does not scan them.

Fix:

- Keep the labels if you want, but still add `AllureIssue(...)`.

### 4. Missing Subsuite Or Package Consistency

Some files use only class name and story, while others use `subSuite` and `package`.

Result:

- The report groups are inconsistent and harder to browse.

Fix:

- Use the same `parentSuite`, `suite`, `subSuite`, and `package` pattern across all test files in the same folder.

## Recommended Pattern Per Folder

### Controllers

- `parentSuite`: `xUnit Backend Tests`
- `suite`: `Controllers`
- `subSuite`: controller test class name
- `package`: `WastePlatform.Tests.Controllers`
- `AllureOwner`: team owner such as `auth`, `qa`, or `backend`
- `AllureIssue`: Jira URL for the related task

### Application

- `parentSuite`: `xUnit Backend Tests`
- `suite`: `Application`
- `subSuite`: handler or service class name
- `package`: `WastePlatform.Tests.Application.<Module>`
- `AllureOwner`: `backend`
- `AllureIssue`: Jira URL

### Domain

- `parentSuite`: `xUnit Backend Tests`
- `suite`: `Domain`
- `subSuite`: entity or domain test class name
- `package`: `WastePlatform.Tests.Domain`
- `AllureOwner`: `backend`
- `AllureIssue`: Jira URL if the test belongs to a Jira task

## Test Method Metadata

At test-method level, keep the evidence specific:

- Use `AllureDescription(...)` for the scenario.
- Use `AllureAttachmentHelper.AttachJson(...)` or `AttachText(...)` with unique names.
- Prefer a Jira-specific test case label only if it is also backed by a real issue link.

Example:

```csharp
[Fact]
[AllureDescription("Registers a new citizen, returns a JWT token, and persists the user in the database.")]
public async Task Register_WithValidCitizen_ShouldReturnOkAndCreateUser()
```

## What To Fix In Newly Pulled Report Tests

When you add or review a new report test, check these items before pushing:

- Does the class have `AllureOwner`?
- Does the class have `AllureIssue` with a real Jira URL?
- Does the file use the standard `parentSuite`, `suite`, `subSuite`, and `package` values?
- Does the test method have a clear `AllureDescription`?
- Do attachments have unique names so they do not overwrite each other?

## Practical Rule

If the owner sync matters, the test must carry a Jira key in a field the pipeline actually reads.

If you only add `AllureLabel("KIEM", "KIEM-13")`, that is not enough for the current sync flow.

## Suggested Review Comment

Use this comment when reviewing a pulled PR:

> Please add a real `AllureOwner(...)` and `AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-xx")` to each xUnit class so Jira owner sync can map the report correctly. Custom labels like `KIEM` or `WRP` are not enough for the current pipeline.

## What To Tell An Agent

When you ask an agent to work on this repository, tell it to read the `skill/` folder first and follow the repo-specific playbook before editing anything.

Suggested instruction:

> Read the files in `skill/` first. Then inspect the relevant xUnit test file, keep the Allure labels consistent, make sure each class has a real `AllureOwner(...)` and `AllureIssue(...)`, and only then patch the tests. Do not guess the owner from the module name or branch name. Follow the existing report flow exactly.

Short version for task handoff:

> Agent, read `skill/` first. Then work strictly by the repo playbook so the xUnit test, Jira issue key, owner sync, and Allure report stay aligned.
