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
using Xunit;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace WastePlatform.Tests.Integration;

[AllureEpic("KIEM-21: Security & Role-based Access Tests")]
[Allure.Net.Commons.Attributes.AllureTag("https://ut-team-36.atlassian.net/browse/KIEM-21")]
[AllureFeature("Admin Enterprise Controller Authorization")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Role-based authorization for admin enterprise endpoints")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Integration")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminEnterpriseAuthorizationTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Integration")]
[AllureOwner("Hoàng Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("integration")]
[Allure.Net.Commons.Attributes.AllureTag("security")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-21")]
public class AdminEnterpriseAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminEnterpriseAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEnterprises_WithoutToken_ReturnsUnauthorized()
    {

        // Arrange: use test factory with in-memory DB and test auth
        // Tạo host test riêng để thay DB thật bằng InMemory DB và tránh chạm MySQL.
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                // Ghi đè JWT settings để mọi test dùng cùng một bộ secret/issuer/audience.
                var settings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                };

                // Đưa config test vào pipeline thay cho config thật của ứng dụng.
                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                // Xóa DbContext gốc và thay bằng InMemory để test không cần database ngoài.
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_NoToken"));

                // Mock IMediator để controller trả dữ liệu giả, tránh phụ thuộc handler thật.
                var mediatorMock = new Mock<IMediator>();
                // Kết quả rỗng đại diện cho trường hợp không có enterprise nào được trả về.
                var emptyResult = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)new System.Collections.Generic.List<EnterpriseListDto>(), 0, 0);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(emptyResult);

                services.AddSingleton<IMediator>(mediatorMock.Object);

                // Đăng ký auth scheme test để đọc claims từ token mà không cần verify signature.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        var client = factory.CreateClient();

        // Act
        // Không gửi header Authorization nên middleware phải chặn và trả về 401.
        var response = await client.GetAsync("/api/admin/enterprises");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEnterprises_WithCitizenToken_ReturnsForbidden()
    {
        // Arrange: configure factory to use test jwt settings
        // Dựng host test riêng để mô phỏng request có token Citizen.
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                // Cùng bộ JWT config test để token sinh ra và handler đọc token khớp nhau.
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
                // Đổi sang DB ảo để controller/hub/middleware không phụ thuộc hạ tầng thật.
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_Citizen"));

                // Replace IMediator with a mock to avoid DB calls when admin is used
                // Với test này, mediator chỉ cần trả dữ liệu mẫu để bước authorization đi qua.
                var mediatorMock = new Mock<IMediator>();
                // Trả về danh sách rỗng vì mục tiêu chính là kiểm tra quyền truy cập, không phải business data.
                var emptyResult = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)new System.Collections.Generic.List<EnterpriseListDto>(), 0, 0);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(emptyResult);

                services.AddSingleton<IMediator>(mediatorMock.Object);

                // Replace authentication with a test scheme that reads JWT claims without validating signature
                // Scheme này chỉ giải mã claim từ token để assert role-based authorization.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        var client = factory.CreateClient();

        // create a citizen token
        // Sinh token có role Citizen để xác nhận endpoint admin phải từ chối truy cập.
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
        // Có token nhưng role không đủ nên middleware/authorize phải trả 403.
        var response = await client.GetAsync("/api/admin/enterprises");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEnterprises_WithAdminToken_ReturnsOk()
    {
        // Arrange: configure factory to use test jwt settings and mock mediator
        // Đây là case happy path: token Admin hợp lệ và endpoint phải cho phép gọi.
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                // Tái sử dụng cùng bộ config JWT test để sinh và đọc token thống nhất.
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
                // Không cần DB thật cho test authorization nên dùng InMemory để cô lập môi trường.
                services.RemoveAll(typeof(DbContextOptions<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>));
                services.AddDbContext<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_Admin"));

                // Mock danh sách enterprise để controller có dữ liệu trả về khi qua được bước auth.
                var mediatorMock = new Mock<IMediator>();

                var dummyList = new System.Collections.Generic.List<EnterpriseListDto>
                {
                    new EnterpriseListDto { Id = System.Guid.NewGuid(), CompanyName = "TestCo", IsVerified = true, ServiceArea = "Area", CreatedAt = System.DateTime.UtcNow }
                };

                // Tuple mô phỏng đúng shape return của GetEnterprisesQuery handler.
                var resultTuple = ((System.Collections.Generic.IEnumerable<EnterpriseListDto>)dummyList, dummyList.Count, 1);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(resultTuple);

                services.AddSingleton<IMediator>(mediatorMock.Object);

                // Dùng test auth scheme để endpoint nhận ra role Admin từ token mẫu.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });

        var client = factory.CreateClient();

        // create an admin token
        // Token Admin phải đi qua authorize thành công và nhận được response 200.
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
        // Request với role Admin hợp lệ nên controller phải trả OK.
        var response = await client.GetAsync("/api/admin/enterprises");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Enterprises retrieved successfully");
    }

    [Fact]
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
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
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
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
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
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
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
}

[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-21")]
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
            // Tạo principal test từ claim của JWT để ASP.NET Core áp dụng [Authorize(Roles=...)]
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            // Token sai định dạng sẽ bị coi là auth fail.
            return Task.FromResult(AuthenticateResult.Fail(ex));
        }
    }
}
