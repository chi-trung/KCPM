# 🔔 WRP-BE-TESTS-003: Kiểm thử mô-đun thông báo

**Trạng thái:** 🟦 IN PROGRESS  
**Nhánh:** `WRP-BE-TESTS-003-Notifications-Module`  
**Liên kết Jira:** WRP-BE-TESTS-003  
**Mô-đun:** Notifications (Thông báo trong ứng dụng cho Citizen)

---

## 📋 Tổng quan test case

| TC ID | Tên kịch bản kiểm thử | Loại | Trạng thái | Độ ưu tiên |
|:---:|:---|:---:|:---:|:---:|
| **TC-NOTIF-001** | Lấy danh sách thông báo (token hợp lệ) | ✅ Positive | ⬜ TBD | 🔴 High |
| **TC-NOTIF-002** | Lấy danh sách thông báo không có token | ❌ Negative | ⬜ TBD | 🔴 High |
| **TC-NOTIF-003** | Lấy số lượng chưa đọc | ✅ Positive | ⬜ TBD | 🟡 Medium |
| **TC-NOTIF-004** | Đánh dấu một thông báo là đã đọc | ✅ Positive | ⬜ TBD | 🟡 Medium |
| **TC-NOTIF-005** | Đánh dấu tất cả thông báo là đã đọc | ✅ Positive | ⬜ TBD | 🟡 Medium |
| **TC-NOTIF-006** | Notification ID không hợp lệ | ❌ Negative | ⬜ TBD | 🔴 High |

---

## 🎯 Mục tiêu kiểm thử

- ✅ Xác nhận citizen lấy được danh sách thông báo của chính mình khi có JWT hợp lệ
- ✅ Xác nhận các endpoint thông báo bắt buộc xác thực
- ✅ Xác nhận unread count trả về đúng theo dữ liệu thực tế
- ✅ Xác nhận có thể đánh dấu một thông báo là đã đọc
- ✅ Xác nhận có thể đánh dấu tất cả thông báo chưa đọc là đã đọc
- ✅ Xác nhận cách hệ thống xử lý notification ID không tồn tại/không hợp lệ

---

## 📌 Danh sách API kiểm thử

- `GET /api/notifications`
- `GET /api/notifications/unread-count`
- `PUT /api/notifications/{id}/read`
- `PUT /api/notifications/mark-all-read`

**Phân quyền:** yêu cầu role `Citizen`

---

## 📝 Chi tiết test case

### TC-NOTIF-001: Lấy danh sách thông báo (token hợp lệ) ✅ (Positive)

**Mục tiêu:** Xác nhận citizen có thể lấy danh sách thông báo phân trang khi token hợp lệ.

**Điều kiện tiên quyết:**
- Citizen đã đăng nhập và có JWT hợp lệ
- Database có ít nhất 1 thông báo thuộc citizen đó
- Backend đang chạy

**Dữ liệu test:**
- Header Authorization: `Bearer {{citizenToken}}`
- Query params: `page=1&pageSize=20`

**Các bước thực hiện:**
```
1. Gửi request GET /api/notifications?page=1&pageSize=20
2. Gắn Authorization header với citizen token hợp lệ
3. Kiểm tra body response và trường phân trang
```

**Kết quả mong đợi:**
- ✅ HTTP status: `200 OK`
- ✅ Response message: `Notifications retrieved successfully`
- ✅ Response có `data` (danh sách)
- ✅ Response có `unreadCount`
- ✅ Response có `pagination.page`, `pagination.pageSize`, `pagination.total`, `pagination.totalPages`
- ✅ Mỗi phần tử thông báo có: `id`, `type`, `channel`, `status`, `title`, `message`, `actionUrl`, `relatedEntityId`, `relatedEntityType`, `createdAt`, `readAt`
- ✅ Chỉ trả về thông báo thuộc citizen hiện tại

**Vị trí evidence:** `postman-results/results.json` → `TC-NOTIF-001`

**Actual result (run):**

