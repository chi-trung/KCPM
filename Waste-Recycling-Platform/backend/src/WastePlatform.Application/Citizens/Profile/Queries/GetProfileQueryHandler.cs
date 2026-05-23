using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Citizens.Profile.DTOs;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Application.Citizens.Profile.Queries
{
    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileDto>
    {
        private readonly IUserRepository _userRepository;

        public GetProfileQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId, cancellationToken);
            
            if (user == null)
                throw new KeyNotFoundException($"User with ID {request.UserId} not found");

            return new ProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                District = user.District,
                Ward = user.Ward,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
