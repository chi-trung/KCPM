using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="WasteCategory"/> entity.
/// </summary>
public class WasteCategoryConfiguration : IEntityTypeConfiguration<WasteCategory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WasteCategory> entity)
    {
        // Configure WasteCategories
        entity.ToTable("waste_categories");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
        entity.HasIndex(e => e.Name).IsUnique();
    }
}
