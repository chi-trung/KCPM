using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="AuditLog"/> entity.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        // Configure AuditLogs
        entity.ToTable("audit_logs");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.Action).HasColumnName("action").IsRequired().HasMaxLength(100);
        entity.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(50);
        entity.Property(e => e.EntityId).HasColumnName("entity_id");
        entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        
        entity.HasOne(e => e.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        entity.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}
