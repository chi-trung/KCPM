using WastePlatform.Domain.Entities;

namespace WastePlatform.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
