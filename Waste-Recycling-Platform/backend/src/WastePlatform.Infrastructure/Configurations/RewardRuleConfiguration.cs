using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="RewardRule"/> entity.
/// </summary>
public class RewardRuleConfiguration : IEntityTypeConfiguration<RewardRule>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RewardRule> entity)
    {
        // Configure RewardRules
        entity.ToTable("reward_rules");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
        entity.Property(e => e.WasteCategoryId).HasColumnName("waste_category_id");
        entity.Property(e => e.PointsPerReport).HasColumnName("points_per_report");
        entity.Property(e => e.BonusQuality).HasColumnName("bonus_quality");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        
        entity.HasOne(e => e.Enterprise)
            .WithMany(en => en.RewardRules)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.WasteCategory)
            .WithMany(wc => wc.RewardRules)
            .HasForeignKey(e => e.WasteCategoryId);

        entity.HasIndex(e => new { e.EnterpriseId, e.WasteCategoryId }).IsUnique();
    }
}
