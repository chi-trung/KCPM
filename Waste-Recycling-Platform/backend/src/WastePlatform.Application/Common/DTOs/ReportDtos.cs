using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Common.DTOs;

public class CreateReportDto
{
    public int WasteCategoryId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? AiSuggestion { get; set; }
}

public class ReportDto
{
    public Guid Id { get; set; }
    public Guid CitizenId { get; set; }
    public string? CitizenName { get; set; }
    public int? WasteCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Address { get; set; }
    public ReportStatus Status { get; set; }
    public string? AiSuggestion { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<RewardPointsDto> RewardPoints { get; set; } = new();
}

public class RewardPointsDto
{
    public Guid Id { get; set; }
    public decimal Points { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReportListDto
{
    public Guid Id { get; set; }
    public string? CitizenName { get; set; }
    public string? CategoryName { get; set; }
    public ReportStatus Status { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ImageCount { get; set; }
}
