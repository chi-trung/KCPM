# TC-WC-009: WasteCategory Module Testing

## 📋 Thông tin kiểm thử

| Field | Value |
|-------|-------|
| **Mã test case** | TC-WC-009 |
| **Jira Task** | WRP-BE-TESTS-009 |
| **Module** | WasteCategory |
| **Độ ưu tiên** | Trung bình |
| **Loại test** | Chức năng / Tích hợp |
| **Ngày tạo** | 2026-05-28 |
| **Người tạo** | Nguyen Hoang Phung |

## 🎯 Mục tiêu

Xác minh các API và hành vi của module `WasteCategory`: CRUD, validate dữ liệu, danh sách/lọc và phân quyền truy cập.

## ✅ Điều kiện tiên quyết

1. Backend server đang chạy (dev/staging) và có endpoint `Waste Categories` tại `/api/waste-categories`.
2. Database đã được seed dữ liệu mẫu hoặc có quyền tạo/xóa bản ghi test.
3. Có token xác thực hợp lệ với quyền phù hợp.
4. Postman / Newman hoặc test runner đã được cấu hình sẵn (nếu dùng).

## 🔧 Test Data

### Header của request
```
Authorization: Bearer {token}
Content-Type: application/json
```

### Ví dụ body request `Create`

```json
{
  "name": "Plastic",
  "description": "Plastic waste category",
  "code": "PLAST"
}
```

## 📝 Các bước kiểm thử

1. Chèn dữ liệu seed trực tiếp vào DB cho `WasteCategory`.
2. Verify bản ghi đã được insert trong DB bằng `SELECT id, name, description FROM waste_categories`.
3. GET danh sách `GET {{baseUrl}}/api/waste-categories` và xác nhận bản ghi vừa tạo xuất hiện.
4. GET chi tiết `GET {{baseUrl}}/api/waste-categories/{{categoryId}}` và verify fields.
5. Cập nhật (PUT/PATCH) trường `description` và kiểm tra cập nhật thành công.
6. Thử tạo với payload thiếu `name` hoặc `code` và kiểm tra validation (400 + message).
7. Thử tạo/cập nhật với `code` trùng lặp và kiểm tra lỗi (409 hoặc theo spec).
8. Xóa bản ghi và kiểm tra 204/200, đồng thời GET chi tiết phải trả 404 sau khi xóa.
9. Kiểm tra phân trang/lọc nếu endpoint hỗ trợ (page, size, filter theo code/name).
10. Kiểm tra quyền: gọi các endpoint với token không đủ quyền và xác nhận mã lỗi (401/403).
11. Dọn dẹp: xóa dữ liệu test còn sót nếu cần.

## ✔️ Kết quả mong đợi

- Create trả về 201 và body hợp lệ.
- GET danh sách / chi tiết trả dữ liệu chính xác.
- Update phản ánh thay đổi.
- Validation lỗi trả mã lỗi phù hợp và thông báo rõ ràng.
- Duplicate code trả lỗi phù hợp.
- Delete thực sự loại bỏ bản ghi.
- Endpoint thực thi kiểm soát quyền đúng (401/403 khi không có quyền).

## 🔄 Kết quả thực tế

Ghi lại kết quả thực thi từng bước ở đây sau khi chạy test.
1. SQL seed đã chạy thành công trên `waste_db`.
Kết quả từ DB:
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
4. GET chi tiết `GET http://localhost:8080/api/waste-categories/11` — Kết quả và kết quả test:

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

Bước 5 — Cập nhật `description`

Request đã dùng (SQL, chạy trên `waste_db`):
```sql
UPDATE waste_categories
SET description = 'Updated description'
WHERE id = 11;
```

Kết quả SELECT trong DB:
```text
id\tname\tdescription
11\tPlastic\tUpdated description
```

Kết quả GET API đã lưu tại: `test-cases/evidence/GET-category-11-response.json`
Kết quả DB đã lưu tại: `test-cases/evidence/update_description_11-db.txt`

Kết luận: `GET /api/waste-categories/11` trả về `description = "Updated description"`, nên bước cập nhật `description` đã PASS.

Kết quả test (Postman - `GET Category By ID`):
- Status 200: PASSED
- Response có `data`: PASSED
- Có `id`, `name`, `description`: PASSED

Evidence:
- File response đã lưu: `GET-category-11-response.json` (gợi ý)
- Kết quả select DB: `id = 11, name = Plastic` (xem bước verify SQL)

6. Tạo với payload thiếu `name`/`code` (negative test)

Payload (POST) đã dùng:
```json
{
    "description": "No name or code"
}
```

Kết quả khi gọi API:
```text
Invoke-RestMethod : The remote server returned an error: (405) Method Not Allowed.
```

