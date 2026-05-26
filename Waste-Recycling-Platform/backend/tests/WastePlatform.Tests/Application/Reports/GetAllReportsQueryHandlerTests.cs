using FluentAssertions;
using Moq;
using WastePlatform.Application.Reports.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using Xunit;

namespace WastePlatform.Tests.Application.Reports;

/// <summary>
/// Unit tests for GetAllReportsQueryHandler
/// TC-REP-003: Get All Reports with Pagination and Filtering
/// </summary>
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
    public async Task Handle_WithDefaultPagination_ShouldReturnReports()
    {
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
    public async Task Handle_WithCustomPagination_ShouldReturnCorrectPage()
    {
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
    public async Task Handle_WithEmptyStatusFilter_ShouldNotFilterByStatus()
    {
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
    public async Task Handle_WithInvalidStatusFilter_ShouldNotFilterByStatus()
    {
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
