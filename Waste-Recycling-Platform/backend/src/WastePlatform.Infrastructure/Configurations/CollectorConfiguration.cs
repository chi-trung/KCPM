using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="Collector"/> entity.
/// </summary>
public class CollectorConfiguration : IEntityTypeConfiguration<Collector>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Collector> entity)
    {
        // Configure Collectors
        entity.ToTable("collectors");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
        entity.Property(e => e.IsAvailable).HasColumnName("is_available");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        
        entity.HasOne(e => e.User)
            .WithOne(u => u.Collector)
            .HasForeignKey<Collector>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Enterprise)
            .WithMany(en => en.Collectors)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.UserId).IsUnique();
    }
}
