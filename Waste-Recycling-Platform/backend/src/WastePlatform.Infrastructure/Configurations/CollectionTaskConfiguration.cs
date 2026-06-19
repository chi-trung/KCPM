using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="CollectionTask"/> entity.
/// </summary>
public class CollectionTaskConfiguration : IEntityTypeConfiguration<CollectionTask>
{
    private static readonly ValueConverter<CollectionTaskStatus, string> CollectionTaskStatusConverter = new(
        v => v == CollectionTaskStatus.OnTheWay ? "on_the_way" : v.ToString().ToLower(),
        v => v == "on_the_way" ? CollectionTaskStatus.OnTheWay : Enum.Parse<CollectionTaskStatus>(v, true)
    );

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CollectionTask> entity)
    {
        // Configure CollectionTasks
        entity.ToTable("collection_tasks");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.ReportId).HasColumnName("report_id");
        entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
        entity.Property(e => e.CollectorId).HasColumnName("collector_id");
        entity.Property(e => e.Status).HasColumnName("status").HasConversion(CollectionTaskStatusConverter);
        entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(500);
        entity.Property(e => e.CollectedWeightKg).HasColumnName("collected_weight_kg").HasPrecision(8, 2);
        entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
        entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
        
        entity.HasOne(e => e.WasteReport)
            .WithOne(r => r.CollectionTask)
            .HasForeignKey<CollectionTask>(e => e.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Enterprise)
            .WithMany(en => en.CollectionTasks)
            .HasForeignKey(e => e.EnterpriseId);

        entity.HasOne(e => e.Collector)
            .WithMany(c => c.CollectionTasks)
            .HasForeignKey(e => e.CollectorId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(e => e.ReportId).IsUnique();
        entity.HasIndex(e => e.EnterpriseId);
        entity.HasIndex(e => e.CollectorId);
        entity.HasIndex(e => e.Status);
    }
}
