namespace WastePlatform.Domain.Entities;

public class RewardPoints
{
    public Guid Id { get; set; }
    public Guid CitizenId { get; set; }
    public Guid? ReportId { get; set; }
    public string? IdempotencyKey { get; set; }
    public int Points { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User Citizen { get; set; } = null!;
    public virtual WasteReport? WasteReport { get; set; }
}
