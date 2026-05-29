# TC-REP-010: Health Endpoint Trả Về 200 OK

## 📋 Thông tin kiểm thử

| Trường | Giá trị |
|-------|-------|
| **Mã test case** | TC-REP-010 |
| **Jira Task** | N/A |
| **Module** | Health / Smoke |
| **Độ ưu tiên** | Cao |
| **Loại test** | Positive / Smoke |
| **Ngày tạo** | 2026-05-29 |
| **Người tạo** | Nguyen Hoang Phung |

## 🎯 Mục tiêu

Xác minh rằng `GET /api/health` phản hồi `200 OK` và trả về payload hợp lệ để dùng cho smoke testing.

## ✅ Điều kiện tiên quyết

1. Backend server đang chạy.
2. `baseUrl` đã được cấu hình đúng trong Postman hoặc Newman.
3. Không cần token xác thực.

## 🔧 Dữ liệu kiểm thử

### Header của request

```http
Accept: application/json
```

### Request Body

Không có.

## 📝 Các bước kiểm thử

1. Gửi yêu cầu `GET {{baseUrl}}/api/health`.
2. Kiểm tra status code của response.
3. Kiểm tra response body.
4. Xác nhận trường `status` có giá trị `ok`.

## ✔️ Kết quả mong đợi

- Status code của response là `200`.
- Response body là JSON hợp lệ.
- Response body chứa:

```json
{
  "status": "ok"
}
```

## 🔄 Kết quả thực tế

Sau khi gửi yêu cầu, API trả về:

```json
{
  "status": "ok"
}
```

Status code của response là `200 OK`.

## 📊 Trạng thái

✅ Đạt

## 🐛 Defects (nếu có)

| Mã lỗi | Mô tả | Mức độ | Trạng thái |
|-----------|-------------|----------|--------|
| N/A | Không phát hiện lỗi | N/A | N/A |

## 📝 Ghi chú

- Endpoint này là public.
- Phù hợp để kiểm tra smoke trong CI/CD.