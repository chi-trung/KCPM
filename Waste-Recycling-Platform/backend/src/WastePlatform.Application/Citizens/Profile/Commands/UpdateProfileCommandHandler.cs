using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Citizens.Profile.DTOs;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Application.Citizens.Profile.Commands
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileDto>
    {
        private readonly IUserRepository _userRepository;

        public UpdateProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var updatedUser = await _userRepository.UpdateProfileAsync(
                request.UserId,
                request.Profile.FullName,
                request.Profile.Phone,
                request.Profile.District,
                request.Profile.Ward,
                cancellationToken);

            return new ProfileDto
            {
                Id = updatedUser.Id,
                Email = updatedUser.Email,
                FullName = updatedUser.FullName,
                Phone = updatedUser.Phone,
                District = updatedUser.District,
                Ward = updatedUser.Ward,
                IsActive = updatedUser.IsActive,
                CreatedAt = updatedUser.CreatedAt,
                UpdatedAt = updatedUser.UpdatedAt
            };
        }
    }
}
