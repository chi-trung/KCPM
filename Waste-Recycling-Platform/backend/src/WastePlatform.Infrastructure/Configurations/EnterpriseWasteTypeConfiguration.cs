using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="EnterpriseWasteType"/> entity.
/// </summary>
public class EnterpriseWasteTypeConfiguration : IEntityTypeConfiguration<EnterpriseWasteType>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EnterpriseWasteType> entity)
    {
        // Configure EnterpriseWasteTypes
        entity.ToTable("enterprise_waste_types");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
        entity.Property(e => e.WasteCategoryId).HasColumnName("waste_category_id");
        
        entity.HasOne(e => e.Enterprise)
            .WithMany(en => en.WasteTypes)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.WasteCategory)
            .WithMany(wc => wc.EnterpriseWasteTypes)
            .HasForeignKey(e => e.WasteCategoryId);

        entity.HasIndex(e => new { e.EnterpriseId, e.WasteCategoryId }).IsUnique();
    }
}
