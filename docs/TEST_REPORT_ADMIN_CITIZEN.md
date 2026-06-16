# TEST REPORT — Admin + Analytics + Citizen Modules

Ngày: 2026-06-16
Người thực hiện: Nguyễn Hoàng Phụng

## 1. Mục tiêu báo cáo
- Tổng hợp kết quả kiểm thử cho 3 module: Admin, Analytics, Citizen.
- Ghi nhận defect phát hiện trong chạy BVA (F11 — Images Upload).
- Đính kèm bằng chứng (Allure results) và link tới mã/tests liên quan.

## 2. Phạm vi kiểm thử
- Admin Module: CRUD user, quản lý roles.
- Analytics Module: dashboard stats, kiểm tra độ chính xác dữ liệu.
- Citizen Module: quản lý profile, lịch sử báo cáo.

## 3. Kỹ thuật áp dụng
- EP (Equivalence Partitioning) cho các trường nhập liệu chính.
- BVA (Boundary Value Analysis) cho các ranh giới (ví dụ: images count 1..5).
- Error Guessing cho các luồng bất thường (missing field, invalid file, v.v.).

## 4. Tổng quan kết quả
- Test suite: F11 (BVA — Images Upload) và các test case chức năng cho 3 module.
- Kết quả (local run): 68 executed, 65 passed, 3 failed (F11 — BVA Images Upload).

Nguồn tham chiếu:
- Traceability & plan: [docs/TRACEABILITY_MATRIX.md](docs/TRACEABILITY_MATRIX.md)
- Test plan: [docs/TEST_PLAN.md](docs/TEST_PLAN.md)

## 5. Defects (Ghi nhận)
1) KIEM-26 — Missing mandatory image validation
   - Mô tả: API/handler không bắt buộc ít nhất 1 ảnh; gửi null/không gửi field vẫn được chấp nhận.
   - Ảnh hưởng: Tính năng upload ảnh có thể nhận báo cáo không hợp lệ (không có ảnh).
   - Priority/Severity: High / Critical (xem [docs/TEST_PLAN.md](docs/TEST_PLAN.md)).
   - Nơi tham chiếu trong repo:
     - Test generator & expectations: generate_unittest_excel.py (BVA mapping KIEM-26)
     - Traceability: [docs/TRACEABILITY_MATRIX.md](docs/TRACEABILITY_MATRIX.md#L109)
     - Test implementation: CreateReportCommandHandlerTests.cs (BVA region)
   - Status: IN PROGRESS (đang xử lý, đã log issue).

2) KIEM-29 — Missing maximum 5 images validation
   - Mô tả: Handler chưa enforce tối đa 5 ảnh; gửi >5 ảnh không bị từ chối.
   - Ảnh hưởng: Có thể gây quá tải lưu trữ / bypass business rule.
   - Priority/Severity: High / High.
   - Nơi tham chiếu trong repo:
     - generate_unittest_excel.py (BVA cases, comments for KIEM-29)
     - CreateReportCommandHandlerTests.cs — có test `Fact(Skip=...)` chỉ rõ KIEM-29 bug
     - docs/TRACEABILITY_MATRIX.md
   - Status: TO DO (test khai báo skip, bug cần fix).

Notes: Tổng cộng có 3 TCs failed trong lần chạy — chủ yếu thuộc F11 (BVA Images Upload). Các failures liên quan tới KIEM-26 và KIEM-29 (các chi tiết test và logs có trong history/chat và Allure results).

## 6. Bằng chứng & Vị trí logs
- Allure results folder: Waste-Recycling-Platform/allure-results
- Liệt kê test liên quan (ví dụ):
  - Waste-Recycling-Platform/backend/tests/.../CreateReportCommandHandlerTests.cs (BVA-F11 block)
  - generate_unittest_excel.py (BVA definitions and expected messages)


