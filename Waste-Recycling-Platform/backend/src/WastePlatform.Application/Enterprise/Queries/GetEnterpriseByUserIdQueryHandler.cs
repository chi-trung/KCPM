using MediatR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Enterprise.Queries;

public class GetEnterpriseByUserIdQueryHandler : IRequestHandler<GetEnterpriseByUserIdQuery, EnterpriseDto?>
{
    private readonly IEnterpriseRepository _enterpriseRepository;

    public GetEnterpriseByUserIdQueryHandler(IEnterpriseRepository enterpriseRepository)
    {
        _enterpriseRepository = enterpriseRepository;
    }

    public async Task<EnterpriseDto?> Handle(GetEnterpriseByUserIdQuery request, CancellationToken cancellationToken)
    {
        return await _enterpriseRepository.GetEnterpriseByUserIdAsync(request.UserId, cancellationToken);
    }
}
