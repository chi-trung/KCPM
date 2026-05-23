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

## Weekly Report for 8-Week Workflow

If you need one report per week for the verification course, create a weekly summary file in the same folder, for example:

- `WEEK-01.md`
- `WEEK-02.md`
- `WEEK-03.md`

Each weekly report should summarize:

- which Jira task(s) were tested that week,
- which 1-2 test cases were actually executed,
- whether the result was pass or fail,
- whether a defect subtask was created for Member 2,
- and which artifact was used as evidence.

If you use Allure, treat the generated HTML as the presentation layer and keep the raw evidence alongside it, for example:

- xUnit `.trx` or test output
- Newman `.xml` / `.json`
- GitHub Actions run link
- Jira comment or subtask key

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

## Weekly Summary Template

Use this format when you need one report per week for the 8-week workflow:

```md
# WEEK-01: Verification Summary

## 📋 Weekly Information

| Field | Value |
|-------|-------|
| **Week** | 1 |
| **Jira Epic / Task** | WRP-BE-TESTS-XXX |
| **Primary Tester** | Your Name |
| **Report Style** | Allure + raw evidence |
| **Created Date** | YYYY-MM-DD |

## 🎯 Objective

Describe what was verified this week and why it matters.

## ✅ Test Cases Executed

| Test Case ID | Function / Feature | Result | Evidence |
|--------------|--------------------|--------|----------|
| TC-XXX | Feature 1 | Pass / Fail | Allure / TRX / Postman link |
| TC-YYY | Feature 2 | Pass / Fail | Allure / TRX / Postman link |

## 🔄 Execution Notes

- Environment used
- Test data used
- Any blocked steps
- Any defect subtask created for Member 2

## 🐛 Defects / Follow-up

| Defect ID | Description | Owner | Status |
|-----------|-------------|-------|--------|
| KIEM-XXX-subtask | Defect summary | Member 2 | Open / Fixed |

## 📎 Evidence

- Jira issue link
- Jira subtask link
- GitHub Actions run link
- Allure report artifact link

## 📝 Week Conclusion

State the overall result of the week in 2-3 lines.
```
