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
2. Mở source code hoặc Postman lên, bắt đầu làm bài.
3. Với môn kiểm chứng phần mềm, ưu tiên tạo hoặc cập nhật test case đúng task được giao trên nhánh cá nhân đó, sau đó mới commit và push.

### BƯỚC 3: Nộp Bài
Làm xong, bạn lưu file lại và đẩy code lên mạng (Commit & Push lên nhánh RIÊNG của bạn):
```bash
git add .
git commit -m "KIEM-4: Hoàn thành task phân quyền"
git push
```
*(Lưu ý: Luôn nhớ bắt đầu lời nhắn commit bằng mã Jira ID như KIEM-4, KIEM-5...)*

### BƯỚC 4: Chờ Hệ Thống Tự Động Duyệt & Nhóm Trưởng Merge
Ngay khi bạn Push code lên nhánh cá nhân, hệ thống Automation (GitHub Actions) sẽ chạy kiểm tra và cập nhật Jira theo đúng luồng sau:

| Sự kiện | Workflow chính | Jira sẽ được làm gì |
|---------|----------------|---------------------|
| Push code lên nhánh cá nhân | `postman-smoke.yml` | Tự động comment kết quả và thử chuyển thẻ sang **In Progress** nếu Jira có transition phù hợp |
| PR đã được merge và workflow PASS | `postman-smoke.yml` | Tự động comment kết quả và thử chuyển thẻ sang **Done** nếu Jira có transition phù hợp |
| Workflow FAIL | `postman-smoke.yml` | Tự động comment cảnh báo **FAIL** để bạn sửa rồi `git push` lại |

Lưu ý:
- `backend-tests.yml` chỉ chạy test .NET, không phải workflow chuyển trạng thái Jira.
- `postman-smoke.yml` là workflow đang chịu trách nhiệm comment và chuyển trạng thái Jira khi push hoặc khi PR pass.
- Nếu Jira của nhóm dùng tên transition khác, hệ thống sẽ tự chọn transition gần đúng nhất khi có thể.
- Nhóm Trưởng vẫn là người review và merge PR vào `main` sau khi mọi thứ pass.
- Workflow hiện tại chỉ chuyển sang **Done** khi PR đã merge xong; PR mở hoặc PR chưa merge sẽ không kéo thẻ sang Done.

### Luồng Chuẩn Mong Muốn Của Nhóm
1. Member nhận task trên Jira và tạo branch mới từ Jira key.
2. Member viết code hoặc test case trên branch đó, commit có Jira key và push lên GitHub.
3. GitHub Actions chạy `postman-smoke.yml` để kiểm tra và comment lên Jira.
4. Nếu push pass, Jira chuyển sang **In Progress**.
5. Member tạo PR để trưởng nhóm review.
6. Nếu PR pass, Jira chuyển sang **Done**.
7. Trưởng nhóm merge PR sau khi đã review xong và mọi kiểm tra đều đạt.

---
*Chúc cả nhóm làm việc hiệu quả và đạt điểm A+!*
