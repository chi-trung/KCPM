using WastePlatform.Domain.Enums;

namespace WastePlatform.Domain.Entities;

public class CollectionTask
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ReportId { get; private set; }
    public Guid EnterpriseId { get; private set; }
    public Guid? CollectorId { get; private set; }
    public CollectionTaskStatus Status { get; private set; } = CollectionTaskStatus.Assigned;
    public decimal? CollectedWeightKg { get; private set; }
    public string? Notes { get; private set; }
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }

    // Navigation
    public WasteReport WasteReport { get; private set; } = null!;
    public Enterprise Enterprise { get; private set; } = null!;
    public Collector? Collector { get; private set; }
    public ICollection<TaskStatusLog> StatusLogs { get; private set; } = new List<TaskStatusLog>();
    public ICollection<CollectionImage> Images { get; private set; } = new List<CollectionImage>();

    public static CollectionTask Create(Guid reportId, Guid enterpriseId)
        => new() { Id = Guid.NewGuid(), ReportId = reportId, EnterpriseId = enterpriseId };

    public void AssignCollector(Guid collectorId) => CollectorId = collectorId;

    public void SetOnTheWay()
    {
        if (Status != CollectionTaskStatus.Assigned)
            throw new InvalidOperationException("Task must be Assigned before going OnTheWay");
        Status = CollectionTaskStatus.OnTheWay;
        StatusLogs.Add(new TaskStatusLog { TaskId = Id, Status = Status });
    }

    public void Complete(decimal weightKg, string? notes)
    {
        if (Status != CollectionTaskStatus.OnTheWay)
            throw new InvalidOperationException("Task must be OnTheWay before Collected");
        Status = CollectionTaskStatus.Collected;
        CollectedWeightKg = weightKg;
        Notes = notes;
        CompletedAt = DateTime.UtcNow;
        StatusLogs.Add(new TaskStatusLog { TaskId = Id, Status = Status });
    }
}
