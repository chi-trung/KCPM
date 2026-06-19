using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="User"/> entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> entity)
    {
        // Configure Users
        entity.ToTable("users");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
        entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
        entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(100);
        entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15);
        entity.Property(e => e.District).HasColumnName("district").HasMaxLength(100);
        entity.Property(e => e.Ward).HasColumnName("ward").HasMaxLength(100);
        entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>();
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired(false);

        entity.HasIndex(e => e.Email).IsUnique();
        // Phone nullable: filter index to avoid UNIQUE violation on multiple NULLs in MySQL
        entity.HasIndex(e => e.Phone).IsUnique().HasFilter("`Phone` IS NOT NULL");
        entity.HasIndex(e => new { e.District, e.Ward });
        entity.HasIndex(e => e.Role);
    }
}
