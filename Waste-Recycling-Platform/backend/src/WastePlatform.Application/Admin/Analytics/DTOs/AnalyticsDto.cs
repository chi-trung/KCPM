namespace WastePlatform.Application.Admin.Analytics.DTOs;

public class AnalyticsOverviewDto
{
    public int TotalReports { get; set; }
    public int TotalComplaints { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveEnterprises { get; set; }
    public int RegisteredCollectors { get; set; }
    public decimal TotalWasteCollected { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ReportAnalyticsDto
{
    public int TotalReports { get; set; }
    public int AcceptedReports { get; set; }
    public int PendingReports { get; set; }
    public int RejectedReports { get; set; }
    public int CollectedReports { get; set; }
    public Dictionary<string, int> ReportsByCategory { get; set; } = new();
    public decimal AverageReportsPerDay { get; set; }
    
    // Waste statistics for charts
    public List<WasteByAreaDto> WasteByArea { get; set; } = new();
    public List<WasteByTypeDto> WasteByType { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
}

public class UserAnalyticsDto
{
    public int TotalCitizens { get; set; }
    public int ActiveCitizens { get; set; }
    public int InactiveCitizens { get; set; }
    public int TotalEnterprises { get; set; }
    public int VerifiedEnterprises { get; set; }
    public int UnverifiedEnterprises { get; set; }
    public int TotalCollectors { get; set; }
    public int ActiveCollectors { get; set; }
    public int TotalAdmins { get; set; }
}

public class WasteAnalyticsDto
{
    public int TotalWasteCategories { get; set; }
    public Dictionary<string, decimal> WasteByCategory { get; set; } = new();
    public decimal TotalWasteKg { get; set; }
    public Dictionary<string, decimal> WasteByMonth { get; set; } = new();
    public decimal AverageWastePerReport { get; set; }
    public int ActiveWasteTypes { get; set; }
}

public class AnalyticsSummaryDto
{
    public AnalyticsOverviewDto Overview { get; set; } = new();
    public ReportAnalyticsDto ReportAnalytics { get; set; } = new();
    public UserAnalyticsDto UserAnalytics { get; set; } = new();
    public WasteAnalyticsDto WasteAnalytics { get; set; } = new();
}

// DTO cho thống kê rác theo khu vực
public class WasteByAreaDto
{
    public string Area { get; set; } = string.Empty; // Tên khu vực (Quận/Huyện)
    public int Count { get; set; } // Số lượng báo cáo
    public double WeightKg { get; set; } // Tổng trọng lượng (kg)
}

// DTO cho thống kê rác theo loại
public class WasteByTypeDto
{
    public string Type { get; set; } = string.Empty; // Loại rác (Organic, Recyclable, Hazardous)
    public int Count { get; set; } // Số lượng báo cáo
    public double WeightKg { get; set; } // Tổng trọng lượng (kg)
    public double Percentage { get; set; } // Tỷ lệ phần trăm
}

// DTO cho xu hướng theo thời gian
public class MonthlyTrendDto
{
    public string Month { get; set; } = string.Empty; // Format: "2024-01"
    public int ReportCount { get; set; }
    public double WeightKg { get; set; }
}
