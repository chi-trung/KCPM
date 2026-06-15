using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Services;
using Xunit;

using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Infrastructure.Services;

[AllureEpic("KIEM-4: Auth Module Testing")]
[Allure.Net.Commons.Attributes.AllureTag("https://ut-team-36.atlassian.net/browse/KIEM-4")]
[AllureFeature("JwtService")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "JWT token generation and claims verification")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "JwtServiceTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Hoàng Phụng")]
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
        AllureAttachmentHelper.AttachText("test-g-e-n-e-r-a-t-e-t-o-k-e-n_-s-h-o-u-l-d-c-o-n-t-a-i", "Executed: GenerateToken_ShouldContainExpectedClaimsAndExpiry");
        // Arrange
        // Tạo cấu hình giả trong bộ nhớ để JwtService đọc secret/issuer/audience như môi trường thật.
        var settings = new System.Collections.Generic.Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
            { "JwtSettings:Issuer", "test-issuer" },
            { "JwtSettings:Audience", "test-audience" },
            { "JwtSettings:ExpirationMinutes", "60" }
        };

        // Build IConfiguration từ dữ liệu giả để không phụ thuộc appsettings thật.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        // Khởi tạo service thật với config test để kiểm tra token sinh ra có đúng format không.
        var jwtService = new JwtService(configuration);
        // Tạo user giả để kiểm tra các claim được nhúng vào token.
        var user = User.Create("user@example.com", "hashedpwd", "Test User", UserRole.Citizen);

        // Act
        var token = jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        // Đọc token ra object JWT để assert từng claim thay vì chỉ kiểm tra chuỗi thô.
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");

        var sub = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        sub.Should().Be(user.Id.ToString());

        var email = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
        email.Should().Be(user.Email);

        var role = jwt.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value;
        role.Should().Be(UserRole.Citizen.ToString());

        // Expiry phải xấp xỉ 60 phút vì cấu hình test đặt ExpirationMinutes = 60.
        var minutesUntilExpiry = (jwt.ValidTo - DateTime.UtcNow).TotalMinutes;
        minutesUntilExpiry.Should().BeInRange(59, 61);
    }
}
