# TC-UPLOAD-009 — Collector Evidence: Empty File Rejected

**Module:** File Uploads & Storage — Collector Evidence  
**Jira:** [KIEM-20](https://ut-team-36.atlassian.net/browse/KIEM-20)  
**Type:** Negative  
**Severity:** Critical  
**Owner:** Nguyễn Minh Phụng

---

## Objective
Verify that a zero-byte evidence file passed to `LocalFileStorageService` is rejected with `ArgumentException("File is empty")`, consistent with citizen report upload behaviour.

---

## Pre-conditions
- `LocalFileStorageService` initialized.
- File has valid extension (`.jpg`) but zero bytes.

---

## Test Steps

### Unit Test
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` with `evidence.jpg` of 0 bytes | Throws `ArgumentException` with message `"File is empty"` |
| 2 | Confirm no file written to `uploads/` | Folder unchanged |

### Postman (API)
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `PUT /api/collector/tasks/{id}/complete` with `WeightKg=1.5` and `Images` = 0-byte `.jpg` | Current controller skips 0-byte files (line 265: `if (file.Length == 0) continue;`) — task still completes `200 OK` |

> **Note:** The controller silently skips empty files but does **not** reject the whole request. If at least 1 evidence image is enforced (fix from TC-008b), this case would then return `400` after skipping empty files.

---

## Postman Test Script
```javascript
pm.test("Status 200 (current: empty file silently skipped)", () => {
    pm.response.to.have.status(200);
});
// After TC-008b fix is applied, this should become:
// pm.test("Status 400 (no valid images after skipping empty)", () => {
//     pm.response.to.have.status(400);
// });
```

---

## Linked Unit Tests
- `CollectorEvidenceUploadTests.CollectorUpload_WithEmptyFile_ShouldThrowArgumentException`

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
