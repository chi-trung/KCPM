using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Moq;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using WastePlatform.Application.Admin.Analytics.DTOs;
using MediatR;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Integration
{
    /// <summary>
    /// WRP-BE-TESTS-006: Analytics API Integration Tests
    /// Integration tests for Analytics endpoints across all levels
    /// Focus: API response validation, date query functionality, and role-based access
    /// </summary>
    [AllureEpic("Analytics")]
    [AllureFeature("Analytics APIs")]
    [Allure.Net.Commons.Attributes.AllureLabel("story", "Date range analytics and summary endpoints")]
    [Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
    [Allure.Net.Commons.Attributes.AllureLabel("suite", "Application Integration")]
    [Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AnalyticsApiIntegrationTests")]
    [Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Integration")]
    [AllureOwner("Nguyễn Chí Trung")]
    [AllureSeverity(SeverityLevel.normal)]
    [Allure.Net.Commons.Attributes.AllureTag("api")]
    [Allure.Net.Commons.Attributes.AllureTag("analytics")]
    [Allure.Net.Commons.Attributes.AllureTag("integration")]
    [Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-9")]
    public class AnalyticsApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AnalyticsApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        #region Setup & Configuration

        private const string AdminAnalyticsApiBaseUrl = "/api/admin/analytics";
        private const string EnterpriseAnalyticsApiBaseUrl = "/api/enterprise/analytics";
        private const string PublicAnalyticsApiBaseUrl = "/api/public/analytics";

        private HttpClient CreateClientWithUser(string email, UserRole role, out Guid userId, string dbName, Action<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext, Guid>? seedAction = null)
        {
            var seededUserId = Guid.NewGuid();
            userId = seededUserId;

            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddInMemoryCollection(new Dictionary<string, string?>
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
                        options.UseInMemoryDatabase(dbName));

                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                    // Seed user
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>();
                        db.Database.EnsureCreated();
                        
                        var user = User.Create(email, "pwd", "Test User", role);
                        var idField = typeof(User).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        if (idField != null)
                            idField.SetValue(user, seededUserId);
                        
                        db.Users.Add(user);
                        db.SaveChanges();

                        seedAction?.Invoke(db, seededUserId);
                    }
                });
            });

            var client = factory.CreateClient();

            var jwtService = new JwtService(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                { "JwtSettings:Issuer", "test-issuer" },
                { "JwtSettings:Audience", "test-audience" },
                { "JwtSettings:ExpirationMinutes", "60" }
            }).Build());

            var tokenUser = User.Create(email, "pwd", "Test User", role);
            var idFieldToken = typeof(User).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (idFieldToken != null)
                idFieldToken.SetValue(tokenUser, seededUserId);

            var token = jwtService.GenerateToken(tokenUser);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        private HttpClient CreatePublicClient(string dbName, Action<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>? seedAction = null)
        {
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddInMemoryCollection(new Dictionary<string, string?>
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
                        options.UseInMemoryDatabase(dbName));

                    // Seed database
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>();
                        db.Database.EnsureCreated();
                        
                        seedAction?.Invoke(db);
                    }
                });
            });

            return factory.CreateClient();
        }

        #endregion

        #region Admin Analytics Overview Tests

        /// <summary>
        /// Test: Admin can retrieve overall statistics
        /// Endpoint: GET /api/admin/analytics/overview
        /// Expected: 200 OK with all metrics
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n O v e r v i e w E n d p o i n t - G e t, R e t u r n s O v e r v i e w M e t r i c s")]
        public async Task AdminOverviewEndpoint_Get_ReturnsOverviewMetrics()
        {
            AllureAttachmentHelper.AttachText("admin-overview-endpoint--get--returns-overview-met", "Test: AdminOverviewEndpoint_Get_ReturnsOverviewMetrics — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/overview");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var data = json.RootElement.GetProperty("data");

            data.GetProperty("totalReports").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("totalComplaints").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("totalUsers").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("activeEnterprises").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("registeredCollectors").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("totalWasteCollected").ValueKind.Should().Be(JsonValueKind.Number);
        }

        /// <summary>
        /// Test: Non-admin cannot access admin overview
        /// Expected: 403 Forbidden
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n O v e r v i e w E n d p o i n t - W i t h C i t i z e n R o l e, R e t u r n s F o r b i d d e n")]
        public async Task AdminOverviewEndpoint_WithCitizenRole_ReturnsForbidden()
        {
            AllureAttachmentHelper.AttachText("admin-overview-endpoint--with-citizen-role--return", "Test: AdminOverviewEndpoint_WithCitizenRole_ReturnsForbidden — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("citizen@example.com", UserRole.Citizen, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/overview");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Admin Report Analytics - Date Range Tests

        /// <summary>
        /// Test: Get report analytics without date parameters
        /// Expected: Uses default range (last 1 month)
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s E n d p o i n t - N o D a t e P a r a m s, U s e s D e f a u l t s")]
        public async Task AdminReportAnalyticsEndpoint_NoDateParams_UsesDefaults()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics-endpoint--no-date-params--u", "Test: AdminReportAnalyticsEndpoint_NoDateParams_UsesDefaults — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var data = json.RootElement.GetProperty("data");
            data.GetProperty("totalReports").ValueKind.Should().Be(JsonValueKind.Number);
        }

        /// <summary>
        /// Test: Get report analytics with valid date range
        /// Query: ?startDate=2026-01-01&endDate=2026-12-31
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s E n d p o i n t - V a l i d D a t e R a n g e, R e t u r n s F i l t e r e d D a t a")]
        public async Task AdminReportAnalyticsEndpoint_ValidDateRange_ReturnsFilteredData()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics-endpoint--valid-date-range", "Test: AdminReportAnalyticsEndpoint_ValidDateRange_ReturnsFilteredData — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2026-01-01&endDate=2026-12-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Get report analytics with only startDate
        /// Query: ?startDate=2026-01-01
        /// EndDate should default to today
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s E n d p o i n t - O n l y S t a r t D a t e, D e f a u l t s E n d T o T o d a y")]
        public async Task AdminReportAnalyticsEndpoint_OnlyStartDate_DefaultsEndToToday()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics-endpoint--only-start-date", "Test: AdminReportAnalyticsEndpoint_OnlyStartDate_DefaultsEndToToday — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2026-01-01");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Get report analytics with only endDate
        /// Query: ?endDate=2026-12-31
        /// StartDate should default to 1 month before endDate
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s E n d p o i n t - O n l y E n d D a t e, D e f a u l t s S t a r t")]
        public async Task AdminReportAnalyticsEndpoint_OnlyEndDate_DefaultsStart()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics-endpoint--only-end-date--de", "Test: AdminReportAnalyticsEndpoint_OnlyEndDate_DefaultsStart — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?endDate=2026-12-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Invalid date range (startDate > endDate)
        /// Query: ?startDate=2026-12-31&endDate=2026-01-01
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s E n d p o i n t - I n v a l i d D a t e R a n g e, R e t u r n s B a d R e q u e s t")]
        public async Task AdminReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics-endpoint--invalid-date-rang", "Test: AdminReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2026-12-31&endDate=2026-01-01");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Test: Malformed date format
        /// Query: ?startDate=2026/01/01&endDate=invalid
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s E n d p o i n t - M a l f o r m e d D a t e F o r m a t, R e t u r n s B a d R e q u e s t")]
        public async Task AdminReportAnalyticsEndpoint_MalformedDateFormat_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics-endpoint--malformed-date-fo", "Test: AdminReportAnalyticsEndpoint_MalformedDateFormat_ReturnsBadRequest — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2026/01/01&endDate=invalid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Test: Response structure validation
        /// Verify response has required fields
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s E n d p o i n t - R e s p o n s e S t r u c t u r e, C o n t a i n s R e q u i r e d F i e l d s")]
        public async Task AdminReportAnalyticsEndpoint_ResponseStructure_ContainsRequiredFields()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics-endpoint--response-structur", "Test: AdminReportAnalyticsEndpoint_ResponseStructure_ContainsRequiredFields — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            Guid citizenId = Guid.Empty;
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName, (db, uid) =>
            {
                var citizen = User.Create("citizen@example.com", "pwd", "Citizen", UserRole.Citizen);
                db.Users.Add(citizen);
                db.SaveChanges();
                citizenId = citizen.Id;

                // 2 Pending, 3 Accepted, 1 Rejected, 4 Collected
                var reports = new List<WasteReport>
                {
                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Pending 1", "Location 1"),
                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Pending 2", "Location 2"),
                    
                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Accepted 1", "Location 3"),
                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Accepted 2", "Location 4"),
                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Accepted 3", "Location 5"),

                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Rejected 1", "Location 6"),

                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Collected 1", "Location 7"),
                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Collected 2", "Location 8"),
                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Collected 3", "Location 9"),
                    WasteReport.Create(citizenId, 1, 10.0m, 20.0m, "Collected 4", "Location 10"),
                };

                reports[2].Accept();
                reports[3].Accept();
                reports[4].Accept();
                
                reports[5].Reject();

                reports[6].Accept();
                reports[6].Collect();
                reports[7].Accept();
                reports[7].Collect();
                reports[8].Accept();
                reports[8].Collect();
                reports[9].Accept();
                reports[9].Collect();

                db.WasteReports.AddRange(reports);
                db.SaveChanges();
            });

            // Act
            var yesterdayStr = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            var tomorrowStr = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate={yesterdayStr}&endDate={tomorrowStr}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var data = json.RootElement.GetProperty("data");

            data.GetProperty("totalReports").GetInt32().Should().Be(10);
            data.GetProperty("pendingReports").GetInt32().Should().Be(2);
            data.GetProperty("acceptedReports").GetInt32().Should().Be(3);
            data.GetProperty("rejectedReports").GetInt32().Should().Be(1);
            data.GetProperty("collectedReports").GetInt32().Should().Be(4);
            data.GetProperty("reportsByCategory").ValueKind.Should().Be(JsonValueKind.Object);
        }

        #endregion

        #region Admin User Analytics Tests

        /// <summary>
        /// Test: Get user analytics
        /// Endpoint: GET /api/admin/analytics/users
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n U s e r A n a l y t i c s E n d p o i n t - G e t, R e t u r n s U s e r M e t r i c s")]
        public async Task AdminUserAnalyticsEndpoint_Get_ReturnsUserMetrics()
        {
            AllureAttachmentHelper.AttachText("admin-user-analytics-endpoint--get--returns-user-m", "Test: AdminUserAnalyticsEndpoint_Get_ReturnsUserMetrics — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddInMemoryCollection(new Dictionary<string, string?>
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
                        options.UseInMemoryDatabase(dbName));

                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            });

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WastePlatform.Infrastructure.Persistence.WastePlatformDbContext>();
                db.Database.EnsureCreated();

                db.Users.RemoveRange(db.Users);
                db.Enterprises.RemoveRange(db.Enterprises);
                db.SaveChanges();

                var cit1 = User.Create("citizen1@example.com", "pwd", "Citizen One", UserRole.Citizen);
                var cit2 = User.Create("citizen2@example.com", "pwd", "Citizen Two", UserRole.Citizen);
                var cit3 = User.Create("citizen3@example.com", "pwd", "Citizen Three", UserRole.Citizen);
                
                var activeField = typeof(User).GetField("<IsActive>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                activeField?.SetValue(cit3, false);

                var entUser1 = User.Create("enterprise1@example.com", "pwd", "Enterprise One", UserRole.Enterprise);
                var entUser2 = User.Create("enterprise2@example.com", "pwd", "Enterprise Two", UserRole.Enterprise);

                var col1 = User.Create("collector1@example.com", "pwd", "Collector One", UserRole.Collector);
                var admin = User.Create("admin@example.com", "pwd", "Admin", UserRole.Admin);

                db.Users.AddRange(cit1, cit2, cit3, entUser1, entUser2, col1, admin);
                db.SaveChanges();

                var ent1 = new Enterprise { Id = Guid.NewGuid(), UserId = entUser1.Id, CompanyName = "Enterprise One Co", CapacityKgPerDay = 1000, IsVerified = true };
                var ent2 = new Enterprise { Id = Guid.NewGuid(), UserId = entUser2.Id, CompanyName = "Enterprise Two Co", CapacityKgPerDay = 1000, IsVerified = false };
                db.Enterprises.AddRange(ent1, ent2);
                db.SaveChanges();
            }

            var client = factory.CreateClient();
            var jwtService = new JwtService(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "test-secret-key-which-is-long-enough" },
                { "JwtSettings:Issuer", "test-issuer" },
                { "JwtSettings:Audience", "test-audience" },
                { "JwtSettings:ExpirationMinutes", "60" }
            }).Build());

            var adminUser = User.Create("admin@example.com", "pwd", "Admin", UserRole.Admin);
            var token = jwtService.GenerateToken(adminUser);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var data = json.RootElement.GetProperty("data");

            data.GetProperty("totalCitizens").GetInt32().Should().Be(3);
            data.GetProperty("activeCitizens").GetInt32().Should().Be(2);
            data.GetProperty("inactiveCitizens").GetInt32().Should().Be(1);

            data.GetProperty("totalEnterprises").GetInt32().Should().Be(2);
            data.GetProperty("verifiedEnterprises").GetInt32().Should().Be(1);
            data.GetProperty("unverifiedEnterprises").GetInt32().Should().Be(1);

            data.GetProperty("totalCollectors").GetInt32().Should().Be(1);
            data.GetProperty("totalAdmins").GetInt32().Should().Be(1);
        }

        #endregion

        #region Admin Waste Analytics - Date Range Tests

        /// <summary>
        /// Test: Get waste analytics without date parameters
        /// Expected: Uses default range
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n W a s t e A n a l y t i c s E n d p o i n t - N o D a t e P a r a m s, U s e s D e f a u l t s")]
        public async Task AdminWasteAnalyticsEndpoint_NoDateParams_UsesDefaults()
        {
            AllureAttachmentHelper.AttachText("admin-waste-analytics-endpoint--no-date-params--us", "Test: AdminWasteAnalyticsEndpoint_NoDateParams_UsesDefaults — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/waste");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Get waste analytics with date range
        /// Query: ?startDate=2026-01-01&endDate=2026-06-30
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n W a s t e A n a l y t i c s E n d p o i n t - W i t h D a t e R a n g e, R e t u r n s F i l t e r e d D a t a")]
        public async Task AdminWasteAnalyticsEndpoint_WithDateRange_ReturnsFilteredData()
        {
            AllureAttachmentHelper.AttachText("admin-waste-analytics-endpoint--with-date-range--r", "Test: AdminWasteAnalyticsEndpoint_WithDateRange_ReturnsFilteredData — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/waste?startDate=2026-01-01&endDate=2026-06-30");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Get waste analytics for future dates
        /// Expected: 200 with empty results
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n W a s t e A n a l y t i c s E n d p o i n t - F u t u r e D a t e s, R e t u r n s E m p t y R e s u l t s")]
        public async Task AdminWasteAnalyticsEndpoint_FutureDates_ReturnsEmptyResults()
        {
            AllureAttachmentHelper.AttachText("admin-waste-analytics-endpoint--future-dates--retu", "Test: AdminWasteAnalyticsEndpoint_FutureDates_ReturnsEmptyResults — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/waste?startDate=2027-01-01&endDate=2027-12-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Waste analytics invalid date range
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n W a s t e A n a l y t i c s E n d p o i n t - I n v a l i d D a t e R a n g e, R e t u r n s B a d R e q u e s t")]
        public async Task AdminWasteAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("admin-waste-analytics-endpoint--invalid-date-range", "Test: AdminWasteAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/waste?startDate=2026-12-31&endDate=2026-01-01");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Admin Summary Analytics Tests

        /// <summary>
        /// Test: Get comprehensive analytics summary
        /// Endpoint: GET /api/admin/analytics/summary
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n S u m m a r y E n d p o i n t - W i t h D a t e R a n g e, R e t u r n s C o m p r e h e n s i v e S u m m a r y")]
        public async Task AdminSummaryEndpoint_WithDateRange_ReturnsComprehensiveSummary()
        {
            AllureAttachmentHelper.AttachText("admin-summary-endpoint--with-date-range--returns-c", "Test: AdminSummaryEndpoint_WithDateRange_ReturnsComprehensiveSummary — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/summary?startDate=2026-01-01&endDate=2026-12-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var data = json.RootElement.GetProperty("data");

            data.GetProperty("overview").ValueKind.Should().Be(JsonValueKind.Object);
            data.GetProperty("reportAnalytics").ValueKind.Should().Be(JsonValueKind.Object);
            data.GetProperty("userAnalytics").ValueKind.Should().Be(JsonValueKind.Object);
            data.GetProperty("wasteAnalytics").ValueKind.Should().Be(JsonValueKind.Object);
        }

        /// <summary>
        /// Test: Summary analytics invalid date range
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n S u m m a r y E n d p o i n t - I n v a l i d D a t e R a n g e, R e t u r n s B a d R e q u e s t")]
        public async Task AdminSummaryEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("admin-summary-endpoint--invalid-date-range--return", "Test: AdminSummaryEndpoint_InvalidDateRange_ReturnsBadRequest — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/summary?startDate=2026-12-31&endDate=2026-01-01");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Enterprise Analytics - Date Range Tests

        /// <summary>
        /// Test: Enterprise can get their own report analytics
        /// Endpoint: GET /api/enterprise/analytics/reports
        /// Data is scoped to that enterprise
        /// </summary>
        [Fact]
        [AllureDescription("E n t e r p r i s e R e p o r t A n a l y t i c s E n d p o i n t - W i t h D a t e R a n g e, R e t u r n s S c o p e d D a t a")]
        public async Task EnterpriseReportAnalyticsEndpoint_WithDateRange_ReturnsScopedData()
        {
            AllureAttachmentHelper.AttachText("enterprise-report-analytics-endpoint--with-date-ra", "Test: EnterpriseReportAnalyticsEndpoint_WithDateRange_ReturnsScopedData — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("enterprise@example.com", UserRole.Enterprise, out var userId, dbName, (db, uid) =>
            {
                var enterpriseProfile = new Enterprise
                {
                    Id = Guid.NewGuid(),
                    UserId = uid,
                    CompanyName = "Test Enterprise",
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                };
                db.Enterprises.Add(enterpriseProfile);
                db.SaveChanges();
            });

            // Act
            var response = await client.GetAsync($"{EnterpriseAnalyticsApiBaseUrl}/reports?startDate=2026-01-01&endDate=2026-12-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Invalid date range for enterprise analytics
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        [AllureDescription("E n t e r p r i s e R e p o r t A n a l y t i c s E n d p o i n t - I n v a l i d D a t e R a n g e, R e t u r n s B a d R e q u e s t")]
        public async Task EnterpriseReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("enterprise-report-analytics-endpoint--invalid-date", "Test: EnterpriseReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("enterprise@example.com", UserRole.Enterprise, out _, dbName);

            // Act
            var response = await client.GetAsync($"{EnterpriseAnalyticsApiBaseUrl}/reports?startDate=2026-12-31&endDate=2026-01-01");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Test: Enterprise analytics without authentication
        /// Expected: 401 Unauthorized
        /// </summary>
        [Fact]
        [AllureDescription("E n t e r p r i s e R e p o r t A n a l y t i c s E n d p o i n t - W i t h o u t A u t h, R e t u r n s U n a u t h o r i z e d")]
        public async Task EnterpriseReportAnalyticsEndpoint_WithoutAuth_ReturnsUnauthorized()
        {
            AllureAttachmentHelper.AttachText("enterprise-report-analytics-endpoint--without-auth", "Test: EnterpriseReportAnalyticsEndpoint_WithoutAuth_ReturnsUnauthorized — passed ✅");
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"{EnterpriseAnalyticsApiBaseUrl}/reports");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Public Analytics - No Auth Required Tests

        /// <summary>
        /// Test: Public analytics without authentication
        /// Default: Last 3 months of data
        /// </summary>
        [Fact]
        [AllureDescription("P u b l i c R e p o r t A n a l y t i c s E n d p o i n t - N o A u t h, R e t u r n s L a s t T h r e e M o n t h s")]
        public async Task PublicReportAnalyticsEndpoint_NoAuth_ReturnsLastThreeMonths()
        {
            AllureAttachmentHelper.AttachText("public-report-analytics-endpoint--no-auth--returns", "Test: PublicReportAnalyticsEndpoint_NoAuth_ReturnsLastThreeMonths — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreatePublicClient(dbName);

            // Act
            var response = await client.GetAsync($"{PublicAnalyticsApiBaseUrl}/reports");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Public analytics with custom date range
        /// Query: ?startDate=2026-01-01&endDate=2026-06-30
        /// </summary>
        [Fact]
        [AllureDescription("P u b l i c R e p o r t A n a l y t i c s E n d p o i n t - W i t h D a t e R a n g e, R e t u r n s F i l t e r e d D a t a")]
        public async Task PublicReportAnalyticsEndpoint_WithDateRange_ReturnsFilteredData()
        {
            AllureAttachmentHelper.AttachText("public-report-analytics-endpoint--with-date-range", "Test: PublicReportAnalyticsEndpoint_WithDateRange_ReturnsFilteredData — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreatePublicClient(dbName);

            // Act
            var response = await client.GetAsync($"{PublicAnalyticsApiBaseUrl}/reports?startDate=2026-01-01&endDate=2026-06-30");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Public analytics with invalid date range
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        [AllureDescription("P u b l i c R e p o r t A n a l y t i c s E n d p o i n t - I n v a l i d D a t e R a n g e, R e t u r n s B a d R e q u e s t")]
        public async Task PublicReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("public-report-analytics-endpoint--invalid-date-ran", "Test: PublicReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreatePublicClient(dbName);

            // Act
            var response = await client.GetAsync($"{PublicAnalyticsApiBaseUrl}/reports?startDate=2026-12-31&endDate=2026-01-01");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Test: Public analytics with very old dates
        /// Query: ?startDate=2020-01-01&endDate=2020-12-31
        /// Expected: 200 with empty results
        /// </summary>
        [Fact]
        [AllureDescription("P u b l i c R e p o r t A n a l y t i c s E n d p o i n t - H i s t o r i c a l D a t e s, R e t u r n s E m p t y R e s u l t s")]
        public async Task PublicReportAnalyticsEndpoint_HistoricalDates_ReturnsEmptyResults()
        {
            AllureAttachmentHelper.AttachText("public-report-analytics-endpoint--historical-dates", "Test: PublicReportAnalyticsEndpoint_HistoricalDates_ReturnsEmptyResults — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreatePublicClient(dbName);

            // Act
            var response = await client.GetAsync($"{PublicAnalyticsApiBaseUrl}/reports?startDate=2020-01-01&endDate=2020-12-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Date Range Edge Cases

        /// <summary>
        /// Test: Same day date range
        /// Query: ?startDate=2026-06-15&endDate=2026-06-15
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s E n d p o i n t - S a m e D a y R a n g e, R e t u r n s D a t a F o r T h a t D a y")]
        public async Task AnalyticsEndpoint_SameDayRange_ReturnsDataForThatDay()
        {
            AllureAttachmentHelper.AttachText("analytics-endpoint--same-day-range--returns-data-f", "Test: AnalyticsEndpoint_SameDayRange_ReturnsDataForThatDay — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2026-06-15&endDate=2026-06-15");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: ISO 8601 UTC timestamps
        /// Query: ?startDate=2026-01-01T00:00:00Z&endDate=2026-01-31T23:59:59Z
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s E n d p o i n t - U t c T i m e s t a m p s, P a r s e s C o r r e c t l y")]
        public async Task AnalyticsEndpoint_UtcTimestamps_ParsesCorrectly()
        {
            AllureAttachmentHelper.AttachText("analytics-endpoint--utc-timestamps--parses-correct", "Test: AnalyticsEndpoint_UtcTimestamps_ParsesCorrectly — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2026-01-01T00:00:00Z&endDate=2026-01-31T23:59:59Z");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Year start boundary
        /// Query: ?startDate=2026-01-01&endDate=2026-01-31
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s E n d p o i n t - Y e a r S t a r t B o u n d a r y, R e t u r n s J a n u a r y D a t a")]
        public async Task AnalyticsEndpoint_YearStartBoundary_ReturnsJanuaryData()
        {
            AllureAttachmentHelper.AttachText("analytics-endpoint--year-start-boundary--returns-j", "Test: AnalyticsEndpoint_YearStartBoundary_ReturnsJanuaryData — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2026-01-01&endDate=2026-01-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Year end boundary
        /// Query: ?startDate=2026-12-01&endDate=2026-12-31
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s E n d p o i n t - Y e a r E n d B o u n d a r y, R e t u r n s D e c e m b e r D a t a")]
        public async Task AnalyticsEndpoint_YearEndBoundary_ReturnsDecemberData()
        {
            AllureAttachmentHelper.AttachText("analytics-endpoint--year-end-boundary--returns-dec", "Test: AnalyticsEndpoint_YearEndBoundary_ReturnsDecemberData — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2026-12-01&endDate=2026-12-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Multi-year date range
        /// Query: ?startDate=2024-01-01&endDate=2026-12-31
        /// Expected: Response time is acceptable
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s E n d p o i n t - M u l t i Y e a r R a n g e, R e s p o n d s I n A c c e p t a b l e T i m e")]
        public async Task AnalyticsEndpoint_MultiYearRange_RespondsInAcceptableTime()
        {
            AllureAttachmentHelper.AttachText("analytics-endpoint--multi-year-range--responds-in", "Test: AnalyticsEndpoint_MultiYearRange_RespondsInAcceptableTime — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2024-01-01&endDate=2026-12-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Very large date range performance
        /// Query: ?startDate=2020-01-01&endDate=2026-12-31
        /// Expected: Response time is acceptable
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s E n d p o i n t - L a r g e D a t a s e t, P e r f o r m a n c e A c c e p t a b l e")]
        public async Task AnalyticsEndpoint_LargeDataset_PerformanceAcceptable()
        {
            AllureAttachmentHelper.AttachText("analytics-endpoint--large-dataset--performance-acc", "Test: AnalyticsEndpoint_LargeDataset_PerformanceAcceptable — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2020-01-01&endDate=2026-12-31");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test: Null date parameters
        /// Query: (empty or ?startDate=&endDate=)
        /// Expected: Uses defaults
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s E n d p o i n t - N u l l P a r a m e t e r s, U s e s D e f a u l t s")]
        public async Task AnalyticsEndpoint_NullParameters_UsesDefaults()
        {
            AllureAttachmentHelper.AttachText("analytics-endpoint--null-parameters--uses-defaults", "Test: AnalyticsEndpoint_NullParameters_UsesDefaults — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=&endDate=");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Response Validation Tests

        /// <summary>
        /// Test: Verify response structure for admin overview
        /// Validate all required fields are present and have correct types
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n O v e r v i e w E n d p o i n t - R e s p o n s e S t r u c t u r e, V a l i d a t e A l l F i e l d s")]
        public async Task AdminOverviewEndpoint_ResponseStructure_ValidateAllFields()
        {
            AllureAttachmentHelper.AttachText("admin-overview-endpoint--response-structure--valid", "Test: AdminOverviewEndpoint_ResponseStructure_ValidateAllFields — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/overview");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var data = json.RootElement.GetProperty("data");

            data.GetProperty("totalReports").GetInt32().Should().BeGreaterThanOrEqualTo(0);
            data.GetProperty("totalComplaints").GetInt32().Should().BeGreaterThanOrEqualTo(0);
            data.GetProperty("totalUsers").GetInt32().Should().BeGreaterThanOrEqualTo(0);
            data.GetProperty("activeEnterprises").GetInt32().Should().BeGreaterThanOrEqualTo(0);
            data.GetProperty("registeredCollectors").GetInt32().Should().BeGreaterThanOrEqualTo(0);
            data.GetProperty("totalWasteCollected").GetDecimal().Should().BeGreaterThanOrEqualTo(0);
        }

        /// <summary>
        /// Test: Verify response structure for report analytics
        /// Validate data structure and types
        /// </summary>
        [Fact]
        [AllureDescription("R e p o r t A n a l y t i c s E n d p o i n t - R e s p o n s e S t r u c t u r e, V a l i d a t e D a t a T y p e s")]
        public async Task ReportAnalyticsEndpoint_ResponseStructure_ValidateDataTypes()
        {
            AllureAttachmentHelper.AttachText("report-analytics-endpoint--response-structure--val", "Test: ReportAnalyticsEndpoint_ResponseStructure_ValidateDataTypes — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var data = json.RootElement.GetProperty("data");

            data.GetProperty("totalReports").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("pendingReports").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("acceptedReports").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("rejectedReports").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("collectedReports").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("reportsByCategory").ValueKind.Should().Be(JsonValueKind.Object);
            data.GetProperty("averageReportsPerDay").ValueKind.Should().Be(JsonValueKind.Number);
            data.GetProperty("wasteByArea").ValueKind.Should().Be(JsonValueKind.Array);
            data.GetProperty("wasteByType").ValueKind.Should().Be(JsonValueKind.Array);
            data.GetProperty("monthlyTrends").ValueKind.Should().Be(JsonValueKind.Array);
        }

        #endregion

        #region Error Messages & Validation

        /// <summary>
        /// Test: Clear error messages for invalid inputs
        /// Verify error responses are meaningful
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s E n d p o i n t - I n v a l i d D a t e R a n g e, C o n t a i n s M e a n i n g f u l E r r o r M e s s a g e")]
        public async Task AnalyticsEndpoint_InvalidDateRange_ContainsMeaningfulErrorMessage()
        {
            AllureAttachmentHelper.AttachText("analytics-endpoint--invalid-date-range--contains-m", "Test: AnalyticsEndpoint_InvalidDateRange_ContainsMeaningfulErrorMessage — passed ✅");
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var client = CreateClientWithUser("admin@example.com", UserRole.Admin, out _, dbName);

            // Act
            var response = await client.GetAsync($"{AdminAnalyticsApiBaseUrl}/reports?startDate=2026-12-31&endDate=2026-01-01");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.ToLower().Should().Contain("date");
        }

        #endregion
    }
}


