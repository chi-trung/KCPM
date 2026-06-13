using MediatR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Users.Commands
{
    public class CreateUserCommand : IRequest<string>
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "citizen";
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
    }

    public class CreateUserHandler : IRequestHandler<CreateUserCommand, string>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<string> Handle(CreateUserCommand request, CancellationToken ct)
        {
            // Generate a temporary password and hash it with BCrypt
            var tempPassword = Guid.NewGuid().ToString("N")[..12];
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            return await _userRepository.CreateUserAsync(
                request.Email, passwordHash, request.FullName, 
                request.Phone, request.Role, request.District, request.Ward, ct);
        }
    }
}