namespace WastePlatform.Domain.Entities;

public class Collector
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EnterpriseId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Enterprise Enterprise { get; set; } = null!;
    public virtual ICollection<CollectionTask> CollectionTasks { get; set; } = new List<CollectionTask>();

    public void ToggleAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }
}
