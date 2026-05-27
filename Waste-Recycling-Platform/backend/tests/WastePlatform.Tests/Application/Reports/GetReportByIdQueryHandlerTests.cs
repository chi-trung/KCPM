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
/// Unit tests for GetReportByIdQueryHandler
/// TC-REP-003: Get Report by ID - Valid Request
/// TC-REP-004: Get Report by ID - Invalid/Non-existent ID
/// </summary>
[AllureEpic("KIEM-5: Reports Module Testing")]
[AllureFeature("Get Report By ID Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Retrieve a specific waste report by its ID")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "GetReportByIdQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Reports")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-5")]
public class GetReportByIdQueryHandlerTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly GetReportByIdQueryHandler _handler;

    public GetReportByIdQueryHandlerTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _handler = new GetReportByIdQueryHandler(_mockReportRepository.Object);
    }

    #region TC-REP-003: Happy Path - Get Existing Report

    [Fact]
    public async Task Handle_WhenReportExists_ShouldReturnReportDto()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var citizen = User.Create("test@test.com", "password123", "Test Citizen", UserRole.Citizen, "0901234567");
        typeof(User).GetProperty("Id")?.SetValue(citizen, citizenId);
        var category = new WasteCategory { Id = 1, Name = "Rác hữu cơ" };
        
        var report = WasteReport.Create(
            citizenId: citizenId,
            wasteCategoryId: 1,
            latitude: 10.7769m,
            longitude: 106.7009m,
            description: "Rác thải sinh hoạt",
            address: "123 Nguyễn Trãi, Q.1",
            aiSuggestion: "Recyclable");
        
        // Use reflection to set navigation properties (normally set by EF Core)
        typeof(WasteReport).GetProperty("Citizen")?.SetValue(report, citizen);
        typeof(WasteReport).GetProperty("WasteCategory")?.SetValue(report, category);

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var query = new GetReportByIdQuery { Id = reportId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(report.Id);
        result.CitizenId.Should().Be(citizenId);
        result.CitizenName.Should().Be("Test Citizen");
        result.WasteCategoryId.Should().Be(1);
        result.CategoryName.Should().Be("Rác hữu cơ");
        result.Description.Should().Be("Rác thải sinh hoạt");
        result.Latitude.Should().Be(10.7769m);
        result.Longitude.Should().Be(106.7009m);
        result.Address.Should().Be("123 Nguyễn Trãi, Q.1");
        result.Status.Should().Be(ReportStatus.Pending);
        result.AiSuggestion.Should().Be("Recyclable");
        result.ImageUrls.Should().BeEmpty();
        result.RewardPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenReportExists_WithImages_ShouldReturnReportDtoWithImageUrls()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var report = WasteReport.Create(
            citizenId: Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude: 10m,
            longitude: 106m,
            description: "Test",
            address: "Test",
            aiSuggestion: "Mixed");
        
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

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var query = new GetReportByIdQuery { Id = reportId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ImageUrls.Should().HaveCount(2);
        result.ImageUrls.Should().Contain("image1.jpg");
        result.ImageUrls.Should().Contain("image2.jpg");
    }

    #endregion

    #region TC-REP-004: Report Not Found

    [Fact]
    public async Task Handle_WhenReportDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentReportId = Guid.NewGuid();

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(nonExistentReportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport?)null);

        var query = new GetReportByIdQuery { Id = nonExistentReportId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region TC-REP-003: Data Integrity Tests

    [Theory]
    [InlineData(ReportStatus.Pending)]
    [InlineData(ReportStatus.Accepted)]
    [InlineData(ReportStatus.Rejected)]
    [InlineData(ReportStatus.Assigned)]
    [InlineData(ReportStatus.Collected)]
    public async Task Handle_WhenReportExists_ShouldReturnCorrectStatus(ReportStatus status)
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var report = WasteReport.Create(
            citizenId: Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude: 10m,
            longitude: 106m,
            description: "Test",
            address: "Test",
            aiSuggestion: "Mixed");
        
        // Set status based on parameter
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

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var query = new GetReportByIdQuery { Id = reportId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(status);
    }

    #endregion
}
