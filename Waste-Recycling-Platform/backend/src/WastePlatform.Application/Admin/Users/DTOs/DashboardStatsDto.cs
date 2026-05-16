namespace WastePlatform.Application.Admin.Dashboard.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalReports { get; set; }
        public int PendingComplaints { get; set; }
        public double TotalWasteWeight { get; set; } // kg
        
        // Các trường mới thêm vào cho Frontend
        public int CompletedReports { get; set; }
        public int ActiveCollectors { get; set; }
        public int AcceptedReports { get; set; }

        public List<MonthlyReportDto> MonthlyTraffic { get; set; } = new();
        public List<UserDistributionDto> UserDistribution { get; set; } = new();
        public List<ActivityLogDto> RecentLogs { get; set; } = new();
        
        // Waste statistics for charts
        public List<WasteByAreaDto> WasteByArea { get; set; } = new();
        public List<WasteByTypeDto> WasteByType { get; set; } = new();
    }

    public class MonthlyReportDto
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // DTO cho biểu đồ Phân bố tài khoản (Pie Chart)
    public class UserDistributionDto
    {
        public string Name { get; set; } = string.Empty; // Ví dụ: "Người dân", "Thu gom"
        public int Value { get; set; } // Số lượng
    }

    // DTO cho Log hoạt động gần đây
    public class ActivityLogDto
    {
        public string User { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Ví dụ: "report", "warning", "info"
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
}