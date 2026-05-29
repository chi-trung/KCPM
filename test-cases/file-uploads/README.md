# File Uploads & Storage — Test Case Overview

**Jira:** [KIEM-20](https://ut-team-36.atlassian.net/browse/KIEM-20)  
**Mã công việc:** `WRP-BE-TESTS-017`  
**Owner:** Nguyễn Minh Phụng  
**Module:** `LocalFileStorageService` (Infrastructure) + Upload endpoints (API)

---

## Scope

Kiểm thử toàn bộ luồng tải ảnh trong hệ thống:
- **Ảnh hiện trường** — Citizen upload khi tạo `WasteReport` (`POST /api/reports`)
- **Ảnh xác thực thu gom** — Collector upload khi hoàn thành task (`PUT /api/collector/tasks/{id}/complete`)

Storage hiện tại: **Local filesystem** (`LocalFileStorageService`), có thể thay bằng S3/Cloudinary qua `IFileStorageService`.

---

## Test Cases

### Group A — LocalFileStorageService (Citizen Report Upload)

| TC ID | Tên | Loại | Severity | Trạng thái |
|-------|-----|------|----------|------------|
| [TC-UPLOAD-001](TC-UPLOAD-001.md) | Upload valid image (.jpg/.png/.gif) | Positive | Critical | ✅ Done |
| [TC-UPLOAD-002](TC-UPLOAD-002.md) | Reject empty / null file | Negative | Critical | ✅ Done |
| [TC-UPLOAD-003](TC-UPLOAD-003.md) | Reject invalid extension (.exe/.pdf/.js) | Negative | Critical | ✅ Done |
| [TC-UPLOAD-004](TC-UPLOAD-004.md) | Reject oversized file (>5MB) / Accept at-limit | Negative + Boundary | Critical | ✅ Done |
| [TC-UPLOAD-005](TC-UPLOAD-005.md) | Folder auto-create & unique GUID filenames | Edge Case | Normal | ✅ Done |
| [TC-UPLOAD-006](TC-UPLOAD-006.md) | IO exception propagated on disk write failure | Abnormal / Negative | Critical | ✅ Done |
| [TC-UPLOAD-007](TC-UPLOAD-007.md) | Minimum 1-byte file accepted (lower boundary) | Boundary | Normal | ✅ Done |

### Group B — Collector Evidence Upload (SRS §3.5)

| TC ID | Tên | Loại | Severity | Trạng thái |
|-------|-----|------|----------|------------|
| [TC-UPLOAD-008](TC-UPLOAD-008.md) | Invalid extension rejected for evidence upload | Negative | Critical | ✅ Done |
| [TC-UPLOAD-008b](TC-UPLOAD-008.md) | **[SRS GAP]** No evidence image → should be 400 | Negative | Critical | ⚠️ Gap Found |
| [TC-UPLOAD-009](TC-UPLOAD-009.md) | Empty evidence file rejected | Negative | Critical | ✅ Done |
| [TC-UPLOAD-010](TC-UPLOAD-010.md) | GUID filename & concurrent upload safety | Positive + Edge | Normal | ✅ Done |

---

## ⚠️ SRS Gap — TC-UPLOAD-008b

> **SRS §3.5 (Page 39):** *"Bắt buộc phải tải lên ít nhất 1 Ảnh xác thực khi nhấn Hoàn thành."*

**Hiện tại:** `CollectorTaskController.CompleteTask` xử lý `Images` là **tùy chọn** — task hoàn thành dù không có ảnh.

**Khuyến nghị fix:**
```csharp
// CollectorTaskController.cs — thêm trước task.Complete():
var images = form.Files.GetFiles("Images");
if (images == null || images.Count == 0)
    return BadRequest(new { message = "At least one evidence image is required." });
```

---

## Unit Test Coverage

### File 1: `LocalFileStorageServiceTests.cs`
**Total:** 16 tests — all passing ✅

| Test Method | TC |
|-------------|----|
| `SaveFileAsync_WithValidJpgFile_ShouldSaveAndReturnFileName` | TC-UPLOAD-001 |
| `SaveFileAsync_WithAllowedExtensions_ShouldSaveSuccessfully` (Theory ×3) | TC-UPLOAD-001 |
| `SaveFileAsync_WithEmptyFile_ShouldThrowArgumentException` | TC-UPLOAD-002 |
| `SaveFileAsync_WithNullFile_ShouldThrowArgumentException` | TC-UPLOAD-002 |
| `SaveFileAsync_WithInvalidExtension_ShouldThrowInvalidOperationException` (Theory ×4) | TC-UPLOAD-003 |
| `SaveFileAsync_WithOversizedFile_ShouldThrowInvalidOperationException` | TC-UPLOAD-004 |
| `SaveFileAsync_WithFileSizeAtLimit_ShouldSaveSuccessfully` | TC-UPLOAD-004 |
| `SaveFileAsync_WhenUploadsFolderMissing_ShouldCreateFolderAndSave` | TC-UPLOAD-005 |
| `SaveFileAsync_CalledTwiceWithSameName_ShouldReturnDistinctFileNames` | TC-UPLOAD-005 |
| `SaveFileAsync_WhenDiskWriteFails_ShouldPropagateIOException` | **TC-UPLOAD-006** ✨ |
| `SaveFileAsync_WithMinimumOneByte_ShouldSaveSuccessfully` | **TC-UPLOAD-007** ✨ |

### File 2: `CollectorEvidenceUploadTests.cs` ✨ NEW
**Total:** 7 tests

| Test Method | TC |
|-------------|----|
| `CollectorUpload_WithInvalidExtension_ShouldThrowInvalidOperationException` (Theory ×3) | TC-UPLOAD-008 |
| `CollectorCompleteTask_WithoutImages_CurrentBehaviourIsPermissive` | **TC-UPLOAD-008b (Gap)** |
| `CollectorUpload_WithEmptyFile_ShouldThrowArgumentException` | TC-UPLOAD-009 |
| `CollectorUpload_WithValidJpg_ShouldSaveAndReturnGuidFilename` | TC-UPLOAD-010 |
| `CollectorUpload_WithAllowedExtensions_ShouldSaveSuccessfully` (Theory ×3) | TC-UPLOAD-010 |
| `CollectorUpload_ConcurrentUploads_ShouldProduceDistinctFilenames` | TC-UPLOAD-010b |

---

## Run Unit Tests

```bash
# Tất cả File Upload tests
dotnet test --filter "FullyQualifiedName~LocalFileStorageServiceTests|FullyQualifiedName~CollectorEvidenceUploadTests"

# Chỉ LocalFileStorageService tests
dotnet test --filter "FullyQualifiedName~LocalFileStorageServiceTests"

# Chỉ Collector evidence tests
dotnet test --filter "FullyQualifiedName~CollectorEvidenceUploadTests"
```

---

## Postman API Tests

Import collection và chạy với Newman:

```bash
newman run postman/collections/FileUpload.postman_collection.json \
  -e postman/environments/local.postman_environment.json
```

> **Lưu ý:** Các Postman test dùng `form-data` với file thật. Cần chuẩn bị file mẫu trong `test-assets/images/`.

---

## Boundary Analysis Summary

```
File Size:
  0 bytes      → ArgumentException "File is empty"        [TC-002]
  1 byte       → ✅ Accepted (lower boundary)              [TC-007] ✨
  5,242,880 B  → ✅ Accepted (upper boundary = 5 MB)       [TC-004]
  5,242,881 B  → InvalidOperationException "exceeds limit" [TC-004]

File Extension:
  .jpg .jpeg .png .gif  → ✅ Accepted                     [TC-001]
  .exe .js .pdf .csv    → InvalidOperationException        [TC-003]
  .svg .exe .pdf        → InvalidOperationException        [TC-008] ✨
```

---

**Last updated:** 2026-05-28
