KIEM-21: Security & Role-based Access Tests — Báo cáo công việc

Ngày: 2026-06-05
Người thực hiện: Hoàng Phụng

**Tóm tắt công việc**
- Viết unit test cho `JwtService.GenerateToken()` kiểm tra claims (sub, email, role), issuer/audience và expiry.
- Viết integration tests cho endpoint admin (`/api/admin/enterprises`) để kiểm tra tình huống:
  - Không có token => trả về 401 Unauthorized
  - Token của `Citizen` => trả về 403 Forbidden
  - Token của `Admin` => trả về 200 OK (kết quả trả về được mock bằng `IMediator`)
- Cấu hình môi trường test để không phụ thuộc MySQL thực tế:
  - Thêm `ProgramForTests.cs` (public partial Program) để `WebApplicationFactory<Program>` hoạt động.
  - Đăng ký `WastePlatformDbContext` sử dụng `InMemoryDatabase` trong test.
  - Thêm `TestAuthHandler` để đọc claims từ JWT mà không cần xác thực chữ ký (dùng để kiểm tra role-based authorization trong môi trường test).
- Cập nhật metadata Allure theo yêu cầu: `AllureEpic`, `AllureTag`, `AllureOwner("Hoàng Phụng")`.

**Các file đã thêm / sửa**
- Thêm test unit: backend/tests/WastePlatform.Tests/Infrastructure/Services/JwtServiceTests.cs
- Thêm test integration: backend/tests/WastePlatform.Tests/Integration/AdminEnterpriseAuthorizationTests.cs
- Thêm helper cho tests: backend/src/WastePlatform.API/ProgramForTests.cs
- Cập nhật project test: backend/tests/WastePlatform.Tests/WastePlatform.Tests.csproj (thêm `Microsoft.AspNetCore.Mvc.Testing`)
- Thêm báo cáo gốc: backend/tests/TestReports/KIEM-21-Security-Role-based-Tests-Report.md
- Bản sao hiện tại (nơi này): reports/KIEM-21-Security-Role-based-Tests-Report.md

**Tên test quan trọng**
- `JwtServiceTests.GenerateToken_ShouldContainExpectedClaimsAndExpiry`
- `AdminEnterpriseAuthorizationTests.GetEnterprises_WithoutToken_ReturnsUnauthorized`
- `AdminEnterpriseAuthorizationTests.GetEnterprises_WithCitizenToken_ReturnsForbidden`
- `AdminEnterpriseAuthorizationTests.GetEnterprises_WithAdminToken_ReturnsOk`

**Các lệnh đã chạy và kết quả**
- Chạy test cụ thể:
```bash
dotnet test "backend/tests/WastePlatform.Tests/WastePlatform.Tests.csproj" --filter "FullyQualifiedName~WastePlatform.Tests.Infrastructure.Services.JwtServiceTests" -v minimal
```
- Chạy integration tests:
```bash
dotnet test "backend/tests/WastePlatform.Tests/WastePlatform.Tests.csproj" --filter "FullyQualifiedName~WastePlatform.Tests.Integration.AdminEnterpriseAuthorizationTests" -v minimal
```
- Kết quả: các test liên quan (4 tests) đều PASS trên môi trường dev local.

**Ghi chú kỹ thuật / Giải thích**
- `TestAuthHandler`:
  - Đọc token JWT từ header `Authorization: Bearer <token>` và chuyển claim vào `ClaimsPrincipal` mà không kiểm tra chữ ký.
  - Điều này cho phép kiểm tra logic `Authorize(Roles = "Admin")` dựa trên claim `role` mà không cần có secret key hợp lệ trong test host.
- In-memory DB:
  - Đã thay thế `WastePlatformDbContext` bằng `UseInMemoryDatabase` trong `ConfigureTestServices` để tránh kết nối MySQL thật.
- Mocking MediatR:
  - `IMediator` được mock để trả về dữ liệu giả cho `GetEnterprisesQuery`, tránh phụ thuộc DB và business logic khác.
- Allure metadata:
  - Mọi test đã được gắn `AllureEpic("KIEM-21: Security & Role-based Access Tests")`, `AllureTag` (link ticket) và `AllureOwner("Hoàng Phụng")`.

**Hạn chế / Điểm cần lưu ý**
- `TestAuthHandler` bỏ qua việc xác thực chữ ký JWT — chỉ dùng trong môi trường test. Không sử dụng cách này trong môi trường staging/production.
- Chưa có test cho các trường hợp: token hết hạn (expired), token malformed (invalid format) — có thể thêm bằng cách tạo token có thời hạn ngắn hoặc chuỗi không phải JWT.
- Test cho "revoked/disabled user" yêu cầu seed user vào InMemory DB và đảm bảo `ValidateUserStatusMiddleware` kiểm tra `IsActive` -> tôi có thể thêm test seed user và verify response 401.

**Đề xuất bước tiếp theo**
1. Thêm tests cho:
   - Expired token (tạo JWT với `ExpirationMinutes= -1` hoặc manipulate `exp` claim).
   - Malformed token.
   - Revoked user: seed một user trong InMemory DB với `IsActive=false` và gửi token tương ứng — mong muốn trả 401.
2. Tạo PR với tiêu đề: `test: add KIEM-21 security & role-based access tests` và mô tả ngắn (liệt kê file thay đổi + cách chạy tests).
3. Nếu muốn, tôi có thể push và tạo PR giúp bạn.

**Thông tin liên hệ / owner**
- Allure owner: Hoàng Phụng

---
Nếu bạn muốn, tôi sẽ:
- (A) Commit & push các thay đổi lên remote và tạo PR; hoặc
- (B) Thêm test cho token expired / revoked user như mô tả; hoặc
- (C) Áp metadata Allure cho toàn bộ thư mục test (nếu cần).

Chọn A / B / C hoặc cho tôi hướng khác.