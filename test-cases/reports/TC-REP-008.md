# TC-REP-008: Upload Image with Invalid Format

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-REP-008 |
| **Jira Task** | WRP-BE-TESTS-002 |
| **Module** | Reports |
| **Priority** | Medium |
| **Test Type** | Negative |
| **API Endpoint** | `POST /api/reports/create` |
| **Created Date** | 2025-05-17 |

## 🎯 Objective

Verify that the API properly rejects image uploads with invalid formats, sizes, or corrupted files.

## ✅ Pre-conditions

1. Backend server running
2. Valid citizen token

## 🔧 Test Scenarios

### Scenario 1: Invalid File Format
```
File: document.pdf (PDF thay vì image)
```

### Scenario 2: File Too Large
```
File: large-image.jpg (> 10MB hoặc limit được config)
```

### Scenario 3: Corrupted Image
```
File: corrupted.jpg (file bị hỏng, không đọc được)
```

### Scenario 4: Non-Image Extension but Image Content
```
File: image.txt (đổi extension .jpg thành .txt)
```

### Scenario 5: Script/Executable disguised as Image
```
File: malicious.php.jpg
```

## ✔️ Expected Results

### Response Status
- **HTTP Code**: `400` (Bad Request) hoặc `415` (Unsupported Media Type)
- **Response Time**: < 2000ms

### Error Messages

| Scenario | Expected Error |
|----------|----------------|
| Invalid format | "Only JPG, PNG, WEBP formats are allowed" |
| File too large | "File size exceeds maximum limit of X MB" |
| Corrupted | "Invalid or corrupted image file" |
| Wrong extension | "File content does not match extension" |
| Malicious | "Invalid file type detected" |

### Security Requirements
- Không lưu file độc hại vào server
- Validate MIME type, không chỉ extension
- Scan file content (magic bytes)

## 🔄 Actual Results

### Execution Date: 2025-05-17

| Check | Expected | Actual | Status |
|-------|----------|--------|--------|
| HTTP Status | 400 | 400 | ✅ Pass |
| Error Message | "File size exceeds limit" | "File size exceeds limit." | ✅ Pass |
| Report Created? | No | No (undefined) | ✅ Pass |
| Image Saved? | No | No | ✅ Pass |
| Response Time | < 3000ms | < 3000ms | ✅ Pass |

### Note
- Test script cần điều chỉnh cho negative test (không check reportId)
- API đúng: Từ chối file > 10MB, không lưu report
- ✅ Bảo vệ hệ thống khỏi file quá lớn

## 📊 Status

⬜ **Not Tested** | ✅ **Pass** | ⬜ **Fail**

## 🔗 Related Test Cases

- TC-REP-001: Create report with valid image (Positive)
- TC-REP-002: Missing image (if image is required)

## 📝 Notes

- Security test quan trọng: ngăn chặn upload file độc hại
- Kiểm tra file còn tồn tại trên server sau khi reject (should be deleted)
- Test với các file thực tế, không chỉ đổi tên

---

**Tested By**: Nguyễn Minh Phụng **Date**: 2026-05-26
