namespace WastePlatform.Domain.Entities;

public class Enterprise
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CompanyName { get; set; } = null!;
    public string? ServiceArea { get; set; } // JSON stored as string
    public int? CapacityKgPerDay { get; set; }
    public bool IsVerified { get; set; } = false;
    public string Status { get; set; } = "Pending"; // "Pending", "Verified", "Rejected"
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Collector> Collectors { get; set; } = new List<Collector>();
    public virtual ICollection<EnterpriseWasteType> WasteTypes { get; set; } = new List<EnterpriseWasteType>();
    public virtual ICollection<CollectionTask> CollectionTasks { get; set; } = new List<CollectionTask>();
    public virtual ICollection<RewardRule> RewardRules { get; set; } = new List<RewardRule>();
}
