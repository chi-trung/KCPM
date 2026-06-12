using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Services;
using Xunit;

namespace WastePlatform.Tests.Infrastructure.Services;

[AllureEpic("KIEM-4: Auth Module Testing")]
[Allure.Net.Commons.Attributes.AllureTag("https://ut-team-36.atlassian.net/browse/KIEM-4")]
[AllureFeature("JwtService")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "JWT token generation and claims verification")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "JwtServiceTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("HoÃ ng Phá»¥ng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("security")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-4")]
public class JwtServiceTests
{
    [Fact]
    [AllureDescription("GenerateToken includes expected claims and expiry")]
    public void GenerateToken_ShouldContainExpectedClaimsAndExpiry()
    {
        // Arrange
        // Táº¡o cáº¥u hÃ¬nh giáº£ trong bá»™ nhá»› Ä‘á»ƒ JwtService Ä‘á»c secret/issuer/audience nhÆ° mÃ´i trÆ°á»ng tháº­t.
        var settings = new System.Collections.Generic.Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
            { "JwtSettings:Issuer", "test-issuer" },
            { "JwtSettings:Audience", "test-audience" },
            { "JwtSettings:ExpirationMinutes", "60" }
        };

        // Build IConfiguration tá»« dá»¯ liá»‡u giáº£ Ä‘á»ƒ khÃ´ng phá»¥ thuá»™c appsettings tháº­t.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        // Khá»Ÿi táº¡o service tháº­t vá»›i config test Ä‘á»ƒ kiá»ƒm tra token sinh ra cÃ³ Ä‘Ãºng format khÃ´ng.
        var jwtService = new JwtService(configuration);
        // Táº¡o user giáº£ Ä‘á»ƒ kiá»ƒm tra cÃ¡c claim Ä‘Æ°á»£c nhÃºng vÃ o token.
        var user = User.Create("user@example.com", "hashedpwd", "Test User", UserRole.Citizen);

        // Act
        var token = jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        // Äá»c token ra object JWT Ä‘á»ƒ assert tá»«ng claim thay vÃ¬ chá»‰ kiá»ƒm tra chuá»—i thÃ´.
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");

        var sub = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        sub.Should().Be(user.Id.ToString());

        var email = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
        email.Should().Be(user.Email);

        var role = jwt.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value;
        role.Should().Be(UserRole.Citizen.ToString());

        // Expiry pháº£i xáº¥p xá»‰ 60 phÃºt vÃ¬ cáº¥u hÃ¬nh test Ä‘áº·t ExpirationMinutes = 60.
        var minutesUntilExpiry = (jwt.ValidTo - DateTime.UtcNow).TotalMinutes;
        minutesUntilExpiry.Should().BeInRange(59, 61);
    }
}

