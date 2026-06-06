# TC-UPLOAD-003 — Reject Invalid File Extension

**Module:** File Uploads & Storage  
**Jira:** [KIEM-20](https://ut-team-36.atlassian.net/browse/KIEM-20)  
**Type:** Negative  
**Severity:** Critical  
**Owner:** Nguyễn Minh Phụng

---

## Objective
Verify that files with disallowed extensions (`.exe`, `.js`, `.pdf`, `.csv`, etc.) are rejected and not saved to disk.

---

## Pre-conditions
- Allowed extensions: `.jpg`, `.jpeg`, `.png`, `.gif`.
- File has non-zero content.

---

## Test Steps

### Unit Test
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` with `malware.exe` | Throws `InvalidOperationException` containing `"Invalid file type: .exe"` |
| 2 | Call `SaveFileAsync` with `script.js` | Throws `InvalidOperationException` containing `"Invalid file type: .js"` |
| 3 | Call `SaveFileAsync` with `document.pdf` | Throws `InvalidOperationException` containing `"Invalid file type: .pdf"` |
| 4 | Call `SaveFileAsync` with `data.csv` | Throws `InvalidOperationException` containing `"Invalid file type: .csv"` |

### Postman (API)
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `POST /api/reports` with `form-data`, `Images` = `.pdf` file | `400 Bad Request` or file silently skipped (no report created with pdf image) |
| 2 | Confirm no `.pdf` path in response imageUrls | Image array is empty or only contains valid images |

---

## Postman Test Script
```javascript
pm.test("Status is 400", () => pm.response.to.have.status(400));
pm.test("Error references invalid file type", () => {
    const text = pm.response.text();
    pm.expect(text).to.include("Invalid file type");
});
```

---

## Linked Unit Tests
- `LocalFileStorageServiceTests.SaveFileAsync_WithInvalidExtension_ShouldThrowInvalidOperationException` (Theory: exe, js, pdf, csv)

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
