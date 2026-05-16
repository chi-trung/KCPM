using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums; // Ensure the namespace containing UserRole is imported

namespace WastePlatform.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WastePlatformDbContext _context;

        public UserRepository(WastePlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetUsersAsync(string? search, string? role, CancellationToken cancellationToken)
        {
            var query = _context.Users.AsQueryable();

            // 1. Search Logic (EF.Functions.Like)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";
                query = query.Where(u => 
                    EF.Functions.Like(u.FullName, pattern) || 
                    EF.Functions.Like(u.Email, pattern));
            }

            // 2. Filter by Role (Fix CS0019 error here)
            if (!string.IsNullOrWhiteSpace(role) && !role.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                // Try to convert string 'role' to Enum 'UserRole'
                // The 'true' parameter is to ignore case (e.g., "admin" or "Admin" are both accepted)
                if (Enum.TryParse<UserRole>(role, true, out var roleEnum))
                {
                    // Now comparing Enum with Enum is the correct way!
                    query = query.Where(u => u.Role == roleEnum);
                }
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken ct)
        {
            return await _context.Users.CountAsync(ct);
        }

        public async Task<string> CreateUserAsync(string email, string passwordHash, string fullName, string phone, string role, string district, string ward, CancellationToken ct)
        {
            if (!Enum.TryParse<UserRole>(role, true, out var roleEnum))
            {
                // Or throw a specific exception
                throw new ArgumentException("Invalid user role specified", nameof(role));
            }

            var newUser = User.Create(
                email,
                passwordHash,
                fullName,
                roleEnum,
                phone,
                district,
                ward
            );

            await _context.Users.AddAsync(newUser, ct);
            await _context.SaveChangesAsync(ct);

            return newUser.Id.ToString();
        }

        public async Task<bool> ToggleUserStatusAsync(string userId, CancellationToken ct)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return false;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userGuid, ct);
            if (user == null) return false;

            if (user.IsActive)
            {
                user.Deactivate();
            }
            else
            {
                user.Activate();
            }

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole, CancellationToken ct)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return false;
            }
            
            if (!Enum.TryParse<UserRole>(newRole, true, out var roleEnum))
            {
                return false; // Or throw an exception for invalid role
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userGuid, ct);
            if (user == null) return false;

            user.UpdateRole(roleEnum);

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        }

        public async Task<User> UpdateProfileAsync(Guid userId, string fullName, string? phone, string? district, string? ward, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            user.UpdateProfile(fullName, phone, district, ward);

            await _context.SaveChangesAsync(ct);
            return user;
        }
    }
}