```
{
    "message": "Notifications retrieved successfully",
    "data": [
        {
            "id": "7f067a74-5528-11f1-80c2-0242ac120002",
            "type": "ReportCreated",
            "channel": "InApp",
            "status": "Unread",
            "title": "Test notification",
            "message": "Seeded for testing by QA",
            "actionUrl": null,
            "relatedEntityId": null,
            "relatedEntityType": null,
            "createdAt": "2026-05-21T15:19:44Z",
            "readAt": null
        },
        {
            "id": "3c6f1b2a-8f4d-4d2a-9c2b-7e6a5f4d1c2b",
            "type": "ReportCreated",
            "channel": "InApp",
            "status": "Unread",
            "title": "Test notification (fixed id)",
            "message": "Seeded for testing by QA (fixed id)",
            "actionUrl": null,
            "relatedEntityId": null,
            "relatedEntityType": null,
            "createdAt": "2026-05-21T15:19:44Z",
            "readAt": null
        }
    ],
    "unreadCount": 2,
    "pagination": {
        "page": 1,
        "pageSize": 20,
        "total": 2,
        "totalPages": 1
    }
}
```

**Status:** FAIL

**Notes:** Response returned an empty `data` array and `unreadCount: 0` — prerequisite "Database có ít nhất 1 thông báo" not met. Recommend seeding test notifications for the citizen and re-running TC-NOTIF-001.

---

### TC-NOTIF-002: Lấy danh sách thông báo không có token ❌ (Negative)

**Mục tiêu:** Xác nhận endpoint từ chối request chưa xác thực.

**Điều kiện tiên quyết:**
- Không gửi JWT token
- Backend đang chạy

**Dữ liệu test:**
- Không có Authorization header

**Các bước thực hiện:**
```
1. Gửi request GET /api/notifications
2. Không thêm Authorization header
3. Kiểm tra status code và response body
```

**Kết quả mong đợi:**
- ✅ HTTP status: `401 Unauthorized`
- ✅ Response body thể hiện thiếu xác thực
- ✅ Không trả dữ liệu thông báo

**Vị trí evidence:** `postman-results/results.json` → `TC-NOTIF-002`

---

### TC-NOTIF-003: Lấy số lượng chưa đọc ✅ (Positive)

**Mục tiêu:** Xác nhận unread count trả về đúng cho citizen hiện tại.

**Điều kiện tiên quyết:**
- Citizen đã đăng nhập và có JWT hợp lệ
- Citizen có thông báo chưa đọc trong database

**Dữ liệu test:**
- Header Authorization: `Bearer {{citizenToken}}`

**Các bước thực hiện:**
```
1. Gửi request GET /api/notifications/unread-count
2. Gắn Authorization header với citizen token hợp lệ
3. So sánh số lượng trả về với dữ liệu DB
```

**Kết quả mong đợi:**
- ✅ HTTP status: `200 OK`
- ✅ Response message: `Unread count retrieved successfully`
- ✅ Response có trường `unreadCount`
- ✅ Giá trị `unreadCount` khớp dữ liệu chưa đọc của citizen hiện tại

**Vị trí evidence:** `postman-results/results.json` → `TC-NOTIF-003`

**Actual result (run):**

```
{
  "message": "Unread count retrieved successfully",
  "unreadCount": 2
}
```

**Status:** FAIL

**Notes:** `unreadCount` is `0` — prerequisite "Citizen có thông báo chưa đọc" not met. Recommend seeding at least one unread notification for the test citizen and re-running TC-NOTIF-003.

---

### TC-NOTIF-004: Đánh dấu một thông báo là đã đọc ✅ (Positive)

**Mục tiêu:** Xác nhận citizen có thể đánh dấu 1 thông báo thành đã đọc.

**Điều kiện tiên quyết:**
- Citizen đã đăng nhập và có JWT hợp lệ
- Thông báo tồn tại và thuộc citizen hiện tại
- Trạng thái thông báo ban đầu là `Unread`

**Dữ liệu test:**
- Header Authorization: `Bearer {{citizenToken}}`
- Path param: `{{notificationId}}`

**Các bước thực hiện:**
```
1. Gửi request PUT /api/notifications/{notificationId}/read
2. Gắn Authorization header với citizen token hợp lệ
3. Kiểm tra trạng thái thông báo sau khi cập nhật
```

