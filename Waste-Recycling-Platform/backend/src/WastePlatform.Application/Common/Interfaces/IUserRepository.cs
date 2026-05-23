using WastePlatform.Domain.Entities;

namespace WastePlatform.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        // Lấy danh sách user với search và filter
        Task<List<User>> GetUsersAsync(string? search, string? role, CancellationToken cancellationToken);

        // Đếm tổng số user
        Task<int> GetTotalCountAsync(CancellationToken ct);

        // 1. Thêm người dùng
        Task<string> CreateUserAsync(string email, string passwordHash, string fullName, string phone, string role, string district, string ward, CancellationToken ct);
        
        // 2. Khóa / Mở khóa
        Task<bool> ToggleUserStatusAsync(string userId, CancellationToken ct);
        
        // 3. Sửa Quyền
        Task<bool> UpdateUserRoleAsync(string userId, string newRole, CancellationToken ct);
        
        // 4. Lấy thông tin user theo ID
        Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct);
        
        // 5. Cập nhật thông tin profile
        Task<User> UpdateProfileAsync(Guid userId, string fullName, string? phone, string? district, string? ward, CancellationToken ct);
    }
}
