using MediatR;
using WastePlatform.Application.Citizens.Profile.DTOs;

namespace WastePlatform.Application.Citizens.Profile.Commands
{
    public record UpdateProfileCommand : IRequest<ProfileDto>
    {
        public Guid UserId { get; init; }
        public UpdateProfileDto Profile { get; init; } = new();
    }
}
