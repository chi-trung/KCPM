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
[AllureFeature("AnalyticsRepository")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Analytics data aggregation")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AnalyticsRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("repository")]
public class AnalyticsRepositoryTests
{
    [Fact]
    [AllureDescription("GetOverviewAsync should return all-zero statistics when the database is empty.")]
    public async Task GetOverviewAsync_EmptyDatabase_ShouldReturnZeroCounts()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new AnalyticsRepository(context);

        // Act
        var overview = await repository.GetOverviewAsync();

        // Assert
        overview.Should().NotBeNull();
        overview.TotalReports.Should().Be(0);
        overview.TotalComplaints.Should().Be(0);
        overview.TotalUsers.Should().Be(0);
        overview.ActiveEnterprises.Should().Be(0);
        overview.RegisteredCollectors.Should().Be(0);
        overview.TotalWasteCollected.Should().Be(0m);

        AllureAttachmentHelper.AttachText("overview-empty-db", "Test: GetOverviewAsync_EmptyDatabase_ShouldReturnZeroCounts — passed ✅");
    }

    [Fact]
    [AllureDescription("GetOverviewAsync should return correct counts for users, reports, complaints, verified enterprises, and collectors.")]
    public async Task GetOverviewAsync_WithSeededData_ShouldReturnCorrectCounts()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new AnalyticsRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen", UserRole.Citizen);
        var collector = User.Create("collector@test.com", "hash", "Collector", UserRole.Collector);
        var enterpriseUser = User.Create("ent@test.com", "hash", "Enterprise User", UserRole.Enterprise);
        context.Users.AddRange(citizen, collector, enterpriseUser);

        context.Enterprises.Add(new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = enterpriseUser.Id,
            CompanyName = "Eco Corp",
            IsVerified = true
        });

        context.WasteReports.Add(WasteReport.Create(citizen.Id, 1, 10.0m, 106.0m));
        context.Complaints.Add(Complaint.Create(citizen.Id, "Test complaint"));
        await context.SaveChangesAsync();

        // Act
        var overview = await repository.GetOverviewAsync();

        // Assert
        overview.TotalUsers.Should().Be(3);
        overview.TotalReports.Should().Be(1);
        overview.TotalComplaints.Should().Be(1);
        overview.ActiveEnterprises.Should().Be(1);
        overview.RegisteredCollectors.Should().Be(1);

        AllureAttachmentHelper.AttachText("overview-seeded", $"Test: Users={overview.TotalUsers}, Reports={overview.TotalReports}, Complaints={overview.TotalComplaints} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetReportAnalyticsAsync should return empty analytics when no reports exist in the date range.")]
    public async Task GetReportAnalyticsAsync_EmptyDatabase_ShouldReturnZeroCounts()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new AnalyticsRepository(context);

        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Act
        var analytics = await repository.GetReportAnalyticsAsync(startDate, endDate);

        // Assert
        analytics.Should().NotBeNull();
        analytics.TotalReports.Should().Be(0);
        analytics.AcceptedReports.Should().Be(0);
        analytics.PendingReports.Should().Be(0);
        analytics.RejectedReports.Should().Be(0);
        analytics.CollectedReports.Should().Be(0);
        analytics.ReportsByCategory.Should().BeEmpty();
        analytics.AverageReportsPerDay.Should().Be(0m);

        AllureAttachmentHelper.AttachText("report-analytics-empty", "Test: GetReportAnalyticsAsync_EmptyDatabase — passed ✅");
    }

    [Fact]
    [AllureDescription("GetReportAnalyticsAsync should count reports by status and category within the date range.")]
    public async Task GetReportAnalyticsAsync_WithSeededData_ShouldReturnCorrectBreakdown()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new AnalyticsRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen", UserRole.Citizen);
        context.Users.Add(citizen);

        var category = new WasteCategory { Id = 1, Name = "Plastic", Description = "Plastic waste" };
        context.WasteCategories.Add(category);

        var pendingReport = WasteReport.Create(citizen.Id, 1, 10.0m, 106.0m);
        var acceptedReport = WasteReport.Create(citizen.Id, 1, 10.1m, 106.1m);
        acceptedReport.Accept();

        context.WasteReports.AddRange(pendingReport, acceptedReport);
        await context.SaveChangesAsync();

        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var analytics = await repository.GetReportAnalyticsAsync(startDate, endDate);

        // Assert
        analytics.TotalReports.Should().Be(2);
        analytics.PendingReports.Should().Be(1);
        analytics.AcceptedReports.Should().Be(1);
        analytics.ReportsByCategory.Should().ContainKey("Plastic");
        analytics.ReportsByCategory["Plastic"].Should().Be(2);
        analytics.AverageReportsPerDay.Should().BeGreaterThan(0);

        AllureAttachmentHelper.AttachText("report-analytics-seeded", $"Test: Total={analytics.TotalReports}, Pending={analytics.PendingReports}, Accepted={analytics.AcceptedReports} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetUserAnalyticsAsync should return correct citizen, enterprise, collector, and admin counts.")]
    public async Task GetUserAnalyticsAsync_WithSeededData_ShouldReturnCorrectRoleCounts()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new AnalyticsRepository(context);

        var citizen1 = User.Create("c1@test.com", "hash", "Citizen 1", UserRole.Citizen);
        var citizen2 = User.Create("c2@test.com", "hash", "Citizen 2", UserRole.Citizen);
        citizen2.Deactivate();
        var collector = User.Create("col@test.com", "hash", "Collector 1", UserRole.Collector);
        var admin = User.Create("admin@test.com", "hash", "Admin", UserRole.Admin);
        var enterpriseUser = User.Create("ent@test.com", "hash", "Ent User", UserRole.Enterprise);
        context.Users.AddRange(citizen1, citizen2, collector, admin, enterpriseUser);

        context.Enterprises.AddRange(
            new Enterprise { Id = Guid.NewGuid(), UserId = enterpriseUser.Id, CompanyName = "Corp A", IsVerified = true },
            new Enterprise { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CompanyName = "Corp B", IsVerified = false });
        await context.SaveChangesAsync();

        // Act
        var analytics = await repository.GetUserAnalyticsAsync();

        // Assert
        analytics.TotalCitizens.Should().Be(2);
        analytics.ActiveCitizens.Should().Be(1);
        analytics.InactiveCitizens.Should().Be(1);
        analytics.TotalEnterprises.Should().Be(2);
        analytics.VerifiedEnterprises.Should().Be(1);
        analytics.UnverifiedEnterprises.Should().Be(1);
        analytics.TotalCollectors.Should().Be(1);
        analytics.TotalAdmins.Should().Be(1);

        AllureAttachmentHelper.AttachText("user-analytics", $"Test: Citizens={analytics.TotalCitizens}, Active={analytics.ActiveCitizens}, Admins={analytics.TotalAdmins} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetWasteAnalyticsAsync should return waste category counts and totals for reports within the date range.")]
    public async Task GetWasteAnalyticsAsync_WithSeededData_ShouldReturnCategoryBreakdown()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new AnalyticsRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen", UserRole.Citizen);
        context.Users.Add(citizen);

        context.WasteCategories.AddRange(
            new WasteCategory { Id = 1, Name = "Plastic", Description = "Plastic" },
            new WasteCategory { Id = 2, Name = "Organic", Description = "Organic" });

        context.WasteReports.AddRange(
            WasteReport.Create(citizen.Id, 1, 10.0m, 106.0m),
            WasteReport.Create(citizen.Id, 1, 10.1m, 106.1m),
            WasteReport.Create(citizen.Id, 2, 10.2m, 106.2m));
        await context.SaveChangesAsync();

        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var analytics = await repository.GetWasteAnalyticsAsync(startDate, endDate);

        // Assert
        analytics.TotalWasteCategories.Should().Be(2);
        analytics.ActiveWasteTypes.Should().Be(2);
        analytics.WasteByCategory.Should().ContainKey("Plastic");
        analytics.WasteByCategory["Plastic"].Should().Be(2m);
        analytics.WasteByCategory.Should().ContainKey("Organic");
        analytics.WasteByCategory["Organic"].Should().Be(1m);
        analytics.TotalWasteKg.Should().Be(3m);

        AllureAttachmentHelper.AttachText("waste-analytics", $"Test: Categories={analytics.TotalWasteCategories}, TotalKg={analytics.TotalWasteKg} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetSummaryAsync should return a composite DTO containing all four analytics sections.")]
    public async Task GetSummaryAsync_EmptyDatabase_ShouldReturnCompositeWithZeroCounts()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new AnalyticsRepository(context);

        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Act
        var summary = await repository.GetSummaryAsync(startDate, endDate);

        // Assert
        summary.Should().NotBeNull();
        summary.Overview.Should().NotBeNull();
        summary.Overview.TotalReports.Should().Be(0);
        summary.ReportAnalytics.Should().NotBeNull();
        summary.ReportAnalytics.TotalReports.Should().Be(0);
        summary.UserAnalytics.Should().NotBeNull();
        summary.UserAnalytics.TotalCitizens.Should().Be(0);
        summary.WasteAnalytics.Should().NotBeNull();
        summary.WasteAnalytics.TotalWasteCategories.Should().Be(0);

        AllureAttachmentHelper.AttachText("summary-empty", "Test: GetSummaryAsync_EmptyDatabase — all sections present with zero counts — passed ✅");
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
