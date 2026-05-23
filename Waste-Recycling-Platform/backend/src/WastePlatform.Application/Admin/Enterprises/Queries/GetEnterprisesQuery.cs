using MediatR;
using WastePlatform.Application.Admin.Enterprises.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Enterprises.Queries;

public class GetEnterprisesQuery : IRequest<(IEnumerable<EnterpriseListDto> Enterprises, int Total, int TotalPages)>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool? IsVerified { get; set; }
    public string? SearchTerm { get; set; }
}

public class GetEnterprisesQueryHandler : IRequestHandler<GetEnterprisesQuery, (IEnumerable<EnterpriseListDto> Enterprises, int Total, int TotalPages)>
{
    private readonly IEnterpriseRepository _enterpriseRepository;

    public GetEnterprisesQueryHandler(IEnterpriseRepository enterpriseRepository)
    {
        _enterpriseRepository = enterpriseRepository;
    }

    public async Task<(IEnumerable<EnterpriseListDto> Enterprises, int Total, int TotalPages)> Handle(GetEnterprisesQuery request, CancellationToken cancellationToken)
    {
        var enterprises = await _enterpriseRepository.GetEnterpriseListAsync(cancellationToken);
        
        var query = enterprises.AsEnumerable();
        
        if (request.IsVerified.HasValue)
        {
            query = query.Where(e => e.IsVerified == request.IsVerified.Value);
        }
        
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(e => 
                e.CompanyName.Contains(request.SearchTerm));
        }
        
        var total = query.Count();
        
        var paginatedEnterprises = query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        
        var dtoList = paginatedEnterprises.Select(e => new EnterpriseListDto
        {
            Id = e.Id,
            CompanyName = e.CompanyName,
            UserEmail = "N/A",
            ServiceArea = e.ServiceArea,
            IsVerified = e.IsVerified,
            CreatedAt = e.CreatedAt
        }).ToList();

        int totalPages = (total + request.PageSize - 1) / request.PageSize;

        return (dtoList, total, totalPages);
    }
}
