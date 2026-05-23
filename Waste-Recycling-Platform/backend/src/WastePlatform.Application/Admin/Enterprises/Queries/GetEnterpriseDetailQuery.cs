using MediatR;
using WastePlatform.Application.Admin.Enterprises.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Enterprises.Queries;

public class GetEnterpriseDetailQuery : IRequest<EnterpriseDetailDto?>
{
    public Guid EnterpriseId { get; set; }
}

public class GetEnterpriseDetailQueryHandler : IRequestHandler<GetEnterpriseDetailQuery, EnterpriseDetailDto?>
{
    private readonly IEnterpriseRepository _enterpriseRepository;

    public GetEnterpriseDetailQueryHandler(IEnterpriseRepository enterpriseRepository)
    {
        _enterpriseRepository = enterpriseRepository;
    }

    public async Task<EnterpriseDetailDto?> Handle(GetEnterpriseDetailQuery request, CancellationToken cancellationToken)
    {
        var enterprise = await _enterpriseRepository.GetEnterpriseByIdAsync(request.EnterpriseId.ToString(), cancellationToken);

        if (enterprise == null)
            return null;

        return new EnterpriseDetailDto
        {
            Id = enterprise.Id,
            UserId = enterprise.UserId,
            UserEmail = "N/A",
            UserFullName = "N/A",
            CompanyName = enterprise.CompanyName,
            ServiceArea = enterprise.ServiceArea,
            CapacityKgPerDay = enterprise.CapacityKgPerDay,
            IsVerified = enterprise.IsVerified,
            CreatedAt = enterprise.CreatedAt,
            CollectorCount = enterprise.Collectors?.Count ?? 0,
            WasteTypeCount = enterprise.WasteTypes?.Count ?? 0
        };
    }
}
