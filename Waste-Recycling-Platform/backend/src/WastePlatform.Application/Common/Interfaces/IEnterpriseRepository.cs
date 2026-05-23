using WastePlatform.Application.Enterprise.Queries;
// Đặt bí danh cho class Enterprise để không bị nhầm lẫn
using DomainEnterprise = WastePlatform.Domain.Entities.Enterprise;

namespace WastePlatform.Application.Common.Interfaces
{
    public interface IEnterpriseRepository
    {
        Task<EnterpriseDto?> GetEnterpriseByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<DomainEnterprise?> GetEnterpriseByIdAsync(string enterpriseId, CancellationToken cancellationToken);
        Task<List<DomainEnterprise>> GetEnterpriseListAsync(CancellationToken cancellationToken);
        
        // --- 2 hàm mới được bổ sung từ các tính năng bạn vừa code ---
        Task<List<DomainEnterprise>> GetEnterprisesByWasteCategoryAsync(int wasteCategoryId, CancellationToken cancellationToken);
        Task UpdateAsync(DomainEnterprise enterprise, CancellationToken cancellationToken);
    }
}