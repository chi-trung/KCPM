using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Moq;
using WastePlatform.Application.Admin.Complaints.Queries;
using WastePlatform.Application.Admin.Complaints.DTOs;
using WastePlatform.Application.Reports.Queries;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Application.Search;

#region Search + Filter + Pagination Tests

[AllureEpic("Search & Discovery")]
[AllureFeature("Complaints Search")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Search complaints with filters and pagination")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "SearchPaginationFiltersTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Search")]
[Allure.Net.Commons.Attributes.AllureLabel("KIEM", "KIEM-23")]
[Allure.Net.Commons.Attributes.AllureLabel("WRP", "WRP-BE-TESTS-020")]
[AllureOwner("11A6_03_Đăng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("search")]
[Allure.Net.Commons.Attributes.AllureTag("pagination")]
[Allure.Net.Commons.Attributes.AllureTag("filter")]
public class ComplaintsSearchQueryHandlerTests
{
    private readonly Mock<IComplaintRepository> _mockComplaintRepository;
    private readonly GetComplaintsQueryHandler _handler;

    public ComplaintsSearchQueryHandlerTests()
    {
        _mockComplaintRepository = new Mock<IComplaintRepository>();
        _handler = new GetComplaintsQueryHandler(_mockComplaintRepository.Object);
    }

    [Fact]
    [AllureDescription("Search complaints with keyword and filter by status")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-SEARCH-001")]
    public async Task Handle_WithSearchTermAndStatusFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var query = new GetComplaintsQuery
        {
            Page = 1,
            PageSize = 10,
            Status = "Open",
            SearchTerm = "garbage"
        };

        var citizenId = Guid.NewGuid();
        var complaints = new List<Complaint>
        {
            Complaint.Create(citizenId, "Garbage disposal not working properly"),
            Complaint.Create(citizenId, "Garbage accumulation on street"),
            Complaint.Create(citizenId, "Recycling bin garbage overflow"),
        };

        _mockComplaintRepository
            .Setup(x => x.GetAllAsync(
                query.Page,
                query.PageSize,
                ComplaintStatus.Open,
                "garbage",
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<Complaint>, int)>((complaints, 3)));

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().HaveCount(3);
        total.Should().Be(3);
        totalPages.Should().Be(1);
        results.All(c => c.Status == ComplaintStatus.Open).Should().BeTrue();

        AllureAttachmentHelper.AttachJson("search-query", query);
        AllureAttachmentHelper.AttachJson("search-results", new { Results = results, Total = total, TotalPages = totalPages });

        _mockComplaintRepository.Verify(
            x => x.GetAllAsync(1, 10, ComplaintStatus.Open, "garbage", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Returns empty results when search term matches no complaints")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-SEARCH-002")]
    public async Task Handle_WithNonMatchingSearchTerm_ShouldReturnEmptyResults()
    {
        // Arrange
        var query = new GetComplaintsQuery
        {
            Page = 1,
            PageSize = 10,
            SearchTerm = "nonexistent_keyword_xyz"
        };

        _mockComplaintRepository
            .Setup(x => x.GetAllAsync(
                query.Page,
                query.PageSize,
                null,
                "nonexistent_keyword_xyz",
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<Complaint>, int)>((new List<Complaint>(), 0)));

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().BeEmpty();
        total.Should().Be(0);
        totalPages.Should().Be(0);

        AllureAttachmentHelper.AttachJson("empty-search-results", new { Total = total, TotalPages = totalPages });
    }

    [Fact]
    [AllureDescription("Filters complaints by status without search term")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-SEARCH-003")]
    public async Task Handle_WithStatusFilterOnly_ShouldReturnAllComplaintsWithStatus()
    {
        // Arrange
        var query = new GetComplaintsQuery
        {
            Page = 1,
            PageSize = 10,
            Status = "Resolved"
        };

        var citizenId = Guid.NewGuid();
        var resolvedComplaints = new List<Complaint>
        {
            Complaint.Create(citizenId, "Issue resolved"),
            Complaint.Create(citizenId, "Problem fixed")
        };

        _mockComplaintRepository
            .Setup(x => x.GetAllAsync(
                1, 10, ComplaintStatus.Resolved, null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<Complaint>, int)>((resolvedComplaints, 2)));

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        total.Should().Be(2);
        totalPages.Should().Be(1);

        AllureAttachmentHelper.AttachJson("status-filter-results", new { Results = results, Total = total, TotalPages = totalPages });

        // Verify the repository was called with the correct status filter
        _mockComplaintRepository.Verify(
            x => x.GetAllAsync(1, 10, ComplaintStatus.Resolved, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

#endregion

#region Pagination Tests

[AllureEpic("Search & Discovery")]
[AllureFeature("Pagination")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Handle pagination and page navigation")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "PaginationTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Search")]
[Allure.Net.Commons.Attributes.AllureLabel("KIEM", "KIEM-23")]
[AllureOwner("11A6_03_Đăng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("pagination")]
public class ReportsPaginationTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly GetAllReportsQueryHandler _handler;

    public ReportsPaginationTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _handler = new GetAllReportsQueryHandler(_mockReportRepository.Object);
    }

    [Fact]
    [AllureDescription("Calculates total pages correctly based on page size")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-PAGINATE-001")]
    public async Task Handle_WithPagination_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var query = new GetAllReportsQuery { Page = 1, PageSize = 5 };
        
        var citizenId = Guid.NewGuid();
        var reports = Enumerable.Range(1, 5)
            .Select(_ => WasteReport.Create(citizenId, 1, 10.5m, 106.5m, "Test report"))
            .ToList();

        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 5, null, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<WasteReport>, int)>((reports, 23))); // 23 total reports

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().HaveCount(5);
        total.Should().Be(23);
        totalPages.Should().Be(5); // ceil(23/5) = 5

        AllureAttachmentHelper.AttachJson("pagination-metadata", new { Total = total, PageSize = query.PageSize, TotalPages = totalPages });
    }

    [Fact]
    [AllureDescription("Retrieves specific page from paginated results")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-PAGINATE-002")]
    public async Task Handle_WithPageNumber_ShouldReturnCorrectPageData()
    {
        // Arrange
        var query = new GetAllReportsQuery { Page = 2, PageSize = 10 };

        var citizenId = Guid.NewGuid();
        var page2Reports = Enumerable.Range(11, 10)
            .Select(i => WasteReport.Create(citizenId, 1, 10.5m, 106.5m, $"Report {i}"))
            .ToList();

        _mockReportRepository
            .Setup(x => x.GetAllAsync(2, 10, null, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<WasteReport>, int)>((page2Reports, 150)));

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().HaveCount(10);
        total.Should().Be(150);
        totalPages.Should().Be(15);

        AllureAttachmentHelper.AttachJson("page-2-results", new { Page = 2, Results = results, Total = total, TotalPages = totalPages });
    }

    [Fact]
    [AllureDescription("Handles default pagination values")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-PAGINATE-003")]
    public async Task Handle_WithDefaultPaginationValues_ShouldUsePageOneAndTenItems()
    {
        // Arrange
        var query = new GetAllReportsQuery(); // Uses defaults: Page=1, PageSize=10

        var citizenId = Guid.NewGuid();
        var defaultPageReports = Enumerable.Range(1, 10)
            .Select(_ => WasteReport.Create(citizenId, 1, 10.5m, 106.5m, "Report"))
            .ToList();

        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<WasteReport>, int)>((defaultPageReports, 10)));

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().HaveCount(10);
        total.Should().Be(10);
        totalPages.Should().Be(1);
        query.Page.Should().Be(1);
        query.PageSize.Should().Be(10);

        AllureAttachmentHelper.AttachJson("default-pagination-values", query);
    }

    [Fact]
    [AllureDescription("Handles empty result set with pagination")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-PAGINATE-004")]
    public async Task Handle_WithNoResults_ShouldReturnZeroPagesAndEmptyList()
    {
        // Arrange
        var query = new GetAllReportsQuery { Page = 1, PageSize = 10 };

        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<WasteReport>, int)>((new List<WasteReport>(), 0)));

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().BeEmpty();
        total.Should().Be(0);
        totalPages.Should().Be(0);

        AllureAttachmentHelper.AttachJson("empty-pagination", new { Total = 0, TotalPages = 0 });
    }
}

#endregion

#region Filters Tests

[AllureEpic("Search & Discovery")]
[AllureFeature("Filtering")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Filter data by multiple criteria")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "FilteringTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Search")]
[Allure.Net.Commons.Attributes.AllureLabel("KIEM", "KIEM-23")]
[AllureOwner("11A6_03_Đăng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("filter")]
public class ReportsFilteringTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly GetAllReportsQueryHandler _handler;

    public ReportsFilteringTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _handler = new GetAllReportsQueryHandler(_mockReportRepository.Object);
    }

    [Fact]
    [AllureDescription("Filters reports by status (Pending reports only)")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-FILTER-001")]
    public async Task Handle_WithStatusFilter_ShouldReturnReportsWithSpecificStatus()
    {
        // Arrange
        var query = new GetAllReportsQuery { Page = 1, PageSize = 10, Status = "Pending" };

        var citizenId = Guid.NewGuid();
        var pendingReports = Enumerable.Range(1, 5)
            .Select(_ => WasteReport.Create(citizenId, 1, 10.5m, 106.5m, "Pending report"))
            .ToList();

        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 10, ReportStatus.Pending, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<WasteReport>, int)>((pendingReports, 5)));

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().HaveCount(5);
        total.Should().Be(5);
        totalPages.Should().Be(1);

        AllureAttachmentHelper.AttachJson("status-filter", new { Status = "Pending", Results = results, Total = total });
    }

    [Fact]
    [AllureDescription("Handles invalid status filter gracefully")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-FILTER-002")]
    public async Task Handle_WithInvalidStatusFilter_ShouldIgnoreAndReturnAllReports()
    {
        // Arrange
        var query = new GetAllReportsQuery
        {
            Page = 1,
            PageSize = 10,
            Status = "InvalidStatus" // Invalid status enum value
        };

        var citizenId = Guid.NewGuid();
        var allReports = new List<WasteReport>
        {
            WasteReport.Create(citizenId, 1, 10.5m, 106.5m, "Any report")
        };

        // When invalid status, should treat as null filter
        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<WasteReport>, int)>((allReports, 1)));

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().HaveCount(1);
        total.Should().Be(1);

        AllureAttachmentHelper.AttachJson("invalid-filter-results", new { Results = results });
    }

    [Fact]
    [AllureDescription("Filters reduce result set significantly from large dataset")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-FILTER-003")]
    public async Task Handle_WithNarrowingFilter_ShouldReduceResultSetSignificantly()
    {
        // Arrange
        var query = new GetAllReportsQuery
        {
            Page = 1,
            PageSize = 10,
            Status = "Collected" // Only collected reports
        };

        var citizenId = Guid.NewGuid();
        // Only 3 collected reports out of potentially thousands
        var collectedReports = Enumerable.Range(1, 3)
            .Select(_ => WasteReport.Create(citizenId, 1, 10.5m, 106.5m, "Completed report"))
            .ToList();

        _mockReportRepository
            .Setup(x => x.GetAllAsync(1, 10, ReportStatus.Collected, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(IEnumerable<WasteReport>, int)>((collectedReports, 3)));

        // Act
        var (results, total, totalPages) = await _handler.Handle(query, CancellationToken.None);

        // Assert
        results.Should().HaveCount(3);
        total.Should().Be(3);
        totalPages.Should().Be(1); // All results fit on one page

        AllureAttachmentHelper.AttachJson("narrowed-filter-results", new { Query = query, TotalMatches = total });
    }
}

#endregion
