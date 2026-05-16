namespace WastePlatform.Domain.Entities;

public class RewardRule
{
    public Guid Id { get; set; }
    public Guid EnterpriseId { get; set; }
    public int WasteCategoryId { get; set; }
    public int PointsPerReport { get; set; } = 10;
    public int BonusQuality { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Enterprise Enterprise { get; set; } = null!;
    public virtual WasteCategory WasteCategory { get; set; } = null!;
}
