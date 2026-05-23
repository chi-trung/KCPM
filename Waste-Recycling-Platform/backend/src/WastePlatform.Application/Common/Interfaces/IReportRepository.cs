using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Common.Interfaces;

public interface IReportRepository
{
    Task<WasteReport> AddAsync(WasteReport report, CancellationToken cancellationToken = default);
    Task<WasteReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<WasteReport> Reports, int Total)> GetByCitizenIdAsync(Guid citizenId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IEnumerable<WasteReport> Reports, int Total)> GetAllAsync(int page, int pageSize, ReportStatus? status, CancellationToken cancellationToken = default);
    Task<(IEnumerable<WasteReport> Reports, int Total)> GetEnterpriseReportsAsync(Guid enterpriseId, int page, int pageSize, ReportStatus? status, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
