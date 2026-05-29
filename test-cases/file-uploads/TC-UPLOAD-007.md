# TC-UPLOAD-007 — Minimum 1-Byte File Accepted (Lower Boundary)

**Module:** File Uploads & Storage  
**Jira:** [KIEM-20](https://ut-team-36.atlassian.net/browse/KIEM-20)  
**Type:** Positive / Boundary  
**Severity:** Normal  
**Owner:** Nguyễn Minh Phụng

---

## Objective
Verify that the smallest possible non-empty file (exactly **1 byte**) is accepted by `SaveFileAsync` and physically written to disk at the correct size.

This completes the boundary triangle alongside TC-UPLOAD-004:

| Boundary | Size | Expected |
|---|---|---|
| Below lower bound | 0 bytes | ❌ Rejected (TC-002) |
| **Lower bound** | **1 byte** | **✅ Accepted (TC-007)** |
| Upper bound | 5,242,880 bytes (5 MB) | ✅ Accepted (TC-004) |
| Above upper bound | 5,242,881 bytes | ❌ Rejected (TC-004) |

---

## Pre-conditions
- `LocalFileStorageService` is configured with a writable `ContentRootPath`.
- Allowed extensions include `.jpg`.
- Max size = 5 MB.

---

## Test Steps

### Unit Test
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call `SaveFileAsync` with a `.jpg` file containing exactly 1 byte (`"x"`) | Returns non-empty filename ending in `.jpg` |
| 2 | Check file exists at `uploads/<returnedName>` | File present on disk |
| 3 | Check file size on disk | `FileInfo.Length == 1` |

---

## Postman (API) — Manual Note
Generating a true 1-byte binary file in Postman is non-trivial. Use `test-assets/images/tiny_1byte.jpg` (a 1-byte placeholder file). The API layer should accept it as long as the file is valid at the storage service level.

---

## Linked Unit Tests
- `LocalFileStorageServiceTests.SaveFileAsync_WithMinimumOneByte_ShouldSaveSuccessfully`

---

**Tested By:** Nguyễn Minh Phụng  **Date:** 2026-05-28
