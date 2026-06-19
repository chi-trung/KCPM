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
[AllureFeature("ReportRepository")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Persist and query waste reports")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "ReportRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("repository")]
public class ReportRepositoryTests
{
    [Fact]
    [AllureDescription("AddAsync should persist a new waste report and return the same entity.")]
    public async Task AddAsync_ShouldAddReportAndReturnIt()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ReportRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen One", UserRole.Citizen);
        context.Users.Add(citizen);
        await context.SaveChangesAsync();

        var report = WasteReport.Create(citizen.Id, 1, 10.5m, 106.7m, "Illegal dump", "123 Street");

        // Act
        var result = await repository.AddAsync(report);
        await context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.CitizenId.Should().Be(citizen.Id);
        result.Latitude.Should().Be(10.5m);
        result.Longitude.Should().Be(106.7m);
        (await context.WasteReports.CountAsync()).Should().Be(1);

        AllureAttachmentHelper.AttachText("add-async--should-add-report", "Test: AddAsync_ShouldAddReportAndReturnIt — passed ✅");
    }

    [Fact]
    [AllureDescription("GetByIdAsync should return null when the report does not exist.")]
    public async Task GetByIdAsync_WhenReportDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ReportRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();

        AllureAttachmentHelper.AttachText("get-by-id--not-found", "Test: GetByIdAsync_WhenReportDoesNotExist_ShouldReturnNull — passed ✅");
    }

    [Fact]
    [AllureDescription("GetByIdAsync should return the report with included navigations when it exists.")]
    public async Task GetByIdAsync_WhenReportExists_ShouldReturnWithIncludes()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ReportRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen One", UserRole.Citizen);
        context.Users.Add(citizen);

        var category = new WasteCategory { Id = 1, Name = "Plastic", Description = "Plastic waste" };
        context.WasteCategories.Add(category);

        var report = WasteReport.Create(citizen.Id, 1, 10.5m, 106.7m, "Test report", "456 Avenue");
        context.WasteReports.Add(report);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(report.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(report.Id);
        result.Citizen.Should().NotBeNull();
        result.Citizen.FullName.Should().Be("Citizen One");
        result.WasteCategory.Should().NotBeNull();
        result.WasteCategory!.Name.Should().Be("Plastic");

        AllureAttachmentHelper.AttachText("get-by-id--with-includes", "Test: GetByIdAsync_WhenReportExists_ShouldReturnWithIncludes — passed ✅");
    }

    [Fact]
    [AllureDescription("GetByCitizenIdAsync should return only reports belonging to the specified citizen with correct pagination.")]
    public async Task GetByCitizenIdAsync_ShouldReturnPaginatedReportsForCitizen()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ReportRepository(context);

        var citizen1 = User.Create("citizen1@test.com", "hash", "Citizen One", UserRole.Citizen);
        var citizen2 = User.Create("citizen2@test.com", "hash", "Citizen Two", UserRole.Citizen);
        context.Users.AddRange(citizen1, citizen2);
        await context.SaveChangesAsync();

        // Add 3 reports for citizen1 and 1 for citizen2
        context.WasteReports.AddRange(
            WasteReport.Create(citizen1.Id, 1, 10.0m, 106.0m),
            WasteReport.Create(citizen1.Id, 1, 10.1m, 106.1m),
            WasteReport.Create(citizen1.Id, 1, 10.2m, 106.2m),
            WasteReport.Create(citizen2.Id, 1, 11.0m, 107.0m));
        await context.SaveChangesAsync();

        // Act — page 1, size 2
        var (reports, total) = await repository.GetByCitizenIdAsync(citizen1.Id, page: 1, pageSize: 2);

        // Assert
        total.Should().Be(3);
        reports.Should().HaveCount(2);
        reports.All(r => r.CitizenId == citizen1.Id).Should().BeTrue();

        AllureAttachmentHelper.AttachText("get-by-citizen-id--paginated", $"Test: GetByCitizenIdAsync — Total={total}, PageCount={reports.Count()} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetAllAsync should return all reports when no status filter is applied.")]
    public async Task GetAllAsync_WithoutStatusFilter_ShouldReturnAllReports()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ReportRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen", UserRole.Citizen);
        context.Users.Add(citizen);

        context.WasteReports.AddRange(
            WasteReport.Create(citizen.Id, 1, 10.0m, 106.0m),
            WasteReport.Create(citizen.Id, 1, 10.1m, 106.1m));
        await context.SaveChangesAsync();

        // Act
        var (reports, total) = await repository.GetAllAsync(page: 1, pageSize: 10, status: null);

        // Assert
        total.Should().Be(2);
        reports.Should().HaveCount(2);

        AllureAttachmentHelper.AttachText("get-all--no-filter", $"Test: GetAllAsync_WithoutStatusFilter — Total={total} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetAllAsync should return only reports matching the specified status filter.")]
    public async Task GetAllAsync_WithStatusFilter_ShouldReturnFilteredReports()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ReportRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen", UserRole.Citizen);
        context.Users.Add(citizen);

        var pendingReport = WasteReport.Create(citizen.Id, 1, 10.0m, 106.0m);
        var acceptedReport = WasteReport.Create(citizen.Id, 1, 10.1m, 106.1m);
        acceptedReport.Accept(); // Transition from Pending → Accepted

        context.WasteReports.AddRange(pendingReport, acceptedReport);
        await context.SaveChangesAsync();

        // Act
        var (reports, total) = await repository.GetAllAsync(page: 1, pageSize: 10, status: ReportStatus.Accepted);

        // Assert
        total.Should().Be(1);
        reports.Should().HaveCount(1);
        reports.First().Status.Should().Be(ReportStatus.Accepted);

        AllureAttachmentHelper.AttachText("get-all--status-filter", $"Test: GetAllAsync_WithStatusFilter — Accepted={total} — passed ✅");
    }

    [Fact]
    [AllureDescription("GetEnterpriseReportsAsync should return reports matching the enterprise's waste categories that have no collection task.")]
    public async Task GetEnterpriseReportsAsync_ShouldReturnMatchingUnassignedReports()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ReportRepository(context);

        var enterpriseUser = User.Create("enterprise@test.com", "hash", "Enterprise User", UserRole.Enterprise);
        var citizenUser = User.Create("citizen@test.com", "hash", "Citizen User", UserRole.Citizen);
        context.Users.AddRange(enterpriseUser, citizenUser);

        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = enterpriseUser.Id,
            CompanyName = "Eco Corp"
        };
        context.Enterprises.Add(enterprise);

        var category = new WasteCategory { Id = 1, Name = "Plastic", Description = "Plastic waste" };
        context.WasteCategories.Add(category);

        // Enterprise handles waste category 1
        context.EnterpriseWasteTypes.Add(new EnterpriseWasteType
        {
            Id = Guid.NewGuid(),
            EnterpriseId = enterprise.Id,
            WasteCategoryId = 1
        });

        // Report with matching category (no CollectionTask) — should be returned
        var matchingReport = WasteReport.Create(citizenUser.Id, 1, 10.0m, 106.0m);
        // Report with non-matching category — should NOT be returned
        var nonMatchingReport = WasteReport.Create(citizenUser.Id, 99, 10.1m, 106.1m);

        context.WasteReports.AddRange(matchingReport, nonMatchingReport);
        await context.SaveChangesAsync();

        // Act
        var (reports, total) = await repository.GetEnterpriseReportsAsync(enterprise.Id, page: 1, pageSize: 10, status: null);

        // Assert
        total.Should().Be(1);
        reports.Should().HaveCount(1);
        reports.First().Id.Should().Be(matchingReport.Id);

        AllureAttachmentHelper.AttachText("get-enterprise-reports", $"Test: GetEnterpriseReportsAsync — Matched={total} — passed ✅");
    }

    [Fact]
    [AllureDescription("SaveChangesAsync should persist pending entity changes to the database.")]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new ReportRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen", UserRole.Citizen);
        context.Users.Add(citizen);
        await context.SaveChangesAsync();

        var report = WasteReport.Create(citizen.Id, 1, 10.0m, 106.0m);
        await repository.AddAsync(report);

        // Act
        await repository.SaveChangesAsync();

        // Assert
        (await context.WasteReports.CountAsync()).Should().Be(1);
        var persisted = await context.WasteReports.FirstAsync();
        persisted.Id.Should().Be(report.Id);

        AllureAttachmentHelper.AttachText("save-changes", "Test: SaveChangesAsync_ShouldPersistChanges — passed ✅");
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
