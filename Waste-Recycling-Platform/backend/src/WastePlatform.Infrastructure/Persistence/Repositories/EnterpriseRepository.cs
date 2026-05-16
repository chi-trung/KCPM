using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Enterprise.Queries;
// Thêm bí danh để code gọn và chuẩn với Interface
using DomainEnterprise = WastePlatform.Domain.Entities.Enterprise;

namespace WastePlatform.Infrastructure.Persistence.Repositories
{
    public class EnterpriseRepository : IEnterpriseRepository
    {
        private readonly WastePlatformDbContext _context;

        public EnterpriseRepository(WastePlatformDbContext context)
        {
            _context = context;
        }

        public async Task<EnterpriseDto?> GetEnterpriseByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var enterprise = await _context.Enterprises
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => new EnterpriseDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    CompanyName = e.CompanyName,
                    IsVerified = e.IsVerified,
                    CreatedAt = e.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            return enterprise;
        }

        public async Task<DomainEnterprise?> GetEnterpriseByIdAsync(string enterpriseId, CancellationToken cancellationToken)
        {
            return await _context.Enterprises
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id.ToString() == enterpriseId, cancellationToken);
        }

        public async Task<List<DomainEnterprise>> GetEnterpriseListAsync(CancellationToken cancellationToken)
        {
            return await _context.Enterprises
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // ====================================================================
        // --- 2 HÀM MỚI ĐƯỢC BỔ SUNG ĐỂ KHÔNG BỊ LỖI THIẾU INTERFACE MEMBER ---
        // ====================================================================

        public async Task<List<DomainEnterprise>> GetEnterprisesByWasteCategoryAsync(int wasteCategoryId, CancellationToken cancellationToken)
        {
            return await _context.Enterprises
                .AsNoTracking()
                // TODO: Bỏ comment dòng dưới và sửa lại 'WasteCategories' cho đúng với thuộc tính trong Entity của bạn
                // .Where(e => e.WasteCategories.Any(wc => wc.Id == wasteCategoryId))
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(DomainEnterprise enterprise, CancellationToken cancellationToken)
        {
            _context.Enterprises.Update(enterprise);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}