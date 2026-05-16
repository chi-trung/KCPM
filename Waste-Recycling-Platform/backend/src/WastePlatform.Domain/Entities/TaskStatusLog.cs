using WastePlatform.Domain.Enums;

namespace WastePlatform.Domain.Entities;

public class TaskStatusLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public CollectionTaskStatus Status { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual CollectionTask CollectionTask { get; set; } = null!;
}
