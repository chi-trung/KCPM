# TC-UPLOAD-008 — Collector Evidence Upload: Invalid Extension Rejected / SRS Gap Documented

**Module:** File Uploads & Storage — Collector Evidence  
**Jira:** [KIEM-20](https://ut-team-36.atlassian.net/browse/KIEM-20)  
**Type:** Negative + SRS Gap  
**Severity:** Critical  
**Owner:** Nguyễn Minh Phụng

---

## Objective

### TC-UPLOAD-008: Extension Validation
Verify that the storage service rejects disallowed extensions (`.exe`, `.pdf`, `.svg`) for collector evidence photos — same rules as citizen report images.

### TC-UPLOAD-008b: SRS Gap — No Image Required (⚠️ DEFECT)
Document the gap between SRS §3.5 and implementation:

> **SRS §3.5 (Page 39):** *"Bắt buộc phải nhập Khối lượng rác (Weight > 0) và tải lên ít nhất 1 Ảnh xác thực khi nhấn 'Hoàn thành'."*

**Current implementation** in `CollectorTaskController.CompleteTask` (line 256):
```csharp
var images = form.Files.GetFiles("Images");
if (images != null && images.Count > 0)   // ← Images are OPTIONAL — gap!
{
    // process images...
}
```
The task completes successfully even with **zero evidence images**. This violates the SRS.

---

## Pre-conditions
- For TC-008: `LocalFileStorageService` configured, file has non-zero content.
- For TC-008b: `PUT /api/collector/tasks/{id}/complete` endpoint running, task in `OnTheWay` status.

---

## Test Steps

### TC-UPLOAD-008 — Unit Test
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` with `evidence.exe` | Throws `InvalidOperationException("Invalid file type: .exe")` |
| 2 | Call `SaveFileAsync` with `photo.pdf` | Throws `InvalidOperationException("Invalid file type: .pdf")` |
| 3 | Call `SaveFileAsync` with `data.svg` | Throws `InvalidOperationException("Invalid file type: .svg")` |

### TC-UPLOAD-008b — Postman API (Documents the Gap)
| Step | Action | Expected (SRS) | Actual (Current) |
|------|--------|----------------|------------------|
| 1 | `PUT /api/collector/tasks/{id}/complete` with `WeightKg=2.5`, **no Images** | `400 Bad Request` — "At least one evidence image is required" | `200 OK` — task marked Collected ❌ |
| 2 | `PUT /api/collector/tasks/{id}/complete` with valid `.jpg` image | `200 OK` | `200 OK` ✅ |

---

## Postman Test Script (TC-UPLOAD-008b documenting gap)
```javascript
// This test currently FAILS (returns 200 instead of 400)
// It serves as a regression test for when the gap is fixed
pm.test("[PENDING FIX] 400 when no evidence image", () => {
    pm.expect(pm.response.code).to.equal(400);
});
pm.test("[PENDING FIX] Error message mentions image", () => {
    pm.expect(pm.response.text()).to.include("evidence image");
});
```

---

## Recommended Fix (CollectorTaskController.cs)

```csharp
// After parsing WeightKg, before task.Complete():
var images = form.Files.GetFiles("Images");
if (images == null || images.Count == 0)
    return BadRequest(new { message = "At least one evidence image is required." }); // ← ADD THIS
```

---

## Linked Unit Tests
- `CollectorEvidenceUploadTests.CollectorUpload_WithInvalidExtension_ShouldThrowInvalidOperationException`
- `CollectorEvidenceUploadTests.CollectorCompleteTask_WithoutImages_CurrentBehaviourIsPermissive`

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
