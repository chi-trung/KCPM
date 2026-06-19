using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="ReportImage"/> entity.
/// </summary>
public class ReportImageConfiguration : IEntityTypeConfiguration<ReportImage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ReportImage> entity)
    {
        // Configure ReportImages
        entity.ToTable("report_images");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.ReportId).HasColumnName("report_id");
        entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsRequired().HasMaxLength(500);
        entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at");
        
        entity.HasOne(e => e.WasteReport)
            .WithMany(r => r.Images)
            .HasForeignKey(e => e.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
