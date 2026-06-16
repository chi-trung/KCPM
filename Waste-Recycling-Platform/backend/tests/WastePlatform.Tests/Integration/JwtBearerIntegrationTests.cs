using System.Net;
using System.Net.Http.Headers;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Moq;
using Allure.Xunit.Attributes;
using FluentAssertions;
using Xunit;
using WastePlatform.Infrastructure.Services;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using MediatR;
using WastePlatform.Application.Admin.Enterprises.DTOs;
using WastePlatform.Application.Admin.Enterprises.Queries;

using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Integration
{
    [AllureEpic("KIEM-21: Security & Role-based Access Tests")]
    [AllureFeature("JWT Bearer Authentication")]
    [AllureOwner("Hoàng Phụng")]
    [AllureSeverity(Allure.Net.Commons.SeverityLevel.blocker)]
    [Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-21")]
    [Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
    [Allure.Net.Commons.Attributes.AllureLabel("suite", "Integration")]
    [Allure.Net.Commons.Attributes.AllureLabel("subSuite", "JwtBearerIntegrationTests")]
    [Allure.Net.Commons.Attributes.AllureTag("security", "jwt", "integration", "backend")]
    public class JwtBearerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public JwtBearerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ValidSignedToken_AllowsAccess_ToProtectedEndpoint()
        {
        AllureAttachmentHelper.AttachText("test-v-a-l-i-d-s-i-g-n-e-d-t-o-k-e-n_-a-l-l-o-w-s-a-c-c", "Executed: ValidSignedToken_AllowsAccess_ToProtectedEndpoint");
            var seededUserId = System.Guid.NewGuid();

            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
                    {
                        { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                        { "JwtSettings:Issuer", "test-issuer" },
                        { "JwtSettings:Audience", "test-audience" },
                        { "JwtSettings:ExpirationMinutes", "60" }
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                    services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                        options.UseInMemoryDatabase("JwtBearerIntegrationTestDb"));

                    // Mock mediator to avoid depending on handlers
                    var mediatorMock = new Mock<IMediator>();
                    var dummy = new System.Collections.Generic.List<EnterpriseListDto>();
                    var tuple = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)dummy, 0, 0);
                    mediatorMock.Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                        .ReturnsAsync(tuple);
                    services.AddSingleton<IMediator>(mediatorMock.Object);

                    // Configure the existing JwtBearer scheme in the test host to use the test key
                    services.Configure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                        Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = "test-issuer",
                                ValidAudience = "test-audience",
                                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("test-secret-key-which-is-long-enough"))
                            };
                        });

                    // Seed a user into the in-memory DB
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>();
                        db.Database.EnsureCreated();
                        var user = User.Create("jwtuser@example.com", "pwd", "JWT User", UserRole.Admin);
                        var idField = typeof(User).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        if (idField != null)
                            idField.SetValue(user, seededUserId);
                        db.Users.Add(user);
                        db.SaveChanges();
                    }
                });
            });

            var client = factory.CreateClient();

            // Generate a signed token using the same JwtService configuration
            var config = new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                { "JwtSettings:Issuer", "test-issuer" },
                { "JwtSettings:Audience", "test-audience" },
                { "JwtSettings:ExpirationMinutes", "60" }
            }).Build();

            var jwtService = new JwtService(config);
            var tokenUser = User.Create("jwtuser@example.com", "pwd", "JWT User", UserRole.Admin);
            var idFieldToken = typeof(User).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (idFieldToken != null)
                idFieldToken.SetValue(tokenUser, seededUserId);

            var token = jwtService.GenerateToken(tokenUser);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/admin/enterprises");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
