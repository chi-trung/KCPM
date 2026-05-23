namespace WastePlatform.Domain.Entities;

public class EnterpriseWasteType
{
    public Guid Id { get; set; }
    public Guid EnterpriseId { get; set; }
    public int WasteCategoryId { get; set; }

    // Navigation properties
    public virtual Enterprise Enterprise { get; set; } = null!;
    public virtual WasteCategory WasteCategory { get; set; } = null!;
}
