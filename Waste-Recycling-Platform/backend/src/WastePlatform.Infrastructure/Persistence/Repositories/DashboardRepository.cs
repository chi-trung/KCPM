using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Admin.Dashboard.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WastePlatform.Infrastructure.Persistence.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly WastePlatformDbContext _context;

        public DashboardRepository(WastePlatformDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct)
        {
            // 1. Thống kê cơ bản từ Database
            var totalUsers = await _context.Users.CountAsync(ct);
            var totalReports = await _context.WasteReports.CountAsync(ct);
            
            // So sánh Enum bằng cách chuyển Enum sang chuỗi (nếu C# báo lỗi, ông có thể đổi sang so sánh Enum trực tiếp vd: ComplaintStatus.Pending)
            var pendingComplaints = await _context.Complaints
                .CountAsync(c => c.Status.ToString() == "Pending", ct); 
                
            var completedReports = await _context.WasteReports
                .CountAsync(r => r.Status.ToString() == "Completed", ct);
                
            var acceptedReports = await _context.WasteReports
                .CountAsync(r => r.Status.ToString() == "Accepted", ct);
            
            // Đếm Collector hoạt động
            var activeCollectors = await _context.Collectors
                .CountAsync(c => c.IsAvailable, ct);
            
            // Tính tổng khối lượng rác từ CollectionTasks đã hoàn thành
            var totalWasteWeightDecimal = await _context.CollectionTasks
                .Where(t => t.Status.ToString() == "Completed" && t.CollectedWeightKg != null)
                .SumAsync(t => t.CollectedWeightKg, ct);
            
            // Ép kiểu Decimal sang Double cho khớp với DTO
            var totalWasteWeight = totalWasteWeightDecimal.HasValue ? (double)totalWasteWeightDecimal.Value : 0.0;

            // 2. Dữ liệu cho Biểu đồ Line Chart (Lưu lượng 6 tháng gần nhất)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyTrafficRaw = await _context.WasteReports
                .Where(r => r.CreatedAt >= sixMonthsAgo)
                .GroupBy(r => r.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var monthlyTraffic = monthlyTrafficRaw
                .Select(x => new MonthlyReportDto { Month = "T" + x.Month, Count = x.Count })
                .ToList();

            // 3. Dữ liệu cho Biểu đồ Pie Chart (Phân bố người dùng theo Role)
            var userDistributionRaw = await _context.Users
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var userDistribution = userDistributionRaw
                .Select(x => new UserDistributionDto 
                { 
                    Name = x.Role.ToString() == "Citizen" ? "Người dân" : 
                           x.Role.ToString() == "Collector" ? "Người thu gom" : 
                           x.Role.ToString() == "Enterprise" ? "Doanh nghiệp" : "Khác", 
                    Value = x.Count 
                })
                .ToList();

            // 4. Dữ liệu cho Log hoạt động (Lấy 5 hành động từ AuditLogs)
            var recentLogsRaw = await _context.AuditLogs
                .Include(a => a.User) // Include để lấy tên User
                .OrderByDescending(l => l.CreatedAt)
                .Take(5)
                .ToListAsync(ct);

            var recentLogs = recentLogsRaw
                .Select(l => new ActivityLogDto 
                {
                    User = l.User != null ? l.User.FullName : "Hệ thống",
                    Action = l.Action,
                    Time = l.CreatedAt.ToString("dd/MM HH:mm"),
                    Type = "info" // Mặc định là info, vì AuditLog của ông không có cột Type
                })
                .ToList();

            // 5. Trả về toàn bộ DTO
            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalReports = totalReports,
                PendingComplaints = pendingComplaints,
                TotalWasteWeight = totalWasteWeight,
                CompletedReports = completedReports,
                ActiveCollectors = activeCollectors,
                AcceptedReports = acceptedReports,
                MonthlyTraffic = monthlyTraffic,
                UserDistribution = userDistribution,
                RecentLogs = recentLogs
            };
        }
    }
}