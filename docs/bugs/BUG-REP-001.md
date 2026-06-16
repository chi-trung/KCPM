# BUG-REP-003: Thiếu Ràng Buộc Giới Hạn Tối Đa 5 Hình Ảnh Báo Cáo Rác Tại Tầng Application

**Kịch Bản Liên Quan (Related TC)**: TC-REP-BVA-005 (Kiểm thử ngoài biên số lượng hình ảnh tối đa)  
**Mức Độ Nghiêm Trọng (Severity)**: Trung bình (Medium)  
**Trạng Thái (Status)**: Đã khắc phục (Fixed)  

## Mô Tả Lỗi
Theo tài liệu đặc tả chức năng (SRS) mục **FR-C01 (Trang 18)** của hệ thống *Crowdsourced Waste Collection & Recycling Platform*:
> "Citizen chụp ảnh (1–5 ảnh), chọn loại rác, xác nhận vị trí GPS và gửi báo cáo."

Quy tắc nghiệp vụ này giới hạn nghiêm ngặt số lượng ảnh đính kèm của mỗi báo cáo rác là từ **1 đến tối đa 5 ảnh**.

Tuy nhiên, trong mã nguồn thực tế tại lớp xử lý nghiệp vụ [CreateReportCommand.cs](file:///d:/GitHub/KCPM/Waste-Recycling-Platform/backend/src/WastePlatform.Application/Reports/Commands/CreateReportCommand.cs), hệ thống chỉ thực hiện kiểm tra biên dưới (bắt buộc tải lên tối thiểu 1 hình ảnh):
```csharp
if (request.Images == null || request.Images.Count == 0)
    throw new ArgumentException("At least one image is required");
```
Hệ thống hoàn toàn **bỏ sót** điều kiện chặn biên trên (`Images.Count > 5`). Điều này dẫn đến việc người dân có thể tải lên $6$, $10$ hoặc hàng trăm tệp ảnh trong một báo cáo rác duy nhất, gây phình to dung lượng đĩa lưu trữ, tăng tải cho dịch vụ File Storage, và trực tiếp vi phạm thiết kế nghiệp vụ của hệ thống.

## Các Bước Tái Hiện
1. Mở file test [CreateReportCommandHandlerTests.cs](file:///d:/GitHub/KCPM/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Application/Reports/CreateReportCommandHandlerTests.cs).
2. Tìm đến test case biên `Handle_WithSixImages_ShouldThrowArgumentException` vừa được bổ sung để kiểm tra việc gửi 6 file ảnh.
3. Chạy kiểm thử bằng lệnh:
   ```bash
   dotnet test --filter "FullyQualifiedName~Handle_WithSixImages_ShouldThrowArgumentException"
   ```
4. Quan sát kết quả thất bại:
   ```text
   Assert.Throws() Failure: No exception was thrown
   Expected: typeof(System.ArgumentException)
   ```

## Kỳ Vọng và Thực Tế (Expected vs Actual)
* **Kỳ vọng**: Khi số lượng tệp ảnh đính kèm lớn hơn 5, Command Handler phải lập tức ngăn chặn và ném ra lỗi `ArgumentException` với thông điệp `"Maximum 5 images are allowed"`.
* **Thực tế**: Command Handler bỏ qua kiểm tra này, xử lý lưu trữ thành công và trả về mã ID báo cáo rác rỗng mà không gặp bất kỳ lỗi nào.

## Kế Hoạch Khắc Phục
1. Bổ sung đoạn code kiểm tra biên trên của thuộc tính `request.Images` trong phương thức `Handle` của [CreateReportCommand.cs](file:///d:/GitHub/KCPM/Waste-Recycling-Platform/backend/src/WastePlatform.Application/Reports/Commands/CreateReportCommand.cs):
   ```csharp
   if (request.Images.Count > 5)
       throw new ArgumentException("Maximum 5 images are allowed");
   ```
2. Thực thi lại bộ kiểm thử xUnit để đảm bảo lỗi được khắc phục triệt để và test chuyển sang màu xanh (Passed).
