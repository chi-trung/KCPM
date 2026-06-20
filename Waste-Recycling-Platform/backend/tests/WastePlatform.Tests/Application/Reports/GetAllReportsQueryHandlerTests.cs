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
/// Unit tests for GetAllReportsQueryHandler
/// TC-REP-003: Get All Reports with Pagination and Filtering
/// </summary>
[AllureEpic("KIEM-5: Reports Module Testing")]
[AllureFeature("Get All Reports Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Admin or Enterprise retrieves all waste reports with pagination and filtering")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "GetAllReportsQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Reports")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
[Allure.Net.Commons.Attributes.AllureIssue("KIEM-5")]
public class GetAllReportsQueryHandlerTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly GetAllReportsQueryHandler _handler;

    public GetAllReportsQueryHandlerTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _handler = new GetAllReportsQueryHandler(_mockReportRepository.Object);
    }

    #region Happy Path - Get All Reports

    [Fact]
    [AllureDescription("H a n d l e - W i t h D e f a u l t P a g i n a t i o n S h o u l d R e t u r n R e p o r t s")]
    public async Task Handle_WithDefaultPagination_ShouldReturnReports()
    {
        AllureAttachmentHelper.AttachText("test-scenario", "executed-test");
        // Arrange
        var reports = new List<WasteReport>
        {
            CreateReport(ReportStatus.Pending),
            CreateReport(ReportStatus.Accepted),
            CreateReport(ReportStatus.Rejected)
        };

        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 3));

        var query = new GetAllReportsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Reports.Should().HaveCount(3);
        result.Total.Should().Be(3);
        result.TotalPages.Should().Be(1); // 3 items / 10 pageSize = 1 page
    }

    [Fact]
    [AllureDescription("H a n d l e - W i t h C u s t o m P a g i n a t i o n S h o u l d R e t u r n C o r r e c t P a g e")]
    public async Task Handle_WithCustomPagination_ShouldReturnCorrectPage()
    {
        AllureAttachmentHelper.AttachText("test-scenario", "executed-test");
        // Arrange
        var reports = new List<WasteReport> { CreateReport(ReportStatus.Pending) };

        _mockReportRepository
            .Setup(x => x.GetAllAsync(2, 5, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 11)); // 11 total, page 2 with size 5

        var query = new GetAllReportsQuery { Page = 2, PageSize = 5 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Reports.Should().HaveCount(1);
        result.Total.Should().Be(11);
        result.TotalPages.Should().Be(3); // ceil(11/5) = 3
    }

    #endregion

    #region Filtering by Status

    [Theory]
    [InlineData("Pending", ReportStatus.Pending)]
    [InlineData("Accepted", ReportStatus.Accepted)]
    [InlineData("Rejected", ReportStatus.Rejected)]
    [InlineData("Assigned", ReportStatus.Assigned)]
    [InlineData("Collected", ReportStatus.Collected)]
    [AllureDescription("H a n d l e - W i t h S t a t u s F i l t e r S h o u l d F i l t e r B y S t a t u s")]
    [InlineData("Pending", ReportStatus.Pending)]
    [InlineData("Accepted", ReportStatus.Accepted)]
    [InlineData("Rejected", ReportStatus.Rejected)]
    [InlineData("Assigned", ReportStatus.Assigned)]
    [InlineData("Collected", ReportStatus.Collected)]
    public async Task Handle_WithStatusFilter_ShouldFilterByStatus(string statusString, ReportStatus expectedStatus)
    {
        // Arrange
        var reports = new List<WasteReport> { CreateReport(expectedStatus) };

        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 10, expectedStatus, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 1));

        var query = new GetAllReportsQuery { Status = statusString };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Reports.Should().HaveCount(1);
        _mockReportRepository.Verify(
            x => x.GetAllAsync(1, 10, expectedStatus, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("H a n d l e - W i t h E m p t y S t a t u s F i l t e r S h o u l d N o t F i l t e r B y S t a t u s")]
    public async Task Handle_WithEmptyStatusFilter_ShouldNotFilterByStatus()
    {
        AllureAttachmentHelper.AttachText("test-scenario", "executed-test");
        // Arrange
        var reports = new List<WasteReport> { CreateReport(ReportStatus.Pending) };

        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 1));

        var query = new GetAllReportsQuery { Status = "" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReportRepository.Verify(
            x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("H a n d l e - W i t h I n v a l i d S t a t u s F i l t e r S h o u l d N o t F i l t e r B y S t a t u s")]
    public async Task Handle_WithInvalidStatusFilter_ShouldNotFilterByStatus()
    {
        AllureAttachmentHelper.AttachText("test-scenario", "executed-test");
        // Arrange - Invalid status string should be ignored
        var reports = new List<WasteReport> { CreateReport(ReportStatus.Pending) };

        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, 1));

        var query = new GetAllReportsQuery { Status = "InvalidStatus" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - Should call with null status (no filter)
        _mockReportRepository.Verify(
            x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Pagination Edge Cases

    [Theory]
    [InlineData(1, 10, 100, 10)]  // 100 items, page size 10 = 10 pages
    [InlineData(1, 20, 100, 5)]   // 100 items, page size 20 = 5 pages
    [InlineData(1, 50, 100, 2)]   // 100 items, page size 50 = 2 pages
    [InlineData(1, 100, 100, 1)]  // 100 items, page size 100 = 1 page
    [InlineData(1, 10, 5, 1)]     // 5 items, page size 10 = 1 page
    [InlineData(1, 10, 0, 0)]     // 0 items = 0 pages
    public async Task Handle_ShouldCalculateTotalPagesCorrectly(int page, int pageSize, int totalItems, int expectedPages)
    {
        // Arrange
        var reports = Enumerable.Range(0, Math.Min(totalItems, pageSize))
            .Select(_ => CreateReport(ReportStatus.Pending))
            .ToList();

        _mockReportRepository
            .Setup(x => x.GetAllAsync(page, pageSize, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reports, totalItems));

        var query = new GetAllReportsQuery { Page = page, PageSize = pageSize };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalPages.Should().Be(expectedPages);
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

        return report;
    }

    #endregion
}

