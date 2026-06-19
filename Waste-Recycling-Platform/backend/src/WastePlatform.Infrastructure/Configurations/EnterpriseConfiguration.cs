using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="Enterprise"/> entity.
/// </summary>
public class EnterpriseConfiguration : IEntityTypeConfiguration<Enterprise>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Enterprise> entity)
    {
        // Configure Enterprises
        entity.ToTable("enterprises");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.CompanyName).HasColumnName("company_name").IsRequired().HasMaxLength(200);
        entity.Property(e => e.ServiceArea).HasColumnName("service_area").HasMaxLength(500);
        entity.Property(e => e.CapacityKgPerDay).HasColumnName("capacity_kg_per_day");
        entity.Property(e => e.IsVerified).HasColumnName("is_verified");
        entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
        entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        
        entity.HasOne(e => e.User)
            .WithOne(u => u.Enterprise)
            .HasForeignKey<Enterprise>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.UserId).IsUnique();
    }
}
