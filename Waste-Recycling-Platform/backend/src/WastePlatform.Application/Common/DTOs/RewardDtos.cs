namespace WastePlatform.Application.Common.DTOs;

/// <summary>DTO for reward points history entry</summary>
public class RewardHistoryDto
{
    public Guid Id { get; set; }
    public int Points { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ReportId { get; set; }
}

/// <summary>DTO for paginated reward history response</summary>
public class RewardHistoryResponseDto
{
    public IEnumerable<RewardHistoryDto> Items { get; set; } = new List<RewardHistoryDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

/// <summary>DTO for total rewards display</summary>
public class TotalRewardsDto
{
    public int TotalPoints { get; set; }
    public DateTime? LastUpdated { get; set; }
}