**Kết quả mong đợi:**
- ✅ HTTP status: `200 OK`
- ✅ Response message: `Notification marked as read`
- ✅ Trạng thái thông báo đổi từ `Unread` sang `Read`
- ✅ Trường `ReadAt` được gán timestamp
- ✅ Unread count giảm 1 (nếu notification đã chọn là unread)

**Vị trí evidence:** `postman-results/results.json` → `TC-NOTIF-004`
{
    "message": "Notifications marked as read"
}
---

### TC-NOTIF-005: Đánh dấu tất cả thông báo là đã đọc ✅ (Positive)

**Mục tiêu:** Xác nhận citizen có thể đánh dấu toàn bộ thông báo chưa đọc thành đã đọc.

**Điều kiện tiên quyết:**
- Citizen đã đăng nhập và có JWT hợp lệ
- Citizen có nhiều thông báo unread

**Dữ liệu test:**
- Header Authorization: `Bearer {{citizenToken}}`

**Các bước thực hiện:**
```
1. Gửi request PUT /api/notifications/mark-all-read
2. Gắn Authorization header với citizen token hợp lệ
3. Kiểm tra toàn bộ thông báo unread sau khi cập nhật
```

**Kết quả mong đợi:**
- ✅ HTTP status: `200 OK`
- ✅ Response message: `All notifications marked as read`
- ✅ Tất cả thông báo unread của citizen hiện tại thành `Read`
- ✅ Các bản ghi được cập nhật có `ReadAt`
- ✅ Unread count về `0`

**Vị trí evidence:** `postman-results/results.json` → `TC-NOTIF-005`

---
{
    "message": "All notifications marked as read"
}

### TC-NOTIF-006: Notification ID không hợp lệ ❌ (Negative)

**Mục tiêu:** Xác nhận hệ thống xử lý ID không tồn tại hoặc không hợp lệ khi đánh dấu đã đọc.

**Điều kiện tiên quyết:**
- Citizen đã đăng nhập và có JWT hợp lệ
- Sử dụng GUID đúng format nhưng không tồn tại trong database

**Dữ liệu test:**
- Header Authorization: `Bearer {{citizenToken}}`
- Path param: `00000000-0000-0000-0000-000000000000`

**Các bước thực hiện:**
```
1. Gửi request PUT /api/notifications/00000000-0000-0000-0000-000000000000/read
2. Gắn Authorization header với citizen token hợp lệ
3. Kiểm tra response và dữ liệu DB
```

**Kết quả mong đợi (theo QA):**
- ✅ Nên trả `404 Not Found` hoặc lỗi validation rõ ràng cho ID không tồn tại
- ✅ Không thay đổi bất kỳ thông báo nào khác
- ✅ Unread count giữ nguyên

**Ghi chú hành vi thực tế của API hiện tại:**
- Implement hiện tại trả `200 OK` với message `Notification marked as read` ngay cả khi ID không tồn tại vì repository đang no-op nếu không tìm thấy record.
- Nếu team mong muốn strict validation thì ghi nhận là defect.

**Vị trí evidence:** `postman-results/results.json` → `TC-NOTIF-006`
{
    "message": "Notification not found"
}

---

## 🔎 Ghi chú xác minh

- Controller hiện tại giới hạn role `Citizen` qua `[Authorize(Roles = "Citizen")]`.
- `GET /api/notifications` hỗ trợ filter `status` với giá trị `Unread` hoặc `Read`.
- `PUT /api/notifications/{id}/read` chưa kiểm tra quyền sở hữu hoặc sự tồn tại trước khi trả success.
- `PUT /api/notifications/mark-all-read` chỉ cập nhật thông báo unread của citizen hiện tại.

---

## 📁 Gợi ý cấu trúc Postman

- `03 - Notifications`
  - `GET List Notifications`
  - `GET Unread Count`
  - `PUT Mark Notification As Read`
  - `PUT Mark All As Read`
  - `NEG Invalid Notification ID`

---

## ✅ Tiêu chí hoàn thành

- Tài liệu có đủ 6 test case với bước chạy và kỳ vọng rõ ràng.
- Luồng positive bám theo hành vi thực tế của controller/repository.
- Case invalid ID nêu rõ chênh lệch giữa kỳ vọng QA và hành vi implement.
- Có thể chạy trực tiếp bằng Postman và lưu evidence vào `postman-results/results.json`.

