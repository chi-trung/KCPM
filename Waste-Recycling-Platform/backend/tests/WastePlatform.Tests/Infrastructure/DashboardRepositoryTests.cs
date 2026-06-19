using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Persistence.Repositories;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Infrastructure;

[AllureEpic("Infrastructure")]
[AllureFeature("DashboardRepository")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Dashboard statistics aggregation")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "DashboardRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("repository")]
public class DashboardRepositoryTests
{
    [Fact]
    [AllureDescription("GetStatsAsync should return all-zero statistics when the database is empty.")]
    public async Task GetStatsAsync_EmptyDatabase_ShouldReturnZeroStats()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new DashboardRepository(context);

        // Act
        var stats = await repository.GetStatsAsync(CancellationToken.None);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalUsers.Should().Be(0);
        stats.TotalReports.Should().Be(0);
        stats.PendingComplaints.Should().Be(0);
        stats.CompletedReports.Should().Be(0);
        stats.ActiveCollectors.Should().Be(0);
        stats.AcceptedReports.Should().Be(0);
        stats.TotalWasteWeight.Should().Be(0.0);
        stats.MonthlyTraffic.Should().BeEmpty();
        stats.UserDistribution.Should().BeEmpty();
        stats.RecentLogs.Should().BeEmpty();

        AllureAttachmentHelper.AttachText("dashboard-empty-db", "Test: GetStatsAsync_EmptyDatabase_ShouldReturnZeroStats — passed ✅");
    }

    [Fact]
    [AllureDescription("GetStatsAsync should count total users correctly from seeded data.")]
    public async Task GetStatsAsync_WithUsers_ShouldReturnCorrectTotalUsers()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new DashboardRepository(context);

        context.Users.AddRange(
            User.Create("citizen1@test.com", "hash", "Citizen 1", UserRole.Citizen),
            User.Create("citizen2@test.com", "hash", "Citizen 2", UserRole.Citizen),
            User.Create("admin@test.com", "hash", "Admin", UserRole.Admin));
        await context.SaveChangesAsync();

        // Act
        var stats = await repository.GetStatsAsync(CancellationToken.None);

        // Assert
        stats.TotalUsers.Should().Be(3);

        AllureAttachmentHelper.AttachText("dashboard-total-users", $"Test: TotalUsers={stats.TotalUsers} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetStatsAsync should count total reports and accepted/completed reports correctly.")]
    public async Task GetStatsAsync_WithReports_ShouldReturnCorrectReportCounts()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new DashboardRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen", UserRole.Citizen);
        context.Users.Add(citizen);

        var pendingReport = WasteReport.Create(citizen.Id, 1, 10.0m, 106.0m);
        var acceptedReport = WasteReport.Create(citizen.Id, 1, 10.1m, 106.1m);
        acceptedReport.Accept();

        context.WasteReports.AddRange(pendingReport, acceptedReport);
        await context.SaveChangesAsync();

        // Act
        var stats = await repository.GetStatsAsync(CancellationToken.None);

        // Assert
        stats.TotalReports.Should().Be(2);
        stats.AcceptedReports.Should().Be(1);

        AllureAttachmentHelper.AttachText("dashboard-report-counts", $"Test: TotalReports={stats.TotalReports}, Accepted={stats.AcceptedReports} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetStatsAsync should count active collectors from the Collectors table.")]
    public async Task GetStatsAsync_WithCollectors_ShouldReturnActiveCollectorCount()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new DashboardRepository(context);

        var enterpriseUser = User.Create("ent@test.com", "hash", "Ent", UserRole.Enterprise);
        var collectorUser1 = User.Create("col1@test.com", "hash", "Collector 1", UserRole.Collector);
        var collectorUser2 = User.Create("col2@test.com", "hash", "Collector 2", UserRole.Collector);
        context.Users.AddRange(enterpriseUser, collectorUser1, collectorUser2);

        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = enterpriseUser.Id,
            CompanyName = "Eco Corp"
        };
        context.Enterprises.Add(enterprise);

        context.Collectors.AddRange(
            new Collector { Id = Guid.NewGuid(), UserId = collectorUser1.Id, EnterpriseId = enterprise.Id, IsAvailable = true },
            new Collector { Id = Guid.NewGuid(), UserId = collectorUser2.Id, EnterpriseId = enterprise.Id, IsAvailable = false });
        await context.SaveChangesAsync();

        // Act
        var stats = await repository.GetStatsAsync(CancellationToken.None);

        // Assert
        stats.ActiveCollectors.Should().Be(1);

        AllureAttachmentHelper.AttachText("dashboard-active-collectors", $"Test: ActiveCollectors={stats.ActiveCollectors} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetStatsAsync should return recent activity logs ordered by CreatedAt descending, limited to 5.")]
    public async Task GetStatsAsync_WithAuditLogs_ShouldReturnRecentLogs()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new DashboardRepository(context);

        var admin = User.Create("admin@test.com", "hash", "Admin User", UserRole.Admin);
        context.Users.Add(admin);

        for (var i = 0; i < 7; i++)
        {
            context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = admin.Id,
                Action = $"Action {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        // Act
        var stats = await repository.GetStatsAsync(CancellationToken.None);

        // Assert
        stats.RecentLogs.Should().HaveCount(5);
        stats.RecentLogs.First().User.Should().Be("Admin User");

        AllureAttachmentHelper.AttachText("dashboard-recent-logs", $"Test: RecentLogs count={stats.RecentLogs.Count} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetStatsAsync should compute user distribution grouped by role.")]
    public async Task GetStatsAsync_WithMixedRoles_ShouldReturnUserDistribution()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new DashboardRepository(context);

        context.Users.AddRange(
            User.Create("c1@test.com", "hash", "Citizen 1", UserRole.Citizen),
            User.Create("c2@test.com", "hash", "Citizen 2", UserRole.Citizen),
            User.Create("e1@test.com", "hash", "Enterprise 1", UserRole.Enterprise),
            User.Create("col1@test.com", "hash", "Collector 1", UserRole.Collector));
        await context.SaveChangesAsync();

        // Act
        var stats = await repository.GetStatsAsync(CancellationToken.None);

        // Assert
        stats.UserDistribution.Should().NotBeEmpty();
        stats.UserDistribution.Should().Contain(d => d.Name == "Người dân" && d.Value == 2);
        stats.UserDistribution.Should().Contain(d => d.Name == "Doanh nghiệp" && d.Value == 1);
        stats.UserDistribution.Should().Contain(d => d.Name == "Người thu gom" && d.Value == 1);

        AllureAttachmentHelper.AttachText("dashboard-user-distribution", $"Test: Distribution groups={stats.UserDistribution.Count} — passed ✅");
    }

    private static WastePlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new WastePlatformDbContext(options);
    }
}
