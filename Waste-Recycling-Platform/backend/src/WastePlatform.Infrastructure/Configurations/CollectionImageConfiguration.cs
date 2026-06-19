using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="CollectionImage"/> entity.
/// </summary>
public class CollectionImageConfiguration : IEntityTypeConfiguration<CollectionImage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CollectionImage> entity)
    {
        // Configure CollectionImages
        entity.ToTable("collection_images");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.TaskId).HasColumnName("task_id");
        entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsRequired().HasMaxLength(500);
        
        entity.HasOne(e => e.CollectionTask)
            .WithMany(t => t.Images)
            .HasForeignKey(e => e.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
