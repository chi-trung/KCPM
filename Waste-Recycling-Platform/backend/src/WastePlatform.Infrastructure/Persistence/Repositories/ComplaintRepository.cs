using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.Infrastructure.Persistence.Repositories;

public class ComplaintRepository : IComplaintRepository
{
    private readonly WastePlatformDbContext _context;

    public ComplaintRepository(WastePlatformDbContext context)
    {
        _context = context;
    }

    public async Task<Complaint> AddAsync(Complaint complaint, CancellationToken cancellationToken = default)
    {
        await _context.Complaints.AddAsync(complaint, cancellationToken);
        return complaint;
    }

    public async Task<Complaint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Complaints
            .Include(c => c.Citizen)
            .Include(c => c.WasteReport)
            .Include(c => c.Enterprise)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<Complaint> Complaints, int Total)> GetAllAsync(int page, int pageSize, ComplaintStatus? status, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Complaints
            .Include(c => c.Citizen)
            .Include(c => c.WasteReport)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c => 
                c.Citizen.FullName.Contains(searchTerm) ||
                c.Content.Contains(searchTerm));
        }

        var total = await query.CountAsync(cancellationToken);

        var complaints = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (complaints, total);
    }

    public async Task<(IEnumerable<Complaint> Complaints, int Total)> GetByCitizenIdAsync(Guid citizenId, int page, int pageSize, ComplaintStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _context.Complaints
            .Where(c => c.CitizenId == citizenId)
            .Include(c => c.Citizen)
            .Include(c => c.WasteReport)
            .Include(c => c.Enterprise)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var complaints = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (complaints, total);
    }

    public async Task<(IEnumerable<Complaint> Complaints, int Total)> GetByEnterpriseIdAsync(Guid enterpriseId, int page, int pageSize, ComplaintStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _context.Complaints
            .Where(c => c.EnterpriseId == enterpriseId)
            .Include(c => c.Citizen)
            .Include(c => c.WasteReport)
            .Include(c => c.Enterprise)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var complaints = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (complaints, total);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
