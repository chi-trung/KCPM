# TC-UPLOAD-010 — Collector Evidence: GUID Filename & Concurrent Upload Safety

**Module:** File Uploads & Storage — Collector Evidence  
**Jira:** [KIEM-20](https://ut-team-36.atlassian.net/browse/KIEM-20)  
**Type:** Positive + Edge Case  
**Severity:** Normal  
**Owner:** Nguyễn Minh Phụng

---

## Objective
Verify that:
1. Valid collector evidence images (`.jpg`, `.jpeg`, `.png`, `.gif`) are saved successfully.
2. Each upload generates a unique GUID-based filename, preventing overwrite between concurrent collector uploads.
3. The GUID name is verifiable via `Guid.TryParse`.

---

## Pre-conditions
- `LocalFileStorageService` configured with writable `ContentRootPath`.
- Allowed extensions: `.jpg`, `.jpeg`, `.png`, `.gif`.

---

## Test Steps

### Unit Test — Valid Upload
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` with `evidence.jpg` (non-empty, valid) | Returns `{guid}.jpg` |
| 2 | Parse filename stem with `Guid.TryParse` | Parses successfully → `true` |
| 3 | Check file exists at `uploads/{guid}.jpg` | File present on disk |

### Unit Test — All Allowed Extensions
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Upload `cam1.jpeg` | Saved, result ends with `.jpeg` |
| 2 | Upload `scene.png` | Saved, result ends with `.png` |
| 3 | Upload `clip.gif` | Saved, result ends with `.gif` |

### Unit Test — Concurrent Upload Safety
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` twice with identical name `evidence.jpg` | Returns two distinct filenames |
| 2 | Both files exist on disk | Two separate files, no overwrite |

### Postman (API) — Multiple Evidence Images
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `PUT /api/collector/tasks/{id}/complete` with `WeightKg=2.0` and 2 evidence `.jpg` files | `200 OK`, task `Collected` |
| 2 | `GET /api/collector/tasks/{id}` | Response `images` array has 2 distinct URLs |
| 3 | Confirm no two URLs are identical | All image URLs unique |

---

## Postman Test Script
```javascript
pm.test("Status 200", () => pm.response.to.have.status(200));
pm.test("Task completed", () => {
    const body = pm.response.json();
    pm.expect(body.taskId).to.be.a("string");
});

// Follow-up GET request to verify image URLs
pm.test("Evidence images stored", () => {
    const task = pm.response.json();           // from GET /tasks/{id}
    pm.expect(task.images).to.be.an("array").with.length.above(0);
    const urls = task.images;
    const uniqueUrls = new Set(urls);
    pm.expect(uniqueUrls.size).to.equal(urls.length, "All image URLs must be unique");
    urls.forEach(url => {
        pm.expect(url).to.match(/\.(jpg|jpeg|png|gif)$/i);
    });
});
```

---

## Linked Unit Tests
- `CollectorEvidenceUploadTests.CollectorUpload_WithValidJpg_ShouldSaveAndReturnGuidFilename`
- `CollectorEvidenceUploadTests.CollectorUpload_WithAllowedExtensions_ShouldSaveSuccessfully`
- `CollectorEvidenceUploadTests.CollectorUpload_ConcurrentUploads_ShouldProduceDistinctFilenames`

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
