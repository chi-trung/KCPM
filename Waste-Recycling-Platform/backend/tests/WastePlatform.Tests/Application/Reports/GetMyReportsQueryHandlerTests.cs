using Allure.Xunit.Attributes;
using FluentAssertions;
using Moq;
using WastePlatform.Application.Reports.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using Xunit;

namespace WastePlatform.Tests.Application.Reports;

/// <summary>
/// Unit tests for GetMyReportsQueryHandler
/// TC-REP-003: Get My Reports (Citizen's own reports)
/// </summary>
[AllureEpic("KIEM-5: Reports Module Testing")]
[AllureFeature("Get My Reports Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen retrieves their own waste reports with pagination")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "GetMyReportsQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Reports")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-5")]
public class GetMyReportsQueryHandlerTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly GetMyReportsQueryHandler _handler;

    public GetMyReportsQueryHandlerTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _handler = new GetMyReportsQueryHandler(_mockReportRepository.Object);
    }

    #region Happy Path - Get My Reports

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnOnlyUserReports()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var myReports = new List<WasteReport>
        {
            CreateReport(userId, ReportStatus.Pending),
            CreateReport(userId, ReportStatus.Accepted)
        };

        _mockReportRepository
            .Setup(x => x.GetByCitizenIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((myReports, 2));

        var query = new GetMyReportsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Reports.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.TotalPages.Should().Be(1);
        _mockReportRepository.Verify(
            x => x.GetByCitizenIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange - User has no reports
        var userId = Guid.NewGuid();

        _mockReportRepository
            .Setup(x => x.GetByCitizenIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<WasteReport>(), 0));

        var query = new GetMyReportsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Reports.Should().BeEmpty();
        result.Total.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task Handle_WithCustomPagination_ShouldApplyPagination()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reports = new List<WasteReport> { CreateReport(userId, ReportStatus.Pending) };

        _mockReportRepository
            .Setup(x => x.GetByCitizenIdAsync(userId, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 12)); // 12 total, page 2, size 5

        var query = new GetMyReportsQuery 
        { 
            UserId = userId, 
            Page = 2, 
            PageSize = 5 
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Reports.Should().HaveCount(1);
        result.Total.Should().Be(12);
        result.TotalPages.Should().Be(3); // ceil(12/5) = 3
    }

    #endregion

    #region Data Transformation Tests

    [Fact]
    public async Task Handle_ShouldMapReportToReportListDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var citizen = User.Create("test@test.com", "password123", "Test Citizen", UserRole.Citizen, "0901234567");
        typeof(User).GetProperty("Id")?.SetValue(citizen, userId);
        var category = new WasteCategory { Id = 1, Name = "Rác hữu cơ" };
        
        var report = WasteReport.Create(
            citizenId: userId,
            wasteCategoryId: 1,
            latitude: 10.7769m,
            longitude: 106.7009m,
            description: "Test report",
            address: "123 Nguyễn Trãi",
            aiSuggestion: "Recyclable");
        
        // Add images
        report.Images.Add(new ReportImage 
        { 
            Id = Guid.NewGuid(), 
            ReportId = report.Id, 
            ImageUrl = "image1.jpg",
            UploadedAt = DateTime.UtcNow 
        });
        report.Images.Add(new ReportImage 
        { 
            Id = Guid.NewGuid(), 
            ReportId = report.Id, 
            ImageUrl = "image2.jpg",
            UploadedAt = DateTime.UtcNow 
        });

        typeof(WasteReport).GetProperty("Citizen")?.SetValue(report, citizen);
        typeof(WasteReport).GetProperty("WasteCategory")?.SetValue(report, category);

        _mockReportRepository
            .Setup(x => x.GetByCitizenIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<WasteReport> { report }, 1));

        var query = new GetMyReportsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Reports.First();
        dto.Id.Should().Be(report.Id);
        dto.CitizenName.Should().Be("Test Citizen");
        dto.CategoryName.Should().Be("Rác hữu cơ");
        dto.Status.Should().Be(ReportStatus.Pending);
        dto.Address.Should().Be("123 Nguyễn Trãi");
        dto.ImageCount.Should().Be(2);
        dto.CreatedAt.Should().Be(report.CreatedAt);
    }

    #endregion

    #region Helper Methods

    private static WasteReport CreateReport(Guid citizenId, ReportStatus status)
    {
        var report = WasteReport.Create(
            citizenId: citizenId,
            wasteCategoryId: 1,
            latitude: 10m,
            longitude: 106m,
            description: "Test report",
            address: "Test address",
            aiSuggestion: "Mixed");

        switch (status)
        {
            case ReportStatus.Accepted:
                report.Accept();
                break;
            case ReportStatus.Rejected:
                report.Reject();
                break;
            case ReportStatus.Assigned:
                report.Accept();
                report.Assign();
                break;
            case ReportStatus.Collected:
                report.Accept();
                report.Assign();
                report.Collect();
                break;
        }

        return report;
    }

    #endregion
}
