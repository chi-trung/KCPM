# Team Member CI/CD Skill

Tài liệu này là quy trình chuẩn cho thành viên trong nhóm khi làm bài kiểm chứng phần mềm. Mục tiêu là tách rõ người viết test, người sửa code, người review PR, và đồng bộ Jira - GitHub - Postman theo đúng workflow của dự án.

## 1. Mục Tiêu

- Không để member tự test rồi tự fix cùng một vòng.
- Mỗi thay đổi đều có dấu vết Jira key.
- GitHub Actions là nơi kiểm tra tự động.
- Jira là nơi ghi nhận trạng thái task.
- Test phải đi theo thứ tự:
  - Unit testing
  - Integration testing
  - System testing
  - Acceptance testing

## 2. Vai Trò Trong Nhóm

### Member Test

- Nhận task từ nhóm trưởng hoặc Jira.
- Viết test case cho một module cụ thể.
- Ghi log hoặc cập nhật Jira sau khi test.
- Nếu test fail, ghi rõ lỗi và bằng chứng.
- Không tự sửa bug nghiệp vụ nếu vai trò là người test.

### Member Fix / Implement

- Nhận log test hoặc issue từ Jira.
- Sửa code theo đúng defect hoặc yêu cầu.
- Commit lên GitHub bằng Jira key.
- Không sửa test theo kiểu né lỗi; test phải phản ánh hành vi thật.

### Nhóm Trưởng

- Phân task trên Jira.
- Review pull request.
- Chỉ merge khi test và evidence đã đủ.

## 3. Luồng Làm Việc Chuẩn

### Bước 1: Nhận Task

1. Jira gán task cho member.
2. Member tạo branch mới có Jira key.
3. Branch phải rõ ràng, ví dụ: `feature/KIEM-4-auth-tests`.

### Bước 2: Viết Test

1. Member test viết test case cho module được giao.
2. Nếu là backend thì ưu tiên unit test hoặc integration test.
3. Nếu là API flow thì thêm Postman collection hoặc smoke request.
4. Frontend hiện chưa phải phạm vi chính của nhóm trong giai đoạn này.

### Bước 3: Ghi Evidence

1. Cập nhật log Jira hoặc file report test.
2. Ghi rõ trạng thái: pass, fail, blocked.
3. Đính kèm link branch, commit, PR, artifact nếu có.
4. Nếu phát hiện lỗi, tạo subtask hoặc linked issue từ task cha để theo dõi vòng fix tiếp theo.

### Bước 3.1: Quy Tắc Khi Test Fail

1. Member 1 chỉ ghi nhận lỗi và evidence, không tự sửa logic trong cùng một vòng.
2. Tạo subtask hoặc linked issue nhỏ cho defect vừa phát hiện.
3. Giao subtask đó cho Member 2 để sửa đúng phần code nghiệp vụ.
4. Sau khi Member 2 fix xong thì quay lại vòng test để xác nhận lại.

### Bước 4: Push Code / Test Artifacts

1. Commit phải có Jira key.
2. Push lên branch cá nhân.
3. GitHub Actions sẽ chạy backend test và Postman smoke test.
4. Jira có thể tự chuyển sang In Progress khi push thành công.

### Bước 4.1: Báo Cáo Hàng Tuần

1. Mỗi tuần tạo 1 report riêng cho bài kiểm chứng, tổng cộng 8 report cho 8 tuần.
2. Report phải dựa trên kết quả test thật, log thật, hoặc artifact thật từ CI.
3. Nếu dùng Allure thì chỉ dùng như lớp trình bày kết quả test tự động, không thay cho evidence gốc.
4. Mỗi report nên chốt rõ: đã test gì, pass/fail ra sao, defect nào đã sinh subtask, và tuần sau xử lý gì.

### Bước 5: PR Và Review

1. Member tạo Pull Request.
2. PR title phải có Jira key.
3. Nhóm trưởng review.
4. Khi PR đã merged và workflow pass, Jira mới chuyển sang Done.

## 4. Test Pyramid Cần Theo

### Unit Testing

- Kiểm tra từng hàm, class, handler, service riêng lẻ.
- Mục tiêu là bắt lỗi logic sớm.

### Integration Testing

- Kiểm tra nhiều thành phần làm việc cùng nhau.
- Ví dụ: controller + service + database hoặc API + repository.

### System Testing

- Kiểm tra toàn bộ hệ thống như một luồng hoàn chỉnh.
- Ví dụ: backend chạy trong Docker, API gọi thật, dữ liệu đi qua nhiều layer.

### Acceptance Testing

- Kiểm tra theo góc nhìn người dùng hoặc tiêu chí nhận bài.
- Ví dụ: Postman collection, UI flow, hoặc checklist theo Jira.

## 5. Công Cụ Hiện Tại Trong Dự Án

- Backend test CI: `.github/workflows/backend-tests.yml`
- Jira key enforcement: `.github/workflows/jira-key-enforcement.yml`
- Jira issue import: `.github/workflows/create-jira-issues.yml`
- Deploy server: `.github/workflows/deploy-server.yml`
- Postman smoke + Jira sync: `.github/workflows/postman-smoke.yml`
- Postman collection: `Waste-Recycling-Platform/postman/WastePlatform.professional.postman_collection.json`
- Postman environment: `Waste-Recycling-Platform/postman/WastePlatform.professional.postman_environment.json`

## 6. Lưu Ý Quan Trọng

- Tránh member tự test rồi tự fix trong cùng một vòng nếu thầy yêu cầu tách vai trò.
- Nếu member test phát hiện lỗi, ghi log và chuyển cho member fix.
- Nếu member fix xong, quay lại vòng test tiếp theo.
- Không để Jira Done khi mới chỉ pass một phần kiểm tra mà chưa merged đúng quy trình.

## 6.1 Tự Sinh Issue Key Khi Có Lỗi

Khi một test case fail, ưu tiên dùng Jira automation để sinh subtask hoặc linked issue mới thay vì tạo tay từng issue.

- Task cha giữ vai trò gốc của tuần hoặc module.
- Subtask mới giữ vai trò defect cụ thể.
- Mỗi defect có key riêng để member fix làm việc và trace dễ hơn.
- Nếu project có rule automation, có thể cấu hình tự tạo key khi issue được chuyển sang trạng thái lỗi hoặc defect.

## 7. Frontend Testing

Hiện tại frontend chưa nằm trong phạm vi chính của workflow tuần này.

- Nhóm đang ưu tiên backend, Postman, Jira automation.
- Nếu sau này cần mở rộng frontend, có thể bổ sung Codecept hoặc công cụ test UI khác.
- Trước mắt không cần tốn token hay thời gian vào frontend test.

## 8. Khi Trình Bày Với Thầy

Bạn có thể nói:

> “Nhóm em đang chia quy trình theo vai trò. Member test viết test case và ghi evidence, member fix xử lý lỗi dựa trên log, nhóm trưởng review PR. CI chạy backend test, Postman smoke test và đồng bộ Jira. Em cũng đang chuẩn hóa luồng theo test pyramid: unit, integration, system, acceptance.”
