# TC-UPLOAD-002 — Reject Empty or Null File

**Module:** File Uploads & Storage  
**Jira:** [KIEM-20](KIEM-20)  
**Type:** Negative  
**Severity:** Critical  
**Owner:** Nguyễn Minh Phụng

---

## Objective
Verify that empty (0-byte) or null files are rejected with a clear error message and no file is saved to disk.

---

## Pre-conditions
- `LocalFileStorageService` is initialized.

---

## Test Steps

### Unit Test
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` with a 0-byte file | Throws `ArgumentException` with message containing `"File is empty"` |
| 2 | Call `SaveFileAsync` with `null` | Throws `ArgumentException` with message containing `"File is empty"` |
| 3 | Confirm no file written to `uploads/` folder | Folder contents unchanged |

### Postman (API)
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `POST /api/reports` with `form-data`, `Images` field present but empty value | `400 Bad Request` |
| 2 | `POST /api/reports` with no `Images` field at all | `400 Bad Request`, message: `"At least one image is required"` |

---

## Postman Test Script
```javascript
pm.test("Status is 400", () => pm.response.to.have.status(400));
pm.test("Error message present", () => {
    const body = pm.response.json();
    pm.expect(body.message || body.errors).to.exist;
});
```

---

## Linked Unit Tests
- `LocalFileStorageServiceTests.SaveFileAsync_WithEmptyFile_ShouldThrowArgumentException`
- `LocalFileStorageServiceTests.SaveFileAsync_WithNullFile_ShouldThrowArgumentException`

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
