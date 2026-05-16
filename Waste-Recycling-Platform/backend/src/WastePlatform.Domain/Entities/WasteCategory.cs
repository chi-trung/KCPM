namespace WastePlatform.Domain.Entities;

public class WasteCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<EnterpriseWasteType> EnterpriseWasteTypes { get; set; } = new List<EnterpriseWasteType>();
    public virtual ICollection<WasteReport> WasteReports { get; set; } = new List<WasteReport>();
    public virtual ICollection<RewardRule> RewardRules { get; set; } = new List<RewardRule>();
}
