using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.Infrastructure.Persistence.Repositories;

public class WasteCategoryRepository : IWasteCategoryRepository
{
    private readonly WastePlatformDbContext _context;

    public WasteCategoryRepository(WastePlatformDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WasteCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.WasteCategories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<WasteCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.WasteCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
