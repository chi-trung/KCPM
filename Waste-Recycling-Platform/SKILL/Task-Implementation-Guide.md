# Hướng dẫn quy trình hoàn thành một Jira Task (Dành cho Member)

Tài liệu này hướng dẫn các thành viên trong nhóm cách thực hiện một task từ lúc nhận việc trên Jira cho đến khi gộp code thành công, đảm bảo không bị conflict và báo cáo Allure Report luôn tự động lấy được dữ liệu.

## Bước 1: Đồng bộ Code mới nhất
Trước khi bắt đầu bất cứ task nào, bạn **BẮT BUỘC** phải cập nhật code mới nhất từ nhánh `main` để tránh bị lỗi Conflict sau này.
```bash
git checkout main
git pull origin main
```

## Bước 2: Tạo nhánh (Branch) làm việc riêng
Tuyệt đối không code trực tiếp trên nhánh `main`. Mỗi task phải có một nhánh riêng.
Cú pháp tên nhánh: `<MÃ-TASK>-<Tên-Task-Viết-Liền-Không-Dấu>`
Ví dụ:
```bash
git checkout -b KIEM-20-Thong-Ke-Bao-Cao
```

## Bước 3: Implement Code (Thực hiện chức năng)
- Viết code cho Controller, Service, Repository tại thư mục `backend/src/...`
- Đảm bảo code tuân thủ các quy chuẩn thiết kế của dự án.

## Bước 4: Viết Unit Test (Bắt buộc)
Để báo cáo Allure Report tự động ghi nhận công sức của bạn, bạn **phải viết Unit Test** cho chức năng mình vừa làm.
- Vị trí viết Test: `backend/tests/WastePlatform.Tests/Controllers/`
- **Lưu ý quan trọng**: Tên Class Test hoặc Tên Hàm Test của bạn nên chứa từ khóa liên quan đến Task để sau này script có thể gom nhóm chính xác. 
- Mẫu tên hàm test chuẩn: `TenHam_DieuKien_KetQuaMongDoi` (Ví dụ: `Register_WithValidData_ShouldReturnOk`)

## Bước 5: Chạy Test kiểm tra kết quả
Để chắc chắn code bạn viết ra không làm hỏng code của người khác:
```bash
# Trực tiếp chạy test trên toàn bộ project
dotnet test ./backend/tests/WastePlatform.Tests/WastePlatform.Tests.csproj
```
*(Nếu tất cả báo `Passed` màu xanh lá thì chúc mừng, bạn đã làm xuất sắc!)*

## Bước 6: Đẩy Code và Tạo Pull Request (PR)
Khi đã test OK, bạn tiến hành nộp bài lên GitHub:
```bash
git add .
git commit -m "KIEM-20: Hoàn thành chức năng Thống kê báo cáo"
git push origin KIEM-20-Thong-Ke-Bao-Cao
```
- Lên trang chủ GitHub của project, bấm nút **Compare & pull request**.
- Viết mô tả rõ ràng bạn đã làm những gì, test những gì.
- Chờ Reviewer (Leader) duyệt và Merge vào `main`.

---
> **Lưu ý:** Chỉ khi code của bạn được Merge vào nhánh `main`, thì các hệ thống tự động xuất báo cáo Allure chung của nhóm mới lấy được các test case của bạn để đưa vào bảng thống kê nhé! Chúc các bạn code vui vẻ! 🚀