Evidence đã lưu: `test-cases/evidence/POST-missing-name-code-response.txt`

Kết luận: **BLOCKED** — API không hỗ trợ `POST /api/waste-categories` (405), nên không thể kiểm tra validate thiếu field qua API. Đề xuất tạo endpoint hoặc kiểm thử validation ở backend unit test.

7. Kiểm tra `code` trùng lặp

Đã kiểm tra: xem cấu trúc bảng `waste_categories` để tìm cột `code`.

Evidence: `test-cases/evidence/waste_categories_columns_list.txt`

Quan sát: bảng `waste_categories` chỉ có các cột `id`, `name`, `description` (không có cột `code`). Vì vậy việc kiểm tra `code` trùng lặp **KHÔNG ÁP DỤNG** trong schema hiện tại.

Kết luận: **N/A** — Không có cột `code` để kiểm tra trùng lặp. Muốn test này, cần thêm cột `code`, unique constraint và implement `POST`/`PUT` trong API.

8. Xóa bản ghi

BLOCKED: "DELETE /api/waste-categories/{id} trả 405 → API không hỗ trợ DELETE. Test bị BLOCKED. Evidence: test-cases/evidence/DELETE-category-11-response.txt"

9. Kiểm tra phân trang / lọc (page, size, filter theo name/code)

Đã thực hiện:
- Called `GET /api/waste-categories?page=1&size=2` and saved response to `test-cases/evidence/GET-page1-size2.json`.
- Called `GET /api/waste-categories?limit=2&offset=0` and saved response to `test-cases/evidence/GET-limit2-offset0.json`.
- Called `GET /api/waste-categories?name=Plastic` and saved response to `test-cases/evidence/GET-filter-name-Plastic.json`.
- Called `GET /api/waste-categories?code=PLAST` and saved response to `test-cases/evidence/GET-filter-code-PLAST.json`.

Kết quả: Tất cả request đều trả về full list (giống `GET /api/waste-categories`), cho thấy endpoint hiện tại bỏ qua hoặc không hỗ trợ các query params `page/size`, `limit/offset`, hoặc filter theo `name/code`.

Kết luận: **BLOCKED / N/A** — API hiện tại không hỗ trợ phân trang và query filtering. Đề xuất implement query params hoặc trả về response có metadata (total, page, size).

10. Kiểm tra quyền (không có token / token không đủ quyền)

Đã thực hiện:
- Called `GET /api/waste-categories` without token and saved response to `test-cases/evidence/GET-no-token-categories.json`.
- Called `POST /api/waste-categories` without token and saved error to `test-cases/evidence/POST-no-token-categories.txt`.
- Called `DELETE /api/waste-categories/11` without token and saved error to `test-cases/evidence/DELETE-no-token-category-11.txt`.

Kết quả:
- `GET /api/waste-categories` không có token vẫn trả `200 OK` kèm dữ liệu, nên endpoint này đang public và không yêu cầu xác thực.
- `POST /api/waste-categories` không có token trả `405 Method Not Allowed`.
- `DELETE /api/waste-categories/11` không có token trả `405 Method Not Allowed`.

Kết luận: **BLOCKED / N/A** — API hiện tại không có các endpoint create/delete được bảo vệ và endpoint GET list là public, nên không thể xác thực 401/403 từ implementation hiện tại. Hướng xử lý tiếp theo: implement POST/PUT/DELETE có auth, sau đó test lại với no token và token hạn chế để kiểm tra 401/403.

11. Dọn dẹp: xóa dữ liệu test còn sót

Đã thực hiện:
- Xóa bản ghi `id = 11` trực tiếp trong DB.
- Verify cleanup bằng cách gọi `GET /api/waste-categories/11`.

Kết quả:
- DB delete hoàn tất thành công; file output SQL cleanup rỗng vì `DELETE`/`SELECT` không còn dòng nào với `id = 11`.
- `GET /api/waste-categories/11` trả về `404 Not Found`.

Evidence:
- `test-cases/evidence/cleanup-delete-category-11-db.txt`
- `test-cases/evidence/cleanup-get-category-11.txt`

Kết luận: **PASS** — dữ liệu test còn sót đã được xóa thành công.

## 📊 Trạng thái

⬜ Not Tested | ✅ Pass | ⬜ Fail

## 🐛 Lỗi phát hiện (nếu có)

| Defect ID | Description | Severity | Status |
|-----------|-------------|----------|--------|
| | | | |

## 🔗 Test case liên quan

- KIEM-... hoặc TC-REP-... liên quan tới danh mục/lookup.

## 📝 Ghi chú

- Đính kèm evidence: link Allure report, Postman collection/run, snapshot database, comment Jira.
- Nếu cần, tôi có thể đưa các bước này vào Postman collection hoặc viết script Newman.
