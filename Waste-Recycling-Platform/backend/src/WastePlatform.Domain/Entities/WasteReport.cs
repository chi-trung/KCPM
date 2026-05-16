using WastePlatform.Domain.Enums;

namespace WastePlatform.Domain.Entities;

public class WasteReport
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CitizenId { get; private set; }
    public int? WasteCategoryId { get; private set; }
    public string? Description { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string? Address { get; private set; }
    public ReportStatus Status { get; private set; } = ReportStatus.Pending;
    public string? AiSuggestion { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public User Citizen { get; private set; } = null!;
    public WasteCategory? WasteCategory { get; private set; }
    public CollectionTask? CollectionTask { get; private set; }
    public ICollection<ReportImage> Images { get; private set; } = new List<ReportImage>();
    public ICollection<RewardPoints> RewardPoints { get; private set; } = new List<RewardPoints>();
    public ICollection<Complaint> Complaints { get; private set; } = new List<Complaint>();

    protected WasteReport() { }

    public static WasteReport Create(Guid citizenId, int wasteCategoryId,
        decimal latitude, decimal longitude, string? description = null, string? address = null, string? aiSuggestion = null)
    {
        return new WasteReport
        {
            Id = Guid.NewGuid(),
            CitizenId = citizenId,
            WasteCategoryId = wasteCategoryId,
            Latitude = latitude,
            Longitude = longitude,
            Description = description,
            Address = address,
            AiSuggestion = aiSuggestion,
            Status = ReportStatus.Pending,
        };
    }

    public void Accept()   => TransitionTo(ReportStatus.Accepted);
    public void Reject()   => TransitionTo(ReportStatus.Rejected);
    public void Assign()   => TransitionTo(ReportStatus.Assigned);
    public void Collect()  => TransitionTo(ReportStatus.Collected);

    private void TransitionTo(ReportStatus next)
    {
        var valid = (Status, next) switch
        {
            (ReportStatus.Pending,    ReportStatus.Accepted)  => true,
            (ReportStatus.Pending,    ReportStatus.Rejected)  => true,
            (ReportStatus.Accepted,   ReportStatus.Assigned)  => true,
            (ReportStatus.Accepted,   ReportStatus.Collected) => true,
            (ReportStatus.Assigned,   ReportStatus.Collected) => true,
            _ => false
        };
        if (!valid) throw new InvalidOperationException($"Cannot transition report from {Status} to {next}");
        Status = next;
    }
}
