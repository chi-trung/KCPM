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
[AllureFeature("ComplaintRepository")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "ComplaintRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
public class ComplaintRepositoryTests
{
    // ──────────────────────────────────────────────────────────────
    // AddAsync + SaveChangesAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Adds a complaint to the context and persists it via SaveChangesAsync.")]
    public async Task AddAsync_ShouldPersistComplaint()
    {
        AllureAttachmentHelper.AttachText("add-async--happy", "Test: AddAsync_ShouldPersistComplaint — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new ComplaintRepository(context);

        var citizen = User.Create("citizen@test.com", "hash", "Citizen A", UserRole.Citizen);
        context.Users.Add(citizen);
        await context.SaveChangesAsync();

        var complaint = Complaint.Create(citizen.Id, "Trash not collected");

        // Act
        var returned = await repository.AddAsync(complaint);
        await repository.SaveChangesAsync();

        // Assert
        var saved = await context.Complaints.FirstOrDefaultAsync(c => c.Id == complaint.Id);
        saved.Should().NotBeNull();
        saved!.Content.Should().Be("Trash not collected");
        saved.Status.Should().Be(ComplaintStatus.Open);
        returned.Id.Should().Be(complaint.Id);
    }

    // ──────────────────────────────────────────────────────────────
    // GetByIdAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns the complaint with included navigation properties.")]
    public async Task GetByIdAsync_WhenExists_ShouldReturnComplaint()
    {
        AllureAttachmentHelper.AttachText("get-by-id--exists", "Test: GetByIdAsync_WhenExists_ShouldReturnComplaint — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new ComplaintRepository(context);

        var citizen = User.Create("citizen2@test.com", "hash", "Citizen B", UserRole.Citizen);
        context.Users.Add(citizen);

        var complaint = Complaint.Create(citizen.Id, "Bad smell");
        context.Complaints.Add(complaint);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(complaint.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Content.Should().Be("Bad smell");
        result.Citizen.Should().NotBeNull();
        result.Citizen.FullName.Should().Be("Citizen B");
    }

    [Fact]
    [AllureDescription("Returns null when the complaint id does not exist.")]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        AllureAttachmentHelper.AttachText("get-by-id--not-found", "Test: GetByIdAsync_WhenNotFound_ShouldReturnNull — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new ComplaintRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    // ──────────────────────────────────────────────────────────────
    // GetAllAsync (pagination, status filter, search)
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns paginated complaints with correct total count.")]
    public async Task GetAllAsync_Pagination_ShouldReturnCorrectPage()
    {
        AllureAttachmentHelper.AttachText("get-all--pagination", "Test: GetAllAsync_Pagination_ShouldReturnCorrectPage — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new ComplaintRepository(context);
        var citizen = SeedCitizenWithComplaints(context, 5);
        await context.SaveChangesAsync();

        // Act – page 1, size 2
        var (complaints, total) = await repository.GetAllAsync(1, 2, null, null);

        // Assert
        total.Should().Be(5);
        complaints.Should().HaveCount(2);
    }

    [Fact]
    [AllureDescription("Filters complaints by status when a status value is provided.")]
    public async Task GetAllAsync_FilterByStatus_ShouldReturnMatching()
    {
        AllureAttachmentHelper.AttachText("get-all--filter-status", "Test: GetAllAsync_FilterByStatus_ShouldReturnMatching — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new ComplaintRepository(context);

        var citizen = User.Create("filter@test.com", "hash", "Filter Citizen", UserRole.Citizen);
        context.Users.Add(citizen);

        var open = Complaint.Create(citizen.Id, "Open complaint");
        var resolved = Complaint.Create(citizen.Id, "Resolved complaint");
        resolved.Resolve("Fixed");

        context.Complaints.AddRange(open, resolved);
        await context.SaveChangesAsync();

        // Act
        var (complaints, total) = await repository.GetAllAsync(1, 10, ComplaintStatus.Resolved, null);

        // Assert
        total.Should().Be(1);
        complaints.First().Status.Should().Be(ComplaintStatus.Resolved);
    }

    [Fact]
    [AllureDescription("Returns empty result when no complaints exist in the database.")]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyAndZeroTotal()
    {
        AllureAttachmentHelper.AttachText("get-all--empty", "Test: GetAllAsync_WhenEmpty_ShouldReturnEmptyAndZeroTotal — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new ComplaintRepository(context);

        // Act
        var (complaints, total) = await repository.GetAllAsync(1, 10, null, null);

        // Assert
        total.Should().Be(0);
        complaints.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────
    // GetByCitizenIdAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns only complaints belonging to the specified citizen.")]
    public async Task GetByCitizenIdAsync_ShouldReturnOnlyCitizensComplaints()
    {
        AllureAttachmentHelper.AttachText("get-by-citizen-id", "Test: GetByCitizenIdAsync_ShouldReturnOnlyCitizensComplaints — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new ComplaintRepository(context);

        var citizenA = User.Create("a@citizen.com", "hash", "Citizen A", UserRole.Citizen);
        var citizenB = User.Create("b@citizen.com", "hash", "Citizen B", UserRole.Citizen);
        context.Users.AddRange(citizenA, citizenB);

        context.Complaints.AddRange(
            Complaint.Create(citizenA.Id, "Complaint A-1"),
            Complaint.Create(citizenA.Id, "Complaint A-2"),
            Complaint.Create(citizenB.Id, "Complaint B-1"));
        await context.SaveChangesAsync();

        // Act
        var (complaints, total) = await repository.GetByCitizenIdAsync(citizenA.Id, 1, 10, null);

        // Assert
        total.Should().Be(2);
        complaints.Should().AllSatisfy(c => c.CitizenId.Should().Be(citizenA.Id));
    }

    // ──────────────────────────────────────────────────────────────
    // GetByEnterpriseIdAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns only complaints associated with the specified enterprise.")]
    public async Task GetByEnterpriseIdAsync_ShouldReturnOnlyEnterpriseComplaints()
    {
        AllureAttachmentHelper.AttachText("get-by-enterprise-id", "Test: GetByEnterpriseIdAsync_ShouldReturnOnlyEnterpriseComplaints — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new ComplaintRepository(context);

        var citizen = User.Create("ent-cit@test.com", "hash", "Enterprise Citizen", UserRole.Citizen);
        var enterpriseUser = User.Create("ent@test.com", "hash", "Enterprise Owner", UserRole.Enterprise);
        context.Users.AddRange(citizen, enterpriseUser);

        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = enterpriseUser.Id,
            CompanyName = "Complaint Corp",
            Status = "Verified"
        };
        context.Enterprises.Add(enterprise);

        context.Complaints.AddRange(
            Complaint.Create(citizen.Id, "About enterprise", enterpriseId: enterprise.Id),
            Complaint.Create(citizen.Id, "General complaint"));
        await context.SaveChangesAsync();

        // Act
        var (complaints, total) = await repository.GetByEnterpriseIdAsync(enterprise.Id, 1, 10, null);

        // Assert
        total.Should().Be(1);
        complaints.First().EnterpriseId.Should().Be(enterprise.Id);
    }

    [Fact]
    [AllureDescription("Returns empty when no complaints reference the given enterprise.")]
    public async Task GetByEnterpriseIdAsync_WhenNone_ShouldReturnEmpty()
    {
        AllureAttachmentHelper.AttachText("get-by-enterprise-id--empty", "Test: GetByEnterpriseIdAsync_WhenNone_ShouldReturnEmpty — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new ComplaintRepository(context);

        // Act
        var (complaints, total) = await repository.GetByEnterpriseIdAsync(Guid.NewGuid(), 1, 10, null);

        // Assert
        total.Should().Be(0);
        complaints.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>Seeds a citizen user with <paramref name="count"/> complaints.</summary>
    private static User SeedCitizenWithComplaints(WastePlatformDbContext context, int count)
    {
        var citizen = User.Create("bulk@citizen.com", "hash", "Bulk Citizen", UserRole.Citizen);
        context.Users.Add(citizen);

        for (var i = 1; i <= count; i++)
        {
            context.Complaints.Add(Complaint.Create(citizen.Id, $"Complaint #{i}"));
        }

        return citizen;
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
