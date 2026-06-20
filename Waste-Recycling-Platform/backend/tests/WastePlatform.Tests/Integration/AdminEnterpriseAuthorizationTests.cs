using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Allure.Xunit.Attributes;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Moq;
using WastePlatform.Application.Admin.Enterprises.Queries;
using WastePlatform.Application.Admin.Enterprises.DTOs;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Services;
using WastePlatform.Tests.TestSupport;
using Xunit;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Infrastructure.SignalR;

namespace WastePlatform.Tests.Integration;

[AllureEpic("KIEM-21: Security & Role-based Access Tests")]
[Allure.Net.Commons.Attributes.AllureTag("KIEM-21")]
[AllureFeature("Admin Enterprise Controller Authorization")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Role-based authorization for admin enterprise endpoints")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Integration")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminEnterpriseAuthorizationTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Integration")]
[AllureOwner("HoÃ ng Phá»¥ng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("integration")]
[Allure.Net.Commons.Attributes.AllureTag("security")]
[Allure.Net.Commons.Attributes.AllureIssue("KIEM-21")]
public class AdminEnterpriseAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminEnterpriseAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    [AllureDescription("GET /api/admin/enterprises without Authorization header should return 401 Unauthorized.")]
    public async System.Threading.Tasks.Task GetEnterprises_WithoutToken_ReturnsUnauthorized()
    {

        // Arrange: use test factory with in-memory DB and test auth
        // Táº¡o host test riÃªng Ä‘á»ƒ thay DB tháº­t báº±ng InMemory DB vÃ  trÃ¡nh cháº¡m MySQL.
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                // Ghi Ä‘Ã¨ JWT settings Ä‘á»ƒ má»i test dÃ¹ng cÃ¹ng má»™t bá»™ secret/issuer/audience.
                var settings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                };

                // ÄÆ°a config test vÃ o pipeline thay cho config tháº­t cá»§a á»©ng dá»¥ng.
                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                // XÃ³a DbContext gá»‘c vÃ  thay báº±ng InMemory Ä‘á»ƒ test khÃ´ng cáº§n database ngoÃ i.
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_NoToken"));

                // Mock IMediator Ä‘á»ƒ controller tráº£ dá»¯ liá»‡u giáº£, trÃ¡nh phá»¥ thuá»™c handler tháº­t.
                var mediatorMock = new Mock<IMediator>();
                // Káº¿t quáº£ rá»—ng Ä‘áº¡i diá»‡n cho trÆ°á»ng há»£p khÃ´ng cÃ³ enterprise nÃ o Ä‘Æ°á»£c tráº£ vá».
                var emptyResult = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)new System.Collections.Generic.List<EnterpriseListDto>(), 0, 0);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(emptyResult);

                services.AddSingleton<IMediator>(mediatorMock.Object);

                // ÄÄƒng kÃ½ auth scheme test Ä‘á»ƒ Ä‘á»c claims tá»« token mÃ  khÃ´ng cáº§n verify signature.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        var client = factory.CreateClient();

        // Act
        // KhÃ´ng gá»­i header Authorization nÃªn middleware pháº£i cháº·n vÃ  tráº£ vá» 401.
        var response = await client.GetAsync("/api/admin/enterprises");

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode} Unauthorized");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [AllureDescription("GET /api/admin/enterprises with Citizen role JWT token should return 403 Forbidden.")]
    public async System.Threading.Tasks.Task GetEnterprises_WithCitizenToken_ReturnsForbidden()
    {
        // Arrange: configure factory to use test jwt settings
        // Dá»±ng host test riÃªng Ä‘á»ƒ mÃ´ phá»ng request cÃ³ token Citizen.
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                // CÃ¹ng bá»™ JWT config test Ä‘á»ƒ token sinh ra vÃ  handler Ä‘á»c token khá»›p nhau.
                var settings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                };

                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                // replace real DB context with in-memory for tests
                // Äá»•i sang DB áº£o Ä‘á»ƒ controller/hub/middleware khÃ´ng phá»¥ thuá»™c háº¡ táº§ng tháº­t.
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_Citizen"));

                // Replace IMediator with a mock to avoid DB calls when admin is used
                // Vá»›i test nÃ y, mediator chá»‰ cáº§n tráº£ dá»¯ liá»‡u máº«u Ä‘á»ƒ bÆ°á»›c authorization Ä‘i qua.
                var mediatorMock = new Mock<IMediator>();
                // Tráº£ vá» danh sÃ¡ch rá»—ng vÃ¬ má»¥c tiÃªu chÃ­nh lÃ  kiá»ƒm tra quyá»n truy cáº­p, khÃ´ng pháº£i business data.
                var emptyResult = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)new System.Collections.Generic.List<EnterpriseListDto>(), 0, 0);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(emptyResult);

                services.AddSingleton<IMediator>(mediatorMock.Object);

                // Replace authentication with a test scheme that reads JWT claims without validating signature
                // Scheme nÃ y chá»‰ giáº£i mÃ£ claim tá»« token Ä‘á»ƒ assert role-based authorization.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        var client = factory.CreateClient();

        // create a citizen token
        // Sinh token cÃ³ role Citizen Ä‘á»ƒ xÃ¡c nháº­n endpoint admin pháº£i tá»« chá»‘i truy cáº­p.
        var jwtService = new JwtService(new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
            { "JwtSettings:Issuer", "test-issuer" },
            { "JwtSettings:Audience", "test-audience" },
            { "JwtSettings:ExpirationMinutes", "60" }
        }).Build());

        var citizenUser = User.Create("citizen@example.com", "pwd", "Citizen", UserRole.Citizen);
        var token = jwtService.GenerateToken(citizenUser);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        // CÃ³ token nhÆ°ng role khÃ´ng Ä‘á»§ nÃªn middleware/authorize pháº£i tráº£ 403.
        var response = await client.GetAsync("/api/admin/enterprises");

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode} Forbidden");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [AllureDescription("GET /api/admin/enterprises with valid Admin role JWT token should return 200 OK.")]
    public async System.Threading.Tasks.Task GetEnterprises_WithAdminToken_ReturnsOk()
    {
        // Arrange: configure factory to use test jwt settings and mock mediator
        // ÄÃ¢y lÃ  case happy path: token Admin há»£p lá»‡ vÃ  endpoint pháº£i cho phÃ©p gá»i.
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                // TÃ¡i sá»­ dá»¥ng cÃ¹ng bá»™ config JWT test Ä‘á»ƒ sinh vÃ  Ä‘á»c token thá»‘ng nháº¥t.
                var settings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                };

                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                // replace real DB context with in-memory for tests
                // KhÃ´ng cáº§n DB tháº­t cho test authorization nÃªn dÃ¹ng InMemory Ä‘á»ƒ cÃ´ láº­p mÃ´i trÆ°á»ng.
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_Admin"));

                // Mock danh sÃ¡ch enterprise Ä‘á»ƒ controller cÃ³ dá»¯ liá»‡u tráº£ vá» khi qua Ä‘Æ°á»£c bÆ°á»›c auth.
                var mediatorMock = new Mock<IMediator>();

                var dummyList = new System.Collections.Generic.List<EnterpriseListDto>
                {
                    new EnterpriseListDto { Id = System.Guid.NewGuid(), CompanyName = "TestCo", IsVerified = true, ServiceArea = "Area", CreatedAt = System.DateTime.UtcNow }
                };

                // Tuple mÃ´ phá»ng Ä‘Ãºng shape return cá»§a GetEnterprisesQuery handler.
                var resultTuple = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)dummyList, dummyList.Count, 1);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(resultTuple);

                services.AddSingleton<IMediator>(mediatorMock.Object);

                // DÃ¹ng test auth scheme Ä‘á»ƒ endpoint nháº­n ra role Admin tá»« token máº«u.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        var client = factory.CreateClient();

        // create an admin token
        // Token Admin pháº£i Ä‘i qua authorize thÃ nh cÃ´ng vÃ  nháº­n Ä‘Æ°á»£c response 200.
        var jwtService = new JwtService(new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
            { "JwtSettings:Issuer", "test-issuer" },
            { "JwtSettings:Audience", "test-audience" },
            { "JwtSettings:ExpirationMinutes", "60" }
        }).Build());

        var adminUser = User.Create("admin@example.com", "pwd", "Admin User", UserRole.Admin);
        var token = jwtService.GenerateToken(adminUser);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        // Request vá»›i role Admin há»£p lá»‡ nÃªn controller pháº£i tráº£ OK.
        var response = await client.GetAsync("/api/admin/enterprises");

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode} OK");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Enterprises retrieved successfully");
    }

    [Fact]
    [AllureDescription("GET /api/admin/enterprises with an expired JWT token should return 401 Unauthorized.")]
    public async System.Threading.Tasks.Task GetEnterprises_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange: use real JwtBearer validation (do not replace auth scheme) and in-memory DB
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                // Expired token: set negative expiration so generated token is already expired
                var settings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationMinutes", "-60" }
                };
                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                // Replace DB with InMemory to avoid MySQL
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_ExpiredToken"));

                // Mock mediator so controller won't fail if authorization passed
                var mediatorMock = new Mock<IMediator>();
                var emptyResult = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)new System.Collections.Generic.List<EnterpriseListDto>(), 0, 0);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(emptyResult);
                services.AddSingleton<IMediator>(mediatorMock.Object);

                // IMPORTANT: do NOT replace the authentication scheme here so JwtBearer from Program is used.
            });
        });

        var client = factory.CreateClient();

        // create an expired token using JwtService (uses test config with negative expiry)
        var jwtService = new JwtService(new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
            { "JwtSettings:Issuer", "test-issuer" },
            { "JwtSettings:Audience", "test-audience" },
            { "JwtSettings:ExpirationMinutes", "-60" }
        }).Build());

        var user = User.Create("expired@example.com", "pwd", "Expired User", UserRole.Admin);
        var token = jwtService.GenerateToken(user);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/admin/enterprises");

        // Assert - expired token should be rejected by JwtBearer middleware
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode} Unauthorized");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [AllureDescription("GET /api/admin/enterprises with a malformed non-JWT token string should return 401 Unauthorized.")]
    public async System.Threading.Tasks.Task GetEnterprises_WithMalformedToken_ReturnsUnauthorized()
    {
        // Arrange: use real JwtBearer validation
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var settings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                };
                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_MalformedToken"));

                var mediatorMock = new Mock<IMediator>();
                var emptyResult = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)new System.Collections.Generic.List<EnterpriseListDto>(), 0, 0);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(emptyResult);
                services.AddSingleton<IMediator>(mediatorMock.Object);

                // Keep real JwtBearer from Program
            });
        });

        var client = factory.CreateClient();

        // malformed token string (not a JWT)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-valid.jwt.token");

        // Act
        var response = await client.GetAsync("/api/admin/enterprises");

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode} Unauthorized");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [AllureDescription("GET /api/enterprise/analytics/reports with Citizen role JWT token should return 403 Forbidden.")]
    public async System.Threading.Tasks.Task EnterpriseEndpoint_WithCitizenToken_ReturnsForbidden()
    {
        // Arrange: test auth handler to parse token claims and InMemory DB; target enterprise endpoint
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var settings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                };
                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_EnterpriseRole"));

                // No IMediator mock required: authorization will block before controller executes.

                // Register test auth handler to bypass signature validation and focus on role check
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        var client = factory.CreateClient();

        var jwtService = new JwtService(new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
            { "JwtSettings:Issuer", "test-issuer" },
            { "JwtSettings:Audience", "test-audience" },
            { "JwtSettings:ExpirationMinutes", "60" }
        }).Build());

        var citizenUser = User.Create("citizen-for-enterprise@example.com", "pwd", "Citizen", UserRole.Citizen);
        var token = jwtService.GenerateToken(citizenUser);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/enterprise/analytics/reports");

        // Assert - user with Citizen role should be forbidden from Enterprise endpoints
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode} Forbidden");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [AllureDescription("GET /api/admin/enterprises with a real JwtBearer-validated Admin token should return 200 OK.")]
    public async System.Threading.Tasks.Task GetEnterprises_WithRealJwtBearerAdmin_ReturnsOk()
    {
        // Arrange: use real JwtBearer (do not replace authentication scheme), seed InMemory DB with admin user
        var seededUserId = Guid.NewGuid();
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var settings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                };
                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                // Replace real DB with InMemory and seed admin user
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_RealJwt_Admin"));

                // Mock mediator to return dummy data once authorization passes
                var mediatorMock = new Mock<IMediator>();
                var dummyList = new System.Collections.Generic.List<EnterpriseListDto>
                {
                    new EnterpriseListDto { Id = System.Guid.NewGuid(), CompanyName = "RealJwtCo", IsVerified = true, ServiceArea = "Area", CreatedAt = System.DateTime.UtcNow }
                };
                var resultTuple = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)dummyList, dummyList.Count, 1);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(resultTuple);
                services.AddSingleton<IMediator>(mediatorMock.Object);

                // Seed admin user into the InMemory DB after provider is built
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>();
                    db.Database.EnsureCreated();
                    var adminUser = User.Create("realadmin@example.com", "pwd", "Real Admin", UserRole.Admin);
                    // Set private backing field for Id to a known value so token subject can match
                    var idField = typeof(User).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (idField != null)
                        idField.SetValue(adminUser, seededUserId);

                    db.Users.Add(adminUser);
                    db.SaveChanges();
                }
                    // Configure the existing JwtBearer scheme to use our test signing key/issuer/audience
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
                // IMPORTANT: do NOT override authentication scheme here - let JwtBearer from Program run
            });
        });

        var client = factory.CreateClient();

        // create a signed admin token using JwtService with same config
        var jwtConfig = new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
            { "JwtSettings:Issuer", "test-issuer" },
            { "JwtSettings:Audience", "test-audience" },
            { "JwtSettings:ExpirationMinutes", "60" }
        }).Build();

        var jwtService = new JwtService(jwtConfig);

        // Create token for user with the same seeded Id
        var adminUserForToken = User.Create("realadmin@example.com", "pwd", "Real Admin", UserRole.Admin);
        var idFieldToken = typeof(User).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (idFieldToken != null)
            idFieldToken.SetValue(adminUserForToken, seededUserId);

        var token = jwtService.GenerateToken(adminUserForToken);

        // Sanity-check: validate token locally with same validation parameters to catch issues early
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes("test-secret-key-which-is-long-enough");
        var validationParams = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "test-issuer",
            ValidAudience = "test-audience",
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key)
        };

        System.Security.Claims.ClaimsPrincipal validated;
        try
        {
            validated = handler.ValidateToken(token, validationParams, out var validatedToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Local token validation failed: {ex.Message}");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/admin/enterprises");

        // Assert - with real JwtBearer and seeded active admin user, request should succeed
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            var www = string.Join(";", response.Headers.WwwAuthenticate.Select(h => h.ToString()));
            throw new Exception($"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}; WWW-Authenticate: {www}; Body: {body}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Enterprises retrieved successfully");
    }

    private HttpClient CreateTestClient(string? role, string dbName, Action<IServiceCollection>? configureServices = null)
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var settings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                };
                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                // Mock IMediator by default
                var mediatorMock = new Mock<IMediator>();
                services.AddSingleton<IMediator>(mediatorMock.Object);

                // Mock INotificationService
                var notificationMock = new Mock<INotificationService>();
                services.AddSingleton<INotificationService>(notificationMock.Object);

                // Mock IHubContext<TaskHub>
                var hubContextMock = new Mock<IHubContext<TaskHub>>();
                var clientsMock = new Mock<IHubClients>();
                var clientProxyMock = new Mock<IClientProxy>();
                clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
                hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);
                services.AddSingleton<IHubContext<TaskHub>>(hubContextMock.Object);

                configureServices?.Invoke(services);

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        // Let's seed the User and Enterprise if role is provided
        Guid userId = Guid.NewGuid();
        User? user = null;

        if (role != null)
        {
            UserRole userRole = role switch
            {
                "Admin" => UserRole.Admin,
                "Enterprise" => UserRole.Enterprise,
                "Citizen" => UserRole.Citizen,
                "Collector" => UserRole.Collector,
                _ => throw new System.ArgumentException("Invalid role")
            };

            user = User.Create($"{role.ToLower()}@example.com", "pwd", $"{role} User", userRole);
            var idField = typeof(User).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (idField != null)
                idField.SetValue(user, userId);
        }

        var client = factory.CreateClient();

        if (role != null && user != null)
        {
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>();
                db.Database.EnsureCreated();
                db.Users.Add(user);

                if (role == "Enterprise")
                {
                    var enterprise = new Enterprise
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        CompanyName = "Test Enterprise",
                        ServiceArea = "[\"District 1\"]",
                        CapacityKgPerDay = 1000,
                        Status = "Verified",
                        CreatedAt = System.DateTime.UtcNow
                    };
                    db.Enterprises.Add(enterprise);
                }
                db.SaveChanges();
            }

            var jwtService = new JwtService(new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                { "JwtSettings:Issuer", "test-issuer" },
                { "JwtSettings:Audience", "test-audience" },
                { "JwtSettings:ExpirationMinutes", "60" }
            }).Build());

            var token = jwtService.GenerateToken(user);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    // ==========================================
    // KIEM-21: ROLE-BASED ACCESS CONTROL TESTS
    // ==========================================

    #region Admin-only endpoints
    [Fact]
    [AllureFeature("Admin User Creation")]
    [AllureStory("Admin can create user, others forbidden")]
    public async System.Threading.Tasks.Task CreateUser_AdminRole_ReturnsOk()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<WastePlatform.Application.Admin.Users.Commands.CreateUserCommand>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync("new-user-id");

        var client = CreateTestClient("Admin", "Db_CreateUser_Admin", services =>
        {
            services.AddSingleton<IMediator>(mockMediator.Object);
        });

        var command = new WastePlatform.Application.Admin.Users.Commands.CreateUserCommand
        {
            Email = "newcitizen@example.com",
            FullName = "New Citizen",
            Phone = "0987654321",
            Role = "citizen",
            District = "District 1",
            Ward = "Ward 1"
        };
        var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/admin/users", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [AllureFeature("Admin User Creation")]
    [AllureStory("Citizen/Enterprise cannot create user")]
    public async System.Threading.Tasks.Task CreateUser_NonAdminRole_ReturnsForbidden()
    {
        // Arrange
        var client = CreateTestClient("Citizen", "Db_CreateUser_NonAdmin");
        var command = new WastePlatform.Application.Admin.Users.Commands.CreateUserCommand();
        var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/admin/users", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [AllureFeature("Admin User Management")]
    [AllureStory("Admin can toggle status, others forbidden")]
    public async System.Threading.Tasks.Task ToggleUserStatus_AdminRole_ReturnsOk()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<WastePlatform.Application.Admin.Users.Commands.ToggleUserStatusCommand>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(true);

        var client = CreateTestClient("Admin", "Db_ToggleStatus_Admin", services =>
        {
            services.AddSingleton<IMediator>(mockMediator.Object);
        });

        // Act
        var response = await client.PatchAsync($"/api/admin/users/{Guid.NewGuid()}/toggle-status", null);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [AllureFeature("Admin User Management")]
    [AllureStory("Citizen/Enterprise cannot toggle user status")]
    public async System.Threading.Tasks.Task ToggleUserStatus_NonAdminRole_ReturnsForbidden()
    {
        // Arrange
        var client = CreateTestClient("Citizen", "Db_ToggleStatus_NonAdmin");

        // Act
        var response = await client.PatchAsync($"/api/admin/users/{Guid.NewGuid()}/toggle-status", null);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [AllureFeature("Admin Analytics")]
    [AllureStory("Admin can view overview, others forbidden")]
    public async System.Threading.Tasks.Task GetAnalyticsOverview_AdminRole_ReturnsOk()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<WastePlatform.Application.Admin.Analytics.Queries.GetAnalyticsOverviewQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new WastePlatform.Application.Admin.Analytics.DTOs.AnalyticsOverviewDto());

        var client = CreateTestClient("Admin", "Db_AnalyticsOverview_Admin", services =>
        {
            services.AddSingleton<IMediator>(mockMediator.Object);
        });

        // Act
        var response = await client.GetAsync("/api/admin/analytics/overview");

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [AllureFeature("Admin Analytics")]
    [AllureStory("Citizen/Enterprise cannot view analytics overview")]
    public async System.Threading.Tasks.Task GetAnalyticsOverview_NonAdminRole_ReturnsForbidden()
    {
        // Arrange
        var client = CreateTestClient("Citizen", "Db_AnalyticsOverview_NonAdmin");

        // Act
        var response = await client.GetAsync("/api/admin/analytics/overview");

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    #endregion

    #region Enterprise-only endpoints
    [Fact]
    [AllureFeature("Enterprise Collectors")]
    [AllureStory("Enterprise can create collector, others forbidden")]
    public async System.Threading.Tasks.Task CreateCollector_EnterpriseRole_ReturnsOk()
    {
        // Arrange
        var client = CreateTestClient("Enterprise", "Db_CreateCollector_Enterprise");

        var request = new CreateEnterpriseCollectorRequest
        {
            FullName = "New Collector",
            Email = "newcollector@example.com",
            Phone = "0981112222",
            TemporaryPassword = "password123",
            IsAvailable = true
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/enterprise/collectors", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [AllureFeature("Enterprise Collectors")]
    [AllureStory("Citizen/Admin cannot create collector")]
    public async System.Threading.Tasks.Task CreateCollector_NonEnterpriseRole_ReturnsForbidden()
    {
        // Arrange
        var client = CreateTestClient("Citizen", "Db_CreateCollector_NonEnterprise");
        var request = new CreateEnterpriseCollectorRequest { IsAvailable = true };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/enterprise/collectors", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [AllureFeature("Enterprise Tasks")]
    [AllureStory("Enterprise can assign task to collector")]
    public async System.Threading.Tasks.Task AssignCollector_EnterpriseRole_ReturnsOkOrNotFound()
    {
        // Arrange
        var client = CreateTestClient("Enterprise", "Db_AssignCollector_Enterprise");
        var request = new AssignCollectorRequest
        {
            CollectorId = Guid.NewGuid()
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/enterprise/tasks/{Guid.NewGuid()}/assign-collector", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [AllureFeature("Enterprise Tasks")]
    [AllureStory("Admin can assign task to collector")]
    public async System.Threading.Tasks.Task AssignCollector_AdminRole_ReturnsOkOrNotFound()
    {
        // Arrange
        var client = CreateTestClient("Admin", "Db_AssignCollector_Admin");
        var request = new AssignCollectorRequest
        {
            CollectorId = Guid.NewGuid()
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/enterprise/tasks/{Guid.NewGuid()}/assign-collector", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [AllureFeature("Enterprise Tasks")]
    [AllureStory("Citizen cannot assign task to collector")]
    public async System.Threading.Tasks.Task AssignCollector_CitizenRole_ReturnsForbidden()
    {
        // Arrange
        var client = CreateTestClient("Citizen", "Db_AssignCollector_Citizen");
        var request = new AssignCollectorRequest
        {
            CollectorId = Guid.NewGuid()
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/enterprise/tasks/{Guid.NewGuid()}/assign-collector", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    #endregion

    #region Citizen-only endpoints
    [Fact]
    [AllureFeature("Citizen Reports")]
    [AllureStory("Citizen can create report")]
    public async System.Threading.Tasks.Task CreateReport_CitizenRole_ReturnsCreated()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<WastePlatform.Application.Reports.Commands.CreateReportCommand>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        mockMediator.Setup(m => m.Send(It.IsAny<WastePlatform.Application.Reports.Queries.GetReportByIdQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new WastePlatform.Application.Common.DTOs.ReportDto());

        var client = CreateTestClient("Citizen", "Db_CreateReport_Citizen", services =>
        {
            services.AddSingleton<IMediator>(mockMediator.Object);
        });

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("1"), "WasteCategoryId");
        content.Add(new StringContent("10.5"), "Latitude");
        content.Add(new StringContent("106.3"), "Longitude");
        content.Add(new StringContent("Report description"), "Description");
        content.Add(new StringContent("Report address"), "Address");

        // Act
        var response = await client.PostAsync("/api/reports/create", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    [AllureFeature("Citizen Reports")]
    [AllureStory("Admin/Enterprise cannot create report")]
    public async System.Threading.Tasks.Task CreateReport_NonCitizenRole_ReturnsForbidden()
    {
        // Arrange
        var client = CreateTestClient("Admin", "Db_CreateReport_NonCitizen");
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("1"), "WasteCategoryId");

        // Act
        var response = await client.PostAsync("/api/reports/create", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [AllureFeature("Citizen Complaints")]
    [AllureStory("Citizen can create complaint")]
    public async System.Threading.Tasks.Task CreateComplaint_CitizenRole_ReturnsCreated()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<WastePlatform.Application.Complaints.Commands.CreateComplaintCommand>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        mockMediator.Setup(m => m.Send(It.IsAny<WastePlatform.Application.Complaints.Queries.GetComplaintByIdQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new WastePlatform.Application.Common.DTOs.ComplaintDto { Content = "Test" });

        var client = CreateTestClient("Citizen", "Db_CreateComplaint_Citizen", services =>
        {
            services.AddSingleton<IMediator>(mockMediator.Object);
        });

        var dto = new WastePlatform.Application.Common.DTOs.CreateComplaintDto
        {
            Content = "Valid complaint content that is not empty.",
            ReportId = Guid.NewGuid()
        };
        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/complaints", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    [AllureFeature("Citizen Complaints")]
    [AllureStory("Admin/Enterprise cannot create complaint")]
    public async System.Threading.Tasks.Task CreateComplaint_NonCitizenRole_ReturnsForbidden()
    {
        // Arrange
        var client = CreateTestClient("Admin", "Db_CreateComplaint_NonCitizen");
        var dto = new WastePlatform.Application.Common.DTOs.CreateComplaintDto
        {
            Content = "Valid content",
            ReportId = Guid.NewGuid()
        };
        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/complaints", content);

        // Assert
        AllureAttachmentHelper.AttachText("http-response", $"HTTP response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    #endregion
}

[Allure.Net.Commons.Attributes.AllureIssue("KIEM-21")]
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Nếu không có header Authorization thì coi như request chưa đăng nhập.
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaders))
            return Task.FromResult(AuthenticateResult.NoResult());

        var authHeader = authHeaders.FirstOrDefault();
        // Chỉ chấp nhận kiểu Bearer token vì đây là flow của JWT auth.
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Task.FromResult(AuthenticateResult.NoResult());

        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            // Chỉ đọc claim từ JWT để phục vụ test authorization, không verify chữ ký.
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var claims = jwt.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();
            
            // Map "sub" to ClaimTypes.NameIdentifier to allow User.FindFirst(ClaimTypes.NameIdentifier) to succeed
            if (claims.Any(c => c.Type == JwtRegisteredClaimNames.Sub) && !claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                var subClaim = claims.First(c => c.Type == JwtRegisteredClaimNames.Sub);
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
            }

            // Tạo principal test từ claim của JWT để ASP.NET Core áp dụng [Authorize(Roles=...)]
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            // Token sai Ä‘á»‹nh dáº¡ng sáº½ bá»‹ coi lÃ  auth fail.
            return Task.FromResult(AuthenticateResult.Fail(ex));
        }
    }
}


