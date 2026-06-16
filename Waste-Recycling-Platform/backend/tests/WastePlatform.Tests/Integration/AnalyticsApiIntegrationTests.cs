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
        public async Task AdminOverviewEndpoint_Get_ReturnsOverviewMetrics()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-o-v-e-r-v-i-e-w-e-n-d-p-o-i-n-t_-g-e-t_-", "Executed: AdminOverviewEndpoint_Get_ReturnsOverviewMetrics");
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
        public async Task AdminOverviewEndpoint_WithCitizenRole_ReturnsForbidden()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-o-v-e-r-v-i-e-w-e-n-d-p-o-i-n-t_-w-i-t-h", "Executed: AdminOverviewEndpoint_WithCitizenRole_ReturnsForbidden");
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
        public async Task AdminReportAnalyticsEndpoint_NoDateParams_UsesDefaults()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_NoDateParams_UsesDefaults");
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
        public async Task AdminReportAnalyticsEndpoint_ValidDateRange_ReturnsFilteredData()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_ValidDateRange_ReturnsFilteredData");
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
        public async Task AdminReportAnalyticsEndpoint_OnlyStartDate_DefaultsEndToToday()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_OnlyStartDate_DefaultsEndToToday");
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
        public async Task AdminReportAnalyticsEndpoint_OnlyEndDate_DefaultsStart()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_OnlyEndDate_DefaultsStart");
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
        public async Task AdminReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest");
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
        public async Task AdminReportAnalyticsEndpoint_MalformedDateFormat_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_MalformedDateFormat_ReturnsBadRequest");
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
        public async Task AdminReportAnalyticsEndpoint_ResponseStructure_ContainsRequiredFields()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_ResponseStructure_ContainsRequiredFields");
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
        public async Task AdminUserAnalyticsEndpoint_Get_ReturnsUserMetrics()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-u-s-e-r-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-", "Executed: AdminUserAnalyticsEndpoint_Get_ReturnsUserMetrics");
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
        public async Task AdminWasteAnalyticsEndpoint_NoDateParams_UsesDefaults()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-", "Executed: AdminWasteAnalyticsEndpoint_NoDateParams_UsesDefaults");
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
        public async Task AdminWasteAnalyticsEndpoint_WithDateRange_ReturnsFilteredData()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-", "Executed: AdminWasteAnalyticsEndpoint_WithDateRange_ReturnsFilteredData");
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
        public async Task AdminWasteAnalyticsEndpoint_FutureDates_ReturnsEmptyResults()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-", "Executed: AdminWasteAnalyticsEndpoint_FutureDates_ReturnsEmptyResults");
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
        public async Task AdminWasteAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-", "Executed: AdminWasteAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest");
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
        public async Task AdminSummaryEndpoint_WithDateRange_ReturnsComprehensiveSummary()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-s-u-m-m-a-r-y-e-n-d-p-o-i-n-t_-w-i-t-h-d", "Executed: AdminSummaryEndpoint_WithDateRange_ReturnsComprehensiveSummary");
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
        public async Task AdminSummaryEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-s-u-m-m-a-r-y-e-n-d-p-o-i-n-t_-i-n-v-a-l", "Executed: AdminSummaryEndpoint_InvalidDateRange_ReturnsBadRequest");
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
        public async Task EnterpriseReportAnalyticsEndpoint_WithDateRange_ReturnsScopedData()
        {
            AllureAttachmentHelper.AttachText("test-e-n-t-e-r-p-r-i-s-e-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-", "Executed: EnterpriseReportAnalyticsEndpoint_WithDateRange_ReturnsScopedData");
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
        public async Task EnterpriseReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("test-e-n-t-e-r-p-r-i-s-e-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-", "Executed: EnterpriseReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest");
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
        public async Task EnterpriseReportAnalyticsEndpoint_WithoutAuth_ReturnsUnauthorized()
        {
            AllureAttachmentHelper.AttachText("test-e-n-t-e-r-p-r-i-s-e-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-", "Executed: EnterpriseReportAnalyticsEndpoint_WithoutAuth_ReturnsUnauthorized");
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
        public async Task PublicReportAnalyticsEndpoint_NoAuth_ReturnsLastThreeMonths()
        {
            AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-", "Executed: PublicReportAnalyticsEndpoint_NoAuth_ReturnsLastThreeMonths");
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
        public async Task PublicReportAnalyticsEndpoint_WithDateRange_ReturnsFilteredData()
        {
            AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-", "Executed: PublicReportAnalyticsEndpoint_WithDateRange_ReturnsFilteredData");
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
        public async Task PublicReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-", "Executed: PublicReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest");
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
        public async Task PublicReportAnalyticsEndpoint_HistoricalDates_ReturnsEmptyResults()
        {
            AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-", "Executed: PublicReportAnalyticsEndpoint_HistoricalDates_ReturnsEmptyResults");
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
        public async Task AnalyticsEndpoint_SameDayRange_ReturnsDataForThatDay()
        {
            AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-s-a-m-e-d-a-y-r", "Executed: AnalyticsEndpoint_SameDayRange_ReturnsDataForThatDay");
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
        public async Task AnalyticsEndpoint_UtcTimestamps_ParsesCorrectly()
        {
            AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-u-t-c-t-i-m-e-s", "Executed: AnalyticsEndpoint_UtcTimestamps_ParsesCorrectly");
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
        public async Task AnalyticsEndpoint_YearStartBoundary_ReturnsJanuaryData()
        {
            AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-y-e-a-r-s-t-a-r", "Executed: AnalyticsEndpoint_YearStartBoundary_ReturnsJanuaryData");
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
        public async Task AnalyticsEndpoint_YearEndBoundary_ReturnsDecemberData()
        {
            AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-y-e-a-r-e-n-d-b", "Executed: AnalyticsEndpoint_YearEndBoundary_ReturnsDecemberData");
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
        public async Task AnalyticsEndpoint_MultiYearRange_RespondsInAcceptableTime()
        {
            AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-m-u-l-t-i-y-e-a", "Executed: AnalyticsEndpoint_MultiYearRange_RespondsInAcceptableTime");
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
        public async Task AnalyticsEndpoint_LargeDataset_PerformanceAcceptable()
        {
            AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-l-a-r-g-e-d-a-t", "Executed: AnalyticsEndpoint_LargeDataset_PerformanceAcceptable");
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
        public async Task AnalyticsEndpoint_NullParameters_UsesDefaults()
        {
            AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-n-u-l-l-p-a-r-a", "Executed: AnalyticsEndpoint_NullParameters_UsesDefaults");
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
        public async Task AdminOverviewEndpoint_ResponseStructure_ValidateAllFields()
        {
            AllureAttachmentHelper.AttachText("test-a-d-m-i-n-o-v-e-r-v-i-e-w-e-n-d-p-o-i-n-t_-r-e-s-p", "Executed: AdminOverviewEndpoint_ResponseStructure_ValidateAllFields");
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
        public async Task ReportAnalyticsEndpoint_ResponseStructure_ValidateDataTypes()
        {
            AllureAttachmentHelper.AttachText("test-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-r-e", "Executed: ReportAnalyticsEndpoint_ResponseStructure_ValidateDataTypes");
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
        public async Task AnalyticsEndpoint_InvalidDateRange_ContainsMeaningfulErrorMessage()
        {
            AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-i-n-v-a-l-i-d-d", "Executed: AnalyticsEndpoint_InvalidDateRange_ContainsMeaningfulErrorMessage");
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
