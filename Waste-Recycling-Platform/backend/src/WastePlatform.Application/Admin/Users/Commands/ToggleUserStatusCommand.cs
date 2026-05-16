using MediatR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Users.Commands
{
    public class ToggleUserStatusCommand : IRequest<bool>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class ToggleUserStatusHandler : IRequestHandler<ToggleUserStatusCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public ToggleUserStatusHandler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<bool> Handle(ToggleUserStatusCommand request, CancellationToken ct)
        {
            return await _userRepository.ToggleUserStatusAsync(request.UserId, ct);
        }
    }
}