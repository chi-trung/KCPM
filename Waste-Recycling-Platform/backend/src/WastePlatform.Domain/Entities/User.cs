using WastePlatform.Domain.Enums;

namespace WastePlatform.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? Phone { get; private set; }
    public UserRole Role { get; private set; }
    public string? District { get; private set; }
    public string? Ward { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public ICollection<WasteReport> WasteReports { get; private set; } = new List<WasteReport>();
    public ICollection<RewardPoints> RewardPoints { get; private set; } = new List<RewardPoints>();
    public ICollection<Complaint> Complaints { get; private set; } = new List<Complaint>();
    public ICollection<AuditLog> AuditLogs { get; private set; } = new List<AuditLog>();
    public Enterprise? Enterprise { get; private set; }
    public Collector? Collector { get; private set; }

    protected User() { }

    public static User Create(string email, string passwordHash, string fullName,
        UserRole role, string? phone = null, string? district = null, string? ward = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName,
            Role = role,
            Phone = phone,
            District = district,
            Ward = ward,
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string fullName, string? phone, string? district, string? ward)
    {
        FullName = fullName;
        Phone = phone;
        District = district;
        Ward = ward;
        UpdatedAt = DateTime.UtcNow;
    }
}
