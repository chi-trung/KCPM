using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="WasteReport"/> entity.
/// </summary>
public class WasteReportConfiguration : IEntityTypeConfiguration<WasteReport>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WasteReport> entity)
    {
        // Configure WasteReports
        entity.ToTable("waste_reports");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.CitizenId).HasColumnName("citizen_id");
        entity.Property(e => e.WasteCategoryId).HasColumnName("waste_category_id");
        entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
        entity.Property(e => e.Latitude).HasColumnName("latitude").HasPrecision(10, 8);
        entity.Property(e => e.Longitude).HasColumnName("longitude").HasPrecision(11, 8);
        entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
        entity.Property(e => e.AiSuggestion).HasColumnName("ai_suggestion").HasMaxLength(50);
        entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        
        entity.HasOne(e => e.Citizen)
            .WithMany(u => u.WasteReports)
            .HasForeignKey(e => e.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.WasteCategory)
            .WithMany(wc => wc.WasteReports)
            .HasForeignKey(e => e.WasteCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(e => e.CitizenId);
        entity.HasIndex(e => e.WasteCategoryId);
        entity.HasIndex(e => e.Status);
        entity.HasIndex(e => new { e.Latitude, e.Longitude });
        entity.HasIndex(e => e.CreatedAt);
    }
}
