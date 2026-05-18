# Test Case Report Guide

Use this folder to store `.md` reports for each test case.

## Purpose

Each report should help another member understand:

- what was tested,
- what data was used,
- what result was expected,
- what result was actually observed,
- and whether anything failed or needed a fix.

## File Naming

Use one clear file name per test case, for example:

- `TC-REP-001.md`
- `TC-AUTH-004.md`
- `TC-SIGNALR-016.md`

## Suggested Structure

Keep the report in this order:

1. Title with the test case ID and short summary
2. Test information table
3. Objective
4. Pre-conditions
5. Test data
6. Test steps
7. Expected results
8. Actual results
9. Status
10. Defects or notes
11. Related test cases

## Simple Template

Copy and fill this template when creating a new report:

```md
# TC-XXX: Short Test Name

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-XXX |
| **Jira Task** | KIEM-XXX |
| **Module** | Module Name |
| **Priority** | High / Medium / Low |
| **Test Type** | Positive / Negative |
| **Created Date** | YYYY-MM-DD |
| **Created By** | Your Name |

## 🎯 Objective

Describe what this test verifies.

## ✅ Pre-conditions

1. List the required setup before testing.

## 🔧 Test Data

### Request Headers
```http
Authorization: Bearer {token}
Content-Type: application/json
```

### Request Body

| Field | Value | Type |
|-------|-------|------|
| ExampleField | ExampleValue | text |

## 📝 Test Steps

1. Step 1
2. Step 2
3. Step 3

## ✔️ Expected Results

- Expected status code
- Expected response body
- Expected database or UI change

## 🔄 Actual Results

Record what happened during execution.

## 📊 Status

⬜ Not Tested | ✅ Pass | ⬜ Fail

## 🐛 Defects (if any)

| Defect ID | Description | Severity | Status |
|-----------|-------------|----------|--------|
| ⬜ | ⬜ | ⬜ | ⬜ |

## 🔗 Related Test Cases

- TC-XXX

## 📝 Notes

- Add any important observations here.
```

## Tips

- Keep the wording short and easy to scan.
- If the API response changes, update the report right away.
- If you use Postman scripts, include the script snippet at the bottom.
- If a test fails, note the root cause and the fix.
