# Hướng Dẫn Làm Bài & Nộp Bài Tự Động (Dành Cho Thành Viên)

Chào các bạn, hệ thống chấm điểm và kiểm chứng tự động (CI/CD) của nhóm đã được kích hoạt. Thay vì làm thủ công, mọi người chỉ cần làm theo đúng 4 bước cực kỳ đơn giản dưới đây để hệ thống tự động ghi nhận điểm và kéo thẻ trên Jira.

> [!WARNING]
> **QUY TẮC CỐT LÕI CỦA NHÓM (BẮT BUỘC ĐỌC):**
> 1. **Tuyệt đối không đẩy thẳng (push trực tiếp) vào nhánh `main`.** Mọi người chỉ được phép push lên nhánh cá nhân của mình. Việc gộp code (Merge) sẽ do **Nhóm Trưởng** trực tiếp review và quyết định.
> 2. **AI KHÔNG LÀM / KHÔNG CÓ LỊCH SỬ COMMIT = 0 ĐIỂM.** Thầy giáo sẽ chấm điểm dựa trên lịch sử commit của nhánh cá nhân và tiến độ trên thẻ Jira của từng người. Không có ngoại lệ.

---

### BƯỚC 1: Nhận Việc & Tạo Nhánh (Branch)
1. Đăng nhập vào **Jira** của nhóm, xem bạn được Assign (phân công) những Task nào.
2. Mở thẻ Task đó lên (Ví dụ: `KIEM-4`), bấm vào nút **"Create Branch"** ngay trên Jira.
3. Jira sẽ tự động tạo một nhánh mới trên GitHub với tên chuẩn xác (Ví dụ: `KIEM-4-wrp-be-tests-001`).

### BƯỚC 2: Tải Code & Làm Bài
1. Mở Terminal / VS Code ở máy bạn và tải nhánh mới về:
   ```bash
   git fetch
   git checkout <tên-nhánh-vừa-tạo>
   ```
2. Mở source code hoặc Postman lên, bắt đầu làm bài (viết code Backend để fix lỗi, hoặc viết thêm các kịch bản Test theo yêu cầu của Task).

### BƯỚC 3: Nộp Bài
Làm xong, bạn lưu file lại và đẩy code lên mạng (Commit & Push lên nhánh RIÊNG của bạn):
```bash
git add .
git commit -m "KIEM-4: Hoàn thành task phân quyền"
git push
```
*(Lưu ý: Luôn nhớ bắt đầu lời nhắn commit bằng mã Jira ID như KIEM-4, KIEM-5...)*

### BƯỚC 4: Chờ Hệ Thống Tự Động Duyệt & Nhóm Trưởng Merge
Ngay khi bạn Push code lên nhánh cá nhân, hệ thống Automation (GitHub Actions) sẽ tự động chạy test để "chấm điểm":
- 🟢 **Nếu PASS:** Hệ thống tự động báo Pass và kéo thẻ Jira của bạn sang cột **Done**. Bạn chỉ việc chờ Nhóm Trưởng review và Merge code của bạn vào `main`.
- 🔴 **Nếu FAIL:** Hệ thống sẽ để lại comment cảnh báo "FAIL" trên thẻ Jira của bạn. Nhóm Trưởng sẽ KHÔNG merge. Bạn phải quay lại màn hình code, sửa lỗi, rồi lại `git commit` và `git push` cho đến khi nào hệ thống báo Pass thì thôi!

---
*Chúc cả nhóm làm việc hiệu quả và đạt điểm A+!*
