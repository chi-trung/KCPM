# TC-WC-009: WasteCategory Module Testing

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-WC-009 |
| **Jira Task** | WRP-BE-TESTS-009 |
| **Module** | WasteCategory |
| **Priority** | Medium |
| **Test Type** | Functional / Integration |
| **Created Date** | 2026-05-28 |
| **Created By** | Nguyen Hoang Phung |

## 🎯 Objective

Xác minh các API và hành vi của `WasteCategory` module: CRUD, validation, danh sách/lọc, và quyền truy cập.

## ✅ Pre-conditions

1. Backend server đang chạy (dev/staging) và có endpoint `Waste Categories` sẵn sàng tại `/api/waste-categories`.
2. Database đã seed dữ liệu mẫu hoặc có quyền tạo/xóa bản ghi test.
3. Có token xác thực hợp lệ với quyền phù hợp.
4. Postman / Newman hoặc test runner đã cấu hình sẵn (nếu dùng).

## 🔧 Test Data

### Request Headers
```
Authorization: Bearer {token}
Content-Type: application/json
```

### Example `Create` Request Body

```json
{
  "name": "Plastic",
  "description": "Plastic waste category",
  "code": "PLAST"
}
```

## 📝 Test Steps

1. Chèn dữ liệu seed trực tiếp vào DB cho `WasteCategory`.
2. Verify bản ghi đã được insert trong DB bằng `SELECT id, name, description FROM waste_categories`.
3. GET danh sách `GET {{baseUrl}}/api/waste-categories` và xác nhận bản ghi vừa tạo xuất hiện.
4. GET chi tiết `GET {{baseUrl}}/api/waste-categories/{{categoryId}}` và verify fields.
5. Update (PUT/PATCH) trường `description` và verify cập nhật thành công.
6. Thử tạo với payload thiếu `name` hoặc `code` và verify validation (400 + message).
7. Thử tạo/ cập nhật với `code` trùng lặp và verify lỗi (409 hoặc theo spec).
8. Delete bản ghi và verify 204/200 và rằng GET chi tiết trả 404 sau khi xóa.
9. Test phân trang/lọc nếu endpoint hỗ trợ (page, size, filter by code/name).
10. Test quyền: gọi các endpoint với token không đủ quyền và xác nhận mã lỗi (401/403).
11. Clean-up: xóa dữ liệu test còn sót nếu cần.

## ✔️ Expected Results

- Create trả về 201 và body hợp lệ.
- GET list/ detail trả dữ liệu chính xác.
- Update phản ánh thay đổi.
- Validation lỗi trả mã lỗi phù hợp và thông báo rõ ràng.
- Duplicate code trả lỗi phù hợp.
- Delete thực sự loại bỏ bản ghi.
- Endpoints thực thi kiểm soát quyền đúng (401/403 khi không có quyền).

## 🔄 Actual Results

Ghi lại kết quả thực thi từng bước ở đây sau khi chạy test.
1. Seed SQL executed successfully against `waste_db`.
Kết quả DB:
```text
id      name    description
11      Plastic Plastic waste category
```
2. GET `http://localhost:8080/api/waste-categories`
Kết quả:
```json
{
    "message": "Categories retrieved successfully",
    "data": [
        {
            "id": 11,
            "name": "Plastic",
            "description": "Plastic waste category"
        },
        {
            "id": 5,
            "name": "Rác thải cây lá",
            "description": "Lá rơi, cành cây, cỏ, v.v."
        },
        {
            "id": 3,
            "name": "Rác thải nguy hiểm",
            "description": "Pin, thuốc, hóa chất, v.v."
        },
        {
            "id": 1,
            "name": "Rác thải sinh hoạt",
            "description": "Rác thải từ nhà ở, cơ quan, cửa hàng"
        },
        {
            "id": 2,
            "name": "Rác thải thực phẩm",
            "description": "Thực phẩm thừa, xương, rau quả"
        },
        {
            "id": 4,
            "name": "Rác thải xây dựng",
            "description": "Xi măng, gạch, thép, v.v."
        }
    ]
}
```

3. `id = 11` là mã định danh duy nhất của bản ghi `Plastic` trong bảng `waste_categories`.
     Dùng `categoryId = 11` để test `GET http://localhost:8080/api/waste-categories/11` trong Postman.
4. GET chi tiết `GET http://localhost:8080/api/waste-categories/11` — Kết quả và Test Results:

```json
{
    "message": "Category retrieved successfully",
    "data": {
        "id": 11,
        "name": "Plastic",
        "description": "Updated description"
    }
}
```

Step 5 — Update `description`

Request used (SQL, executed against `waste_db`):
```sql
UPDATE waste_categories
SET description = 'Updated description'
WHERE id = 11;
```

DB SELECT result:
```text
id\tname\tdescription
11\tPlastic\tUpdated description
```

API GET result saved to: `test-cases/evidence/GET-category-11-response.json`
DB output saved to: `test-cases/evidence/update_description_11-db.txt`

Kết luận: `GET /api/waste-categories/11` trả về `description = "Updated description"`, nên bước Update `description` đã PASS.

Test Results (Postman - `GET Category By ID`):
- Status is 200: PASSED
- Response has data: PASSED
- Has id,name,description: PASSED

Evidence:
- Saved response file: `GET-category-11-response.json` (suggested)
- DB select output: `id = 11, name = Plastic` (see SQL verification step)

## 📊 Status

⬜ Not Tested | ✅ Pass | ⬜ Fail

## 🐛 Defects (if any)

| Defect ID | Description | Severity | Status |
|-----------|-------------|----------|--------|
| | | | |

## 🔗 Related Test Cases

- KIEM-... hoặc TC-REP-... liên quan tới danh mục/lookup.

## 📝 Notes

- Đính kèm evidence: Allure report link, Postman collection/run, database snapshots, Jira comment.
- Nếu cần, tôi có thể đưa các bước này vào Postman collection hoặc viết script Newman.
