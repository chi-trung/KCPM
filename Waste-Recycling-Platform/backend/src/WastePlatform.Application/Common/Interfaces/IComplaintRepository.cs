using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Common.Interfaces;

public interface IComplaintRepository
{
    Task<Complaint> AddAsync(Complaint complaint, CancellationToken cancellationToken = default);
    Task<Complaint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Complaint> Complaints, int Total)> GetByEnterpriseIdAsync(Guid enterpriseId, int page, int pageSize, ComplaintStatus? status, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Complaint> Complaints, int Total)> GetAllAsync(int page, int pageSize, ComplaintStatus? status, string? searchTerm, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Complaint> Complaints, int Total)> GetByCitizenIdAsync(Guid citizenId, int page, int pageSize, ComplaintStatus? status, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCitizenAndReportAsync(Guid citizenId, Guid reportId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
