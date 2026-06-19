using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="Notification"/> entity.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Notification> entity)
    {
        // Configure Notifications
        entity.ToTable("notifications");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.CitizenId).HasColumnName("citizen_id").IsRequired(false);
        entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
        entity.Property(e => e.Channel).HasColumnName("channel").HasConversion<string>();
        entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
        entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
        entity.Property(e => e.Message).HasColumnName("message").IsRequired().HasMaxLength(1000);
        entity.Property(e => e.ActionUrl).HasColumnName("action_url").HasMaxLength(500);
        entity.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id");
        entity.Property(e => e.RelatedEntityType).HasColumnName("related_entity_type").HasMaxLength(50);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        entity.Property(e => e.ReadAt).HasColumnName("read_at").IsRequired(false);
        
        entity.HasOne(e => e.Citizen)
            .WithMany()
            .HasForeignKey(e => e.CitizenId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(e => e.CitizenId);
        entity.HasIndex(e => new { e.CitizenId, e.Status });
        entity.HasIndex(e => e.CreatedAt);
    }
}
