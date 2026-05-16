using MediatR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Users.Commands
{
    public class UpdateUserRoleCommand : IRequest<bool>
    {
        public string UserId { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
    }

    public class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserRoleHandler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken ct)
        {
            return await _userRepository.UpdateUserRoleAsync(request.UserId, request.NewRole, ct);
        }
    }
}