---

## ✅ Checklist chạy nhanh

### A. Sẵn sàng môi trường

- [ ] Backend API chạy ổn định
- [ ] Database chạy và có dữ liệu test
- [ ] Đã import Postman collection
- [ ] Đã chọn đúng Postman environment
- [ ] Đăng nhập được user Citizen để lấy token
- [ ] Biến môi trường `citizenToken` đã được set

### B. Chuẩn bị dữ liệu thông báo

- [ ] Citizen có ít nhất 2 thông báo unread
- [ ] Đã lấy một `notificationId` hợp lệ từ response danh sách
- [ ] Đã ghi lại unread count ban đầu (`beforeUnreadCount`)

### C. Chạy test case

- [ ] TC-NOTIF-001: Lấy danh sách thông báo (token hợp lệ)
- [ ] TC-NOTIF-002: Lấy danh sách thông báo không token
- [ ] TC-NOTIF-003: Lấy số lượng chưa đọc
- [ ] TC-NOTIF-004: Đánh dấu một thông báo đã đọc
- [ ] TC-NOTIF-005: Đánh dấu tất cả thông báo đã đọc
- [ ] TC-NOTIF-006: Notification ID không hợp lệ

### D. Kiểm tra toàn vẹn dữ liệu

- [ ] Unread count giảm sau TC-NOTIF-004
- [ ] Unread count về 0 sau TC-NOTIF-005
- [ ] `ReadAt` được cập nhật cho các bản ghi đã đọc
- [ ] Không lộ dữ liệu thông báo giữa các user

### E. Evidence và báo cáo

- [ ] Lưu ảnh/snapshot request-response cho từng TC
- [ ] Ghi pass/fail và actual result cho từng case
- [ ] Lưu evidence vào `postman-results/results.json`
- [ ] Tạo bug cho hành vi lệch kỳ vọng (đặc biệt TC-NOTIF-006)

---

## ▶️ Hướng dẫn chạy test từng bước

### Bước 1: Đăng nhập và set token

1. Chạy request login trong folder Auth.
2. Copy access token nhận được.
3. Set biến môi trường `citizenToken`.
4. (Tùy chọn) Gọi thử 1 endpoint protected để kiểm tra token còn hiệu lực.

### Bước 2: Lấy mốc unread ban đầu

1. Gọi `GET /api/notifications/unread-count`.
2. Lưu giá trị này thành `beforeUnreadCount`.

### Bước 3: Chạy TC-NOTIF-001

1. Gọi `GET /api/notifications?page=1&pageSize=20` với bearer token.
2. Xác nhận `200` và có `data`, `unreadCount`, `pagination`.
3. Lấy một `notificationId` đang unread để dùng cho TC-004.

### Bước 4: Chạy TC-NOTIF-002

1. Tắt Authorization header.
2. Gọi `GET /api/notifications`.
3. Xác nhận `401 Unauthorized`.

### Bước 5: Chạy TC-NOTIF-003

1. Bật lại Authorization header.
2. Gọi `GET /api/notifications/unread-count`.
3. Xác nhận số lượng hợp lý với dữ liệu hiện tại.

### Bước 6: Chạy TC-NOTIF-004

1. Gọi `PUT /api/notifications/{notificationId}/read`.
2. Xác nhận `200` và message thành công.
3. Gọi lại unread-count để xác nhận giảm 1 (nếu notification ban đầu là unread).

### Bước 7: Chạy TC-NOTIF-005

1. Gọi `PUT /api/notifications/mark-all-read`.
2. Xác nhận `200`.
3. Gọi lại unread-count và xác nhận giá trị bằng `0`.

### Bước 8: Chạy TC-NOTIF-006

1. Gọi `PUT /api/notifications/00000000-0000-0000-0000-000000000000/read`.
2. Ghi nhận actual response thực tế.
3. Đánh giá kết quả theo rule của team:
- Team yêu cầu strict validation: tạo defect (do actual thường là `200`).
- Team chấp nhận idempotent behavior: có thể pass kèm note implement.

### Bước 9: Hoàn tất và cập nhật

1. Điền actual result cho cả 6 case.
2. Đính kèm screenshot/response logs.
3. Cập nhật trạng thái Jira và link evidence.
