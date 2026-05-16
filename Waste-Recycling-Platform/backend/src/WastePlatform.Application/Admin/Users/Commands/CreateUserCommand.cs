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
            // Tạm thời fix cứng password cho user mới tạo là 123456 (đã hash)
            var passwordHash = "hashed_123456"; 

            return await _userRepository.CreateUserAsync(
                request.Email, passwordHash, request.FullName, 
                request.Phone, request.Role, request.District, request.Ward, ct);
        }
    }
}