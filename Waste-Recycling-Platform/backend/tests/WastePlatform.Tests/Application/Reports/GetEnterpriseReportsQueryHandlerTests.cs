using Allure.Xunit.Attributes;
using FluentAssertions;
using Moq;
using WastePlatform.Application.Reports.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using Xunit;

using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Reports;

/// <summary>
/// Unit tests for GetEnterpriseReportsQueryHandler
/// TC-REP-005: Get Enterprise Reports (Reports that enterprise can handle)
/// </summary>
[AllureEpic("KIEM-5: Reports Module Testing")]
[AllureFeature("Get Enterprise Reports Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise retrieves waste reports within their service area")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "GetEnterpriseReportsQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Reports")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-5")]
public class GetEnterpriseReportsQueryHandlerTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly GetEnterpriseReportsQueryHandler _handler;

    public GetEnterpriseReportsQueryHandlerTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _handler = new GetEnterpriseReportsQueryHandler(_mockReportRepository.Object);
    }

    #region Happy Path - Get Enterprise Reports

    [Fact]
    [AllureDescription("H a n d l e - W i t h V a l i d E n t e r p r i s e I d S h o u l d R e t u r n R e p o r t s")]
    public async Task Handle_WithValidEnterpriseId_ShouldReturnReports()
    {
        AllureAttachmentHelper.AttachText("test-scenario", "executed-test");
        // Arrange
        var enterpriseId = Guid.NewGuid();
        var reports = new List<WasteReport>
        {
            CreateReport(ReportStatus.Pending),
            CreateReport(ReportStatus.Accepted)
        };

        _mockReportRepository
            .Setup(x => x.GetEnterpriseReportsAsync(enterpriseId, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 2));

        var query = new GetEnterpriseReportsQuery { EnterpriseId = enterpriseId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Reports.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.TotalPages.Should().Be(1);
        _mockReportRepository.Verify(
            x => x.GetEnterpriseReportsAsync(enterpriseId, 1, 10, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Status Filtering

    [Theory]
    [InlineData("Pending", ReportStatus.Pending)]
    [InlineData("Accepted", ReportStatus.Accepted)]
    [InlineData("Rejected", ReportStatus.Rejected)]
    [AllureDescription("H a n d l e - W i t h S t a t u s F i l t e r S h o u l d F i l t e r B y S t a t u s")]
    [InlineData("Pending", ReportStatus.Pending)]
    [InlineData("Accepted", ReportStatus.Accepted)]
    [InlineData("Rejected", ReportStatus.Rejected)]
    public async Task Handle_WithStatusFilter_ShouldFilterByStatus(string statusString, ReportStatus expectedStatus)
    {
        // Arrange
        var enterpriseId = Guid.NewGuid();
        var reports = new List<WasteReport> { CreateReport(expectedStatus) };

        _mockReportRepository
            .Setup(x => x.GetEnterpriseReportsAsync(enterpriseId, 1, 10, expectedStatus, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 1));

        var query = new GetEnterpriseReportsQuery 
        { 
            EnterpriseId = enterpriseId, 
            Status = statusString 
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Reports.Should().HaveCount(1);
        _mockReportRepository.Verify(
            x => x.GetEnterpriseReportsAsync(enterpriseId, 1, 10, expectedStatus, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("H a n d l e - W i t h E m p t y S t a t u s S h o u l d N o t F i l t e r B y S t a t u s")]
    public async Task Handle_WithEmptyStatus_ShouldNotFilterByStatus()
    {
        AllureAttachmentHelper.AttachText("test-scenario", "executed-test");
        // Arrange
        var enterpriseId = Guid.NewGuid();
        var reports = new List<WasteReport> { CreateReport(ReportStatus.Pending) };

        _mockReportRepository
            .Setup(x => x.GetEnterpriseReportsAsync(enterpriseId, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 1));

        var query = new GetEnterpriseReportsQuery 
        { 
            EnterpriseId = enterpriseId, 
            Status = "" 
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReportRepository.Verify(
            x => x.GetEnterpriseReportsAsync(enterpriseId, 1, 10, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("H a n d l e - W i t h I n v a l i d S t a t u s S h o u l d N o t F i l t e r B y S t a t u s")]
    public async Task Handle_WithInvalidStatus_ShouldNotFilterByStatus()
    {
        AllureAttachmentHelper.AttachText("test-scenario", "executed-test");
        // Arrange - Invalid status should be ignored
        var enterpriseId = Guid.NewGuid();
        var reports = new List<WasteReport> { CreateReport(ReportStatus.Pending) };

        _mockReportRepository
            .Setup(x => x.GetEnterpriseReportsAsync(enterpriseId, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 1));

        var query = new GetEnterpriseReportsQuery 
        { 
            EnterpriseId = enterpriseId, 
            Status = "InvalidStatus" 
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - Should call with null status (no filter)
        _mockReportRepository.Verify(
            x => x.GetEnterpriseReportsAsync(enterpriseId, 1, 10, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    [AllureDescription("H a n d l e - W i t h C u s t o m P a g i n a t i o n S h o u l d A p p l y P a g i n a t i o n")]
    public async Task Handle_WithCustomPagination_ShouldApplyPagination()
    {
        AllureAttachmentHelper.AttachText("test-scenario", "executed-test");
        // Arrange
        var enterpriseId = Guid.NewGuid();
        var reports = new List<WasteReport> { CreateReport(ReportStatus.Pending) };

        _mockReportRepository
            .Setup(x => x.GetEnterpriseReportsAsync(enterpriseId, 3, 15, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 45)); // 45 total, page 3, size 15

        var query = new GetEnterpriseReportsQuery 
        { 
            EnterpriseId = enterpriseId, 
            Page = 3, 
            PageSize = 15 
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Reports.Should().HaveCount(1);
        result.Total.Should().Be(45);
        result.TotalPages.Should().Be(3); // ceil(45/15) = 3
    }

    #endregion

    #region Helper Methods

    private static WasteReport CreateReport(ReportStatus status)
    {
        var report = WasteReport.Create(
            citizenId: Guid.NewGuid(),
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

