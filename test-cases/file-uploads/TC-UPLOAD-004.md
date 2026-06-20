# TC-UPLOAD-004 — Reject Oversized File / Accept At-Limit File

**Module:** File Uploads & Storage  
**Jira:** [KIEM-20](KIEM-20)  
**Type:** Negative + Boundary  
**Severity:** Critical  
**Owner:** Nguyễn Minh Phụng

---

## Objective
Verify that files exceeding the 5 MB size limit are rejected, while files exactly at the limit are accepted.

---

## Pre-conditions
- Max allowed size: **5 MB** (5 × 1024 × 1024 = 5,242,880 bytes).
- File extension is `.jpg` (valid).

---

## Test Steps

### Unit Test
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` with file of size `5,242,881` bytes | Throws `InvalidOperationException`: `"File size exceeds limit."` |
| 2 | Call `SaveFileAsync` with file of size `5,242,880` bytes (exactly 5 MB) | Returns valid filename, file saved |
| 3 | Call `SaveFileAsync` with file of size `1` byte | Returns valid filename, file saved |

### Postman (API)
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `POST /api/reports` with a 6 MB `.jpg` file | `400 Bad Request`, message references size limit |
| 2 | `POST /api/reports` with a 4.9 MB `.jpg` file | `200 OK`, report created |

---

## Postman Test Script (oversized)
```javascript
pm.test("Status is 400 for oversized file", () => pm.response.to.have.status(400));
pm.test("Error mentions size limit", () => {
    pm.expect(pm.response.text()).to.include("exceeds");
});
```

---

## Linked Unit Tests
- `LocalFileStorageServiceTests.SaveFileAsync_WithOversizedFile_ShouldThrowInvalidOperationException`
- `LocalFileStorageServiceTests.SaveFileAsync_WithFileSizeAtLimit_ShouldSaveSuccessfully`

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
