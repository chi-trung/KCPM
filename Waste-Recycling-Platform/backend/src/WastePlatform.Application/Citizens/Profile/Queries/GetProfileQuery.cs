using MediatR;
using WastePlatform.Application.Citizens.Profile.DTOs;

namespace WastePlatform.Application.Citizens.Profile.Queries
{
    public record GetProfileQuery : IRequest<ProfileDto>
    {
        public Guid UserId { get; init; }
    }
}
