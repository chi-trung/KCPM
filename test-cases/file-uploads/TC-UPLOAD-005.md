# TC-UPLOAD-005 — Storage Behavior: Folder Auto-Create & Unique Filenames

**Module:** File Uploads & Storage  
**Jira:** [KIEM-20](KIEM-20)  
**Type:** Positive + Edge Case  
**Severity:** Normal  
**Owner:** Nguyễn Minh Phụng

---

## Objective
Verify that:
1. The `uploads/` folder is automatically created if it does not exist.
2. Every upload generates a unique GUID-based filename, preventing overwrites.

---

## Pre-conditions
- `ContentRootPath` points to a writable temp directory.
- `uploads/` folder does not exist before the first test.

---

## Test Steps

### Unit Test — Folder Auto-Create
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Delete `uploads/` folder if it exists | Folder absent |
| 2 | Call `SaveFileAsync` with valid file | No exception thrown |
| 3 | Check `uploads/` folder exists | Folder created automatically |
| 4 | Check file exists inside folder | File written at `uploads/<guid>.jpg` |

### Unit Test — Unique Filenames
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` twice with files having identical name `photo.jpg` | Returns two distinct filenames |
| 2 | Confirm both files exist on disk | Two separate files in `uploads/` |

### Postman (API) — Multiple Images
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `POST /api/reports` with 3 image files attached | `200 OK`, report `imageUrls` has 3 entries |
| 2 | Confirm no two URLs are identical | All URLs are unique |

---

## Postman Test Script
```javascript
pm.test("Status is 200", () => pm.response.to.have.status(200));
pm.test("Report ID returned", () => {
    const id = pm.response.json();
    pm.expect(id).to.be.a("string");
});
```

---

## Linked Unit Tests
- `LocalFileStorageServiceTests.SaveFileAsync_WhenUploadsFolderMissing_ShouldCreateFolderAndSave`
- `LocalFileStorageServiceTests.SaveFileAsync_CalledTwiceWithSameName_ShouldReturnDistinctFileNames`

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
