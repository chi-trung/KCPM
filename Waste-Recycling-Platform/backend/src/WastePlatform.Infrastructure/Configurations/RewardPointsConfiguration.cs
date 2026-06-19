using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="RewardPoints"/> entity.
/// </summary>
public class RewardPointsConfiguration : IEntityTypeConfiguration<RewardPoints>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RewardPoints> entity)
    {
        // Configure RewardPoints
        entity.ToTable("reward_points");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.CitizenId).HasColumnName("citizen_id");
        entity.Property(e => e.ReportId).HasColumnName("report_id");
        entity.Property(e => e.Points).HasColumnName("points");
        entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(255);
        entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        
        entity.HasOne(e => e.Citizen)
            .WithMany(u => u.RewardPoints)
            .HasForeignKey(e => e.CitizenId);

        entity.HasOne(e => e.WasteReport)
            .WithMany(r => r.RewardPoints)
            .HasForeignKey(e => e.ReportId)
            .OnDelete(DeleteBehavior.SetNull);

        // IdempotencyKey nullable: only unique when non-null
        entity.HasIndex(e => e.IdempotencyKey).IsUnique().HasFilter("`IdempotencyKey` IS NOT NULL");
        entity.HasIndex(e => new { e.CitizenId, e.CreatedAt });
    }
}
