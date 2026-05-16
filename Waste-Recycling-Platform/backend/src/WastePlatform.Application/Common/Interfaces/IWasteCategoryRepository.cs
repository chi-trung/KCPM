using WastePlatform.Domain.Entities;

namespace WastePlatform.Application.Common.Interfaces;

public interface IWasteCategoryRepository
{
    Task<IEnumerable<WasteCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WasteCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
