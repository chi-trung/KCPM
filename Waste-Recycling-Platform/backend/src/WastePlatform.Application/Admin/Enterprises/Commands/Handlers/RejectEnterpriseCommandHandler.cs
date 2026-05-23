using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Admin.Enterprises.Commands;

namespace WastePlatform.Application.Admin.Enterprises.Commands.Handlers;

public class RejectEnterpriseCommandHandler : IRequestHandler<RejectEnterpriseCommand, RejectEnterpriseResult>
{
    private readonly IEnterpriseRepository _enterpriseRepository;

    public RejectEnterpriseCommandHandler(IEnterpriseRepository enterpriseRepository)
    {
        _enterpriseRepository = enterpriseRepository;
    }

    public async Task<RejectEnterpriseResult> Handle(RejectEnterpriseCommand request, CancellationToken cancellationToken)
    {
        var enterprise = await _enterpriseRepository.GetEnterpriseByIdAsync(request.EnterpriseId.ToString(), cancellationToken);

        if (enterprise == null)
        {
            return new RejectEnterpriseResult
            {
                Success = false,
                Message = "Enterprise not found",
                EnterpriseId = request.EnterpriseId
            };
        }

        // Mark enterprise as not verified (rejected) and set status + rejection reason
        enterprise.IsVerified = false;
        enterprise.Status = "Rejected";
        enterprise.RejectionReason = request.ReasonForRejection;

        // Persist changes to database
        await _enterpriseRepository.UpdateAsync(enterprise, cancellationToken);

        return new RejectEnterpriseResult
        {
            Success = true,
            Message = "Enterprise rejected successfully",
            EnterpriseId = request.EnterpriseId
        };
    }
}
