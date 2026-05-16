namespace WastePlatform.Domain.Entities;

public class ReportImage
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public int SortOrder { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual WasteReport WasteReport { get; set; } = null!;
}
