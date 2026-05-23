namespace WastePlatform.Domain.Entities;

public class CollectionImage
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string ImageUrl { get; set; } = null!;

    // Navigation properties
    public virtual CollectionTask CollectionTask { get; set; } = null!;
}
