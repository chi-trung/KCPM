# TC-UPLOAD-006 — IO Exception Propagation on Disk Write Failure

**Module:** File Uploads & Storage  
**Jira:** [KIEM-20](KIEM-20)  
**Type:** Negative / Abnormal Case  
**Severity:** Critical  
**Owner:** Nguyễn Minh Phụng

---

## Objective
Verify that when `IFormFile.CopyToAsync` fails (e.g. disk full, permission denied), the `IOException` bubbles up unchanged from `SaveFileAsync` — no exception is swallowed, and no corrupt partial file is silently left on disk.

Corresponds to SRS Abnormal Case (Page 35):  
> *"Lỗi upload ảnh → Hệ thống báo lỗi và yêu cầu thử lại."*

---

## Pre-conditions
- `LocalFileStorageService` is configured with a writable `ContentRootPath`.
- `IFormFile.CopyToAsync` is mocked to throw `IOException("No space left on device")`.
- File passes all previous validations (valid extension, non-empty, within size limit).

---

## Test Steps

### Unit Test
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create valid `.jpg` `IFormFile` mock where `CopyToAsync` throws `IOException` | — |
| 2 | Call `SaveFileAsync(file, allowedExtensions, maxSize)` | Throws `IOException` with message `"No space left on device"` |
| 3 | Check `uploads/` folder for leftover files | Folder may contain an empty/partial file (FileStream already created) — verified and noted in Allure attachment |

### Postman (API)
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Simulate server-side disk full (or intercept) | `500 Internal Server Error` |
| 2 | Retry same request after disk space restored | `200 OK`, report created successfully |

---

## Postman Test Script (simulate error path)
```javascript
pm.test("Status 500 on storage error", () => pm.response.to.have.status(500));
pm.test("Error message present", () => {
    const body = pm.response.json();
    pm.expect(body.message || body.title).to.exist;
});
```

---

## Root Cause Note
`LocalFileStorageService` opens a `FileStream` **before** calling `CopyToAsync`. If the copy fails, a zero-byte file may remain. A future improvement could wrap this in a try/catch and delete the partial file:

```csharp
// Suggested improvement to LocalFileStorageService.cs:
var filePath = Path.Combine(uploadsFolder, fileName);
try
{
    using var fileStream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(fileStream, cancellationToken);
}
catch
{
    if (File.Exists(filePath)) File.Delete(filePath); // cleanup partial file
    throw;
}
```

---

## Linked Unit Tests
- `LocalFileStorageServiceTests.SaveFileAsync_WhenDiskWriteFails_ShouldPropagateIOException`

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
