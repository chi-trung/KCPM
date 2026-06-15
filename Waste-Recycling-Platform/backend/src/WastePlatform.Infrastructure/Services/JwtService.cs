using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var jwtSection = _configuration.GetSection("JwtSettings");
        var signingKey  = jwtSection.GetValue<string>("SecretKey")
                         ?? throw new InvalidOperationException("JWT signing key is not configured. Set the JwtSettings:SecretKey environment variable.");
        var issuer     = jwtSection.GetValue<string>("Issuer") ?? "waste-platform";
        var audience   = jwtSection.GetValue<string>("Audience") ?? "waste-platform-users";
        var expMinutes = jwtSection.GetValue<int?>("ExpirationMinutes") ?? 60;

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim("fullName",                    user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
