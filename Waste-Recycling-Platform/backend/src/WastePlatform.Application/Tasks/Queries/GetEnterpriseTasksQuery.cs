using MediatR;
using WastePlatform.Application.Common.DTOs;

namespace WastePlatform.Application.Tasks.Queries;

public class GetEnterpriseTasksQuery : IRequest<List<CollectionTaskDto>>
{
    public Guid EnterpriseId { get; set; }
    public string? Status { get; set; }
    public bool? UnassignedOnly { get; set; }
}

public class CollectionTaskDto
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid EnterpriseId { get; set; }
    public Guid? CollectorId { get; set; }
    public string? CollectorName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? CollectedWeightKg { get; set; }
    public string? Notes { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ReportDetailDto Report { get; set; } = new();
}

public class ReportDetailDto
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string? CitizenPhone { get; set; }
}
