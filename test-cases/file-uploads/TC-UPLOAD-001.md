# TC-UPLOAD-001 — Upload Valid Image File

**Module:** File Uploads & Storage  
**Jira:** [KIEM-20](https://ut-team-36.atlassian.net/browse/KIEM-20)  
**Type:** Positive  
**Severity:** Critical  
**Owner:** Nguyễn Minh Phụng

---

## Objective
Verify that valid image files (`.jpg`, `.jpeg`, `.png`, `.gif`) are accepted, saved to the `uploads/` folder, and a unique filename is returned.

---

## Pre-conditions
- `LocalFileStorageService` is configured with a writable `ContentRootPath`.
- File size is within the 5 MB limit.

---

## Test Steps

### Unit Test
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` with a `.jpg` file of valid size | Returns non-empty filename ending in `.jpg` |
| 2 | Check file exists at `uploads/<returnedName>` | File is present on disk |
| 3 | Repeat with `.jpeg`, `.png`, `.gif` | Each saved successfully with correct extension |

### Postman (API)
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `POST /api/reports` with `form-data`, field `Images` = valid `.jpg` file | `200 OK`, response contains report `id` |
| 2 | `GET /api/reports/{id}` | Report has non-empty `imageUrls` array |
| 3 | `PUT /api/collector/tasks/{id}/complete` with `form-data`, `Images` = valid `.png` | `200 OK`, task status = `Collected` |

---

## Postman Test Script
```javascript
pm.test("Status is 200", () => pm.response.to.have.status(200));
pm.test("Report ID returned", () => {
    const id = pm.response.json();
    pm.expect(id).to.be.a('string').and.have.lengthOf.above(0);
    pm.environment.set("reportId", id);
});
```

---

## Linked Unit Tests
- `LocalFileStorageServiceTests.SaveFileAsync_WithValidJpgFile_ShouldSaveAndReturnFileName`
- `LocalFileStorageServiceTests.SaveFileAsync_WithAllowedExtensions_ShouldSaveSuccessfully`

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
