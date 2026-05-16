namespace WastePlatform.Application.Admin.Enterprises.DTOs;

public class EnterpriseDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
    public string CompanyName { get; set; } = null!;
    public string? ServiceArea { get; set; }
    public int? CapacityKgPerDay { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CollectorCount { get; set; }
    public int WasteTypeCount { get; set; }
}

public class EnterpriseListDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = null!;
    public string? UserEmail { get; set; }
    public string? ServiceArea { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VerifyEnterpriseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid EnterpriseId { get; set; }
}
