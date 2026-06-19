using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="TaskStatusLog"/> entity.
/// </summary>
public class TaskStatusLogConfiguration : IEntityTypeConfiguration<TaskStatusLog>
{
    private static readonly ValueConverter<CollectionTaskStatus, string> CollectionTaskStatusConverter = new(
        v => v == CollectionTaskStatus.OnTheWay ? "on_the_way" : v.ToString().ToLower(),
        v => v == "on_the_way" ? CollectionTaskStatus.OnTheWay : Enum.Parse<CollectionTaskStatus>(v, true)
    );

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TaskStatusLog> entity)
    {
        // Configure TaskStatusLogs
        entity.ToTable("task_status_logs");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.TaskId).HasColumnName("task_id");
        entity.Property(e => e.Status).HasColumnName("status").HasConversion(CollectionTaskStatusConverter);
        entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
        
        entity.HasOne(e => e.CollectionTask)
            .WithMany(t => t.StatusLogs)
            .HasForeignKey(e => e.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.TaskId);
    }
}
