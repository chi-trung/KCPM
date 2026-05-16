using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Admin.Enterprises.Commands;

namespace WastePlatform.Application.Admin.Enterprises.Commands.Handlers;

public class VerifyEnterpriseCommandHandler : IRequestHandler<VerifyEnterpriseCommand, VerifyEnterpriseResult>
{
    private readonly IEnterpriseRepository _enterpriseRepository;

    public VerifyEnterpriseCommandHandler(IEnterpriseRepository enterpriseRepository)
    {
        _enterpriseRepository = enterpriseRepository;
    }

    public async Task<VerifyEnterpriseResult> Handle(VerifyEnterpriseCommand request, CancellationToken cancellationToken)
    {
        var enterprise = await _enterpriseRepository.GetEnterpriseByIdAsync(request.EnterpriseId.ToString(), cancellationToken);

        if (enterprise == null)
        {
            return new VerifyEnterpriseResult
            {
                Success = false,
                Message = "Enterprise not found",
                EnterpriseId = request.EnterpriseId
            };
        }

        if (enterprise.IsVerified)
        {
            return new VerifyEnterpriseResult
            {
                Success = false,
                Message = "Enterprise is already verified",
                EnterpriseId = request.EnterpriseId
            };
        }

        // Update both old and new status fields
        enterprise.IsVerified = true;
        enterprise.Status = "Verified";
        enterprise.RejectionReason = null; // Clear any previous rejection reason

        // Persist changes to database
        await _enterpriseRepository.UpdateAsync(enterprise, cancellationToken);

        return new VerifyEnterpriseResult
        {
            Success = true,
            Message = "Enterprise verified successfully",
            EnterpriseId = request.EnterpriseId
        };
    }
}
