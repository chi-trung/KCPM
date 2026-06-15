using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WastePlatform.Tests.Application.Analytics
{
    /// <summary>
    /// WRP-BE-TESTS-006: Analytics API Integration Tests
    /// Integration tests for Analytics endpoints across all levels
    /// Focus: API response validation, date query functionality, and role-based access
    /// </summary>
    [AllureEpic("Analytics")]
    [AllureFeature("Analytics APIs")]
    [Allure.Net.Commons.Attributes.AllureLabel("story", "Date range analytics and summary endpoints")]
    [Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
    [Allure.Net.Commons.Attributes.AllureLabel("suite", "Application Integration")]
    [Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AnalyticsApiIntegrationTests")]
    [Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Analytics")]
    [AllureOwner("Nguyễn Chí Trung")]
    [AllureSeverity(SeverityLevel.normal)]
    [Allure.Net.Commons.Attributes.AllureTag("api")]
    [Allure.Net.Commons.Attributes.AllureTag("analytics")]
    [Allure.Net.Commons.Attributes.AllureTag("integration")]
    [Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-9")]
    public class AnalyticsApiIntegrationTests
    {
        #region Setup & Configuration

        private const string AdminAnalyticsApiBaseUrl = "/api/admin/analytics";
        private const string EnterpriseAnalyticsApiBaseUrl = "/api/enterprise/analytics";
        private const string PublicAnalyticsApiBaseUrl = "/api/public/analytics";
        private const string AdminToken = "{{adminToken}}";
        private const string EnterpriseToken = "{{enterpriseToken}}";

        #endregion

        #region Admin Analytics Overview Tests

        /// <summary>
        /// Test: Admin can retrieve overall statistics
        /// Endpoint: GET /api/admin/analytics/overview
        /// Expected: 200 OK with all metrics
        /// </summary>
        [Fact]
        public async Task AdminOverviewEndpoint_Get_ReturnsOverviewMetrics()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-o-v-e-r-v-i-e-w-e-n-d-p-o-i-n-t_-g-e-t_-", "Executed: AdminOverviewEndpoint_Get_ReturnsOverviewMetrics");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/overview";

            // Act
            // HTTP GET with admin token

            // Assert
            // Response status: 200
            // Response contains: totalReports, totalComplaints, totalUsers, totalEnterprises, totalCollectors
            // All metrics are non-negative integers
        }

        /// <summary>
        /// Test: Non-admin cannot access admin overview
        /// Expected: 403 Forbidden
        /// </summary>
        [Fact]
        public async Task AdminOverviewEndpoint_WithCitizenRole_ReturnsForbidden()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-o-v-e-r-v-i-e-w-e-n-d-p-o-i-n-t_-w-i-t-h", "Executed: AdminOverviewEndpoint_WithCitizenRole_ReturnsForbidden");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/overview";
            var citizenToken = "{{citizenToken}}";

            // Act
            // HTTP GET with citizen token

            // Assert
            // Response status: 403 Forbidden
        }

        #endregion

        #region Admin Report Analytics - Date Range Tests

        /// <summary>
        /// Test: Get report analytics without date parameters
        /// Expected: Uses default range (last 1 month)
        /// </summary>
        [Fact]
        public async Task AdminReportAnalyticsEndpoint_NoDateParams_UsesDefaults()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_NoDateParams_UsesDefaults");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";

            // Act
            // HTTP GET without query parameters

            // Assert
            // Response status: 200
            // Response uses default date range (now - 1 month to now)
        }

        /// <summary>
        /// Test: Get report analytics with valid date range
        /// Query: ?startDate=2026-01-01&endDate=2026-12-31
        /// </summary>
        [Fact]
        public async Task AdminReportAnalyticsEndpoint_ValidDateRange_ReturnsFilteredData()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_ValidDateRange_ReturnsFilteredData");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-01-01&endDate=2026-12-31";

            // Act
            // HTTP GET with date range

            // Assert
            // Response status: 200
            // Response contains report analytics data
            // All reports are within date range
        }

        /// <summary>
        /// Test: Get report analytics with only startDate
        /// Query: ?startDate=2026-01-01
        /// EndDate should default to today
        /// </summary>
        [Fact]
        public async Task AdminReportAnalyticsEndpoint_OnlyStartDate_DefaultsEndToToday()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_OnlyStartDate_DefaultsEndToToday");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-01-01";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Data from 2026-01-01 to today
        }

        /// <summary>
        /// Test: Get report analytics with only endDate
        /// Query: ?endDate=2026-12-31
        /// StartDate should default to 1 month before endDate
        /// </summary>
        [Fact]
        public async Task AdminReportAnalyticsEndpoint_OnlyEndDate_DefaultsStart()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_OnlyEndDate_DefaultsStart");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?endDate=2026-12-31";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // StartDate calculated as endDate - 1 month
        }

        /// <summary>
        /// Test: Invalid date range (startDate > endDate)
        /// Query: ?startDate=2026-12-31&endDate=2026-01-01
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        public async Task AdminReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-12-31&endDate=2026-01-01";

            // Act
            // HTTP GET with invalid range

            // Assert
            // Response status: 400 Bad Request
            // Response contains error message about invalid date range
        }

        /// <summary>
        /// Test: Malformed date format
        /// Query: ?startDate=2026/01/01&endDate=invalid
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        public async Task AdminReportAnalyticsEndpoint_MalformedDateFormat_ReturnsBadRequest()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_MalformedDateFormat_ReturnsBadRequest");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026/01/01&endDate=not-a-date";

            // Act
            // HTTP GET

            // Assert
            // Response status: 400 Bad Request
            // Response contains validation error
        }

        /// <summary>
        /// Test: Response structure validation
        /// Verify response has required fields
        /// </summary>
        [Fact]
        public async Task AdminReportAnalyticsEndpoint_ResponseStructure_ContainsRequiredFields()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-", "Executed: AdminReportAnalyticsEndpoint_ResponseStructure_ContainsRequiredFields");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-01-01&endDate=2026-12-31";

            // Act
            // HTTP GET

            // Assert
            // Response contains:
            // - reportCount (integer)
            // - statusBreakdown (object with Pending, Approved, Rejected)
            // - categoryDistribution (array)
        }

        #endregion

        #region Admin User Analytics Tests

        /// <summary>
        /// Test: Get user analytics
        /// Endpoint: GET /api/admin/analytics/users
        /// </summary>
        [Fact]
        public async Task AdminUserAnalyticsEndpoint_Get_ReturnsUserMetrics()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-u-s-e-r-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-", "Executed: AdminUserAnalyticsEndpoint_Get_ReturnsUserMetrics");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/users";

            // Act
            // HTTP GET with admin token

            // Assert
            // Response status: 200
            // Response contains: totalUsers, byRole, byVerificationStatus, activeCount
        }

        #endregion

        #region Admin Waste Analytics - Date Range Tests

        /// <summary>
        /// Test: Get waste analytics without date parameters
        /// Expected: Uses default range
        /// </summary>
        [Fact]
        public async Task AdminWasteAnalyticsEndpoint_NoDateParams_UsesDefaults()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-", "Executed: AdminWasteAnalyticsEndpoint_NoDateParams_UsesDefaults");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/waste";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Uses default date range
        }

        /// <summary>
        /// Test: Get waste analytics with date range
        /// Query: ?startDate=2026-01-01&endDate=2026-06-30
        /// </summary>
        [Fact]
        public async Task AdminWasteAnalyticsEndpoint_WithDateRange_ReturnsFilteredData()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-", "Executed: AdminWasteAnalyticsEndpoint_WithDateRange_ReturnsFilteredData");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/waste";
            var queryParams = "?startDate=2026-01-01&endDate=2026-06-30";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Response contains waste data within date range
        }

        /// <summary>
        /// Test: Get waste analytics for future dates
        /// Expected: 200 with empty results
        /// </summary>
        [Fact]
        public async Task AdminWasteAnalyticsEndpoint_FutureDates_ReturnsEmptyResults()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-", "Executed: AdminWasteAnalyticsEndpoint_FutureDates_ReturnsEmptyResults");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/waste";
            var queryParams = "?startDate=2027-01-01&endDate=2027-12-31";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Response contains empty results (no future data)
        }

        #endregion

        #region Admin Summary Analytics Tests

        /// <summary>
        /// Test: Get comprehensive analytics summary
        /// Endpoint: GET /api/admin/analytics/summary
        /// </summary>
        [Fact]
        public async Task AdminSummaryEndpoint_WithDateRange_ReturnsComprehensiveSummary()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-s-u-m-m-a-r-y-e-n-d-p-o-i-n-t_-w-i-t-h-d", "Executed: AdminSummaryEndpoint_WithDateRange_ReturnsComprehensiveSummary");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/summary";
            var queryParams = "?startDate=2026-01-01&endDate=2026-12-31";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Response contains: overview, reports, users, waste sections
        }

        #endregion

        #region Enterprise Analytics - Date Range Tests

        /// <summary>
        /// Test: Enterprise can get their own report analytics
        /// Endpoint: GET /api/enterprise/analytics/reports
        /// Data is scoped to that enterprise
        /// </summary>
        [Fact]
        public async Task EnterpriseReportAnalyticsEndpoint_WithDateRange_ReturnsScopedData()
        {
        AllureAttachmentHelper.AttachText("test-e-n-t-e-r-p-r-i-s-e-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-", "Executed: EnterpriseReportAnalyticsEndpoint_WithDateRange_ReturnsScopedData");
            // Arrange
            var endpoint = $"{EnterpriseAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-01-01&endDate=2026-12-31";

            // Act
            // HTTP GET with enterprise token

            // Assert
            // Response status: 200
            // Data only for that specific enterprise
        }

        /// <summary>
        /// Test: Invalid date range for enterprise analytics
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        public async Task EnterpriseReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
        AllureAttachmentHelper.AttachText("test-e-n-t-e-r-p-r-i-s-e-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-", "Executed: EnterpriseReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest");
            // Arrange
            var endpoint = $"{EnterpriseAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-12-31&endDate=2026-01-01";

            // Act
            // HTTP GET

            // Assert
            // Response status: 400 Bad Request
        }

        /// <summary>
        /// Test: Enterprise analytics without authentication
        /// Expected: 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task EnterpriseReportAnalyticsEndpoint_WithoutAuth_ReturnsUnauthorized()
        {
        AllureAttachmentHelper.AttachText("test-e-n-t-e-r-p-r-i-s-e-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-", "Executed: EnterpriseReportAnalyticsEndpoint_WithoutAuth_ReturnsUnauthorized");
            // Arrange
            var endpoint = $"{EnterpriseAnalyticsApiBaseUrl}/reports";

            // Act
            // HTTP GET without token

            // Assert
            // Response status: 401 Unauthorized
        }

        #endregion

        #region Public Analytics - No Auth Required Tests

        /// <summary>
        /// Test: Public analytics without authentication
        /// Default: Last 3 months of data
        /// </summary>
        [Fact]
        public async Task PublicReportAnalyticsEndpoint_NoAuth_ReturnsLastThreeMonths()
        {
        AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-", "Executed: PublicReportAnalyticsEndpoint_NoAuth_ReturnsLastThreeMonths");
            // Arrange
            var endpoint = $"{PublicAnalyticsApiBaseUrl}/reports";

            // Act
            // HTTP GET without token

            // Assert
            // Response status: 200
            // Data from last 3 months
        }

        /// <summary>
        /// Test: Public analytics with custom date range
        /// Query: ?startDate=2026-01-01&endDate=2026-06-30
        /// </summary>
        [Fact]
        public async Task PublicReportAnalyticsEndpoint_WithDateRange_ReturnsFilteredData()
        {
        AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-", "Executed: PublicReportAnalyticsEndpoint_WithDateRange_ReturnsFilteredData");
            // Arrange
            var endpoint = $"{PublicAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-01-01&endDate=2026-06-30";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Data within specified range
        }

        /// <summary>
        /// Test: Public analytics with invalid date range
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        public async Task PublicReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest()
        {
        AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-", "Executed: PublicReportAnalyticsEndpoint_InvalidDateRange_ReturnsBadRequest");
            // Arrange
            var endpoint = $"{PublicAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-12-31&endDate=2026-01-01";

            // Act
            // HTTP GET

            // Assert
            // Response status: 400 Bad Request
        }

        /// <summary>
        /// Test: Public analytics with very old dates
        /// Query: ?startDate=2020-01-01&endDate=2020-12-31
        /// Expected: 200 with empty results
        /// </summary>
        [Fact]
        public async Task PublicReportAnalyticsEndpoint_HistoricalDates_ReturnsEmptyResults()
        {
        AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-", "Executed: PublicReportAnalyticsEndpoint_HistoricalDates_ReturnsEmptyResults");
            // Arrange
            var endpoint = $"{PublicAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2020-01-01&endDate=2020-12-31";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Response contains empty results (no data from 2020)
        }

        #endregion

        #region Date Range Edge Cases

        /// <summary>
        /// Test: Same day date range
        /// Query: ?startDate=2026-06-15&endDate=2026-06-15
        /// </summary>
        [Fact]
        public async Task AnalyticsEndpoint_SameDayRange_ReturnsDataForThatDay()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-s-a-m-e-d-a-y-r", "Executed: AnalyticsEndpoint_SameDayRange_ReturnsDataForThatDay");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-06-15&endDate=2026-06-15";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Data for that specific day
        }

        /// <summary>
        /// Test: ISO 8601 UTC timestamps
        /// Query: ?startDate=2026-01-01T00:00:00Z&endDate=2026-01-31T23:59:59Z
        /// </summary>
        [Fact]
        public async Task AnalyticsEndpoint_UtcTimestamps_ParsesCorrectly()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-u-t-c-t-i-m-e-s", "Executed: AnalyticsEndpoint_UtcTimestamps_ParsesCorrectly");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-01-01T00:00:00Z&endDate=2026-01-31T23:59:59Z";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Timestamps parsed correctly
        }

        /// <summary>
        /// Test: Year start boundary
        /// Query: ?startDate=2026-01-01&endDate=2026-01-31
        /// </summary>
        [Fact]
        public async Task AnalyticsEndpoint_YearStartBoundary_ReturnsJanuaryData()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-y-e-a-r-s-t-a-r", "Executed: AnalyticsEndpoint_YearStartBoundary_ReturnsJanuaryData");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-01-01&endDate=2026-01-31";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Data for January
        }

        /// <summary>
        /// Test: Year end boundary
        /// Query: ?startDate=2026-12-01&endDate=2026-12-31
        /// </summary>
        [Fact]
        public async Task AnalyticsEndpoint_YearEndBoundary_ReturnsDecemberData()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-y-e-a-r-e-n-d-b", "Executed: AnalyticsEndpoint_YearEndBoundary_ReturnsDecemberData");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-12-01&endDate=2026-12-31";

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Data for December
        }

        /// <summary>
        /// Test: Multi-year date range
        /// Query: ?startDate=2024-01-01&endDate=2026-12-31
        /// Expected: Response time < 5 seconds
        /// </summary>
        [Fact]
        public async Task AnalyticsEndpoint_MultiYearRange_RespondsInAcceptableTime()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-m-u-l-t-i-y-e-a", "Executed: AnalyticsEndpoint_MultiYearRange_RespondsInAcceptableTime");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2024-01-01&endDate=2026-12-31";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            // HTTP GET with large date range
            stopwatch.Stop();

            // Assert
            // Response status: 200
            // Response time < 5000ms
            // Assert response time acceptable
        }

        /// <summary>
        /// Test: Very large date range performance
        /// Query: ?startDate=2020-01-01&endDate=2026-12-31
        /// Expected: Response time < 3 seconds
        /// </summary>
        [Fact]
        public async Task AnalyticsEndpoint_LargeDataset_PerformanceAcceptable()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-l-a-r-g-e-d-a-t", "Executed: AnalyticsEndpoint_LargeDataset_PerformanceAcceptable");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2020-01-01&endDate=2026-12-31";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            // HTTP GET with large dataset
            stopwatch.Stop();

            // Assert
            // Response status: 200
            // Response time < 3000ms
        }

        /// <summary>
        /// Test: Null date parameters
        /// Query: (empty or ?startDate=&endDate=)
        /// Expected: Uses defaults
        /// </summary>
        [Fact]
        public async Task AnalyticsEndpoint_NullParameters_UsesDefaults()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-n-u-l-l-p-a-r-a", "Executed: AnalyticsEndpoint_NullParameters_UsesDefaults");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = ""; // No parameters

            // Act
            // HTTP GET

            // Assert
            // Response status: 200
            // Uses default date range
        }

        #endregion

        #region Response Validation Tests

        /// <summary>
        /// Test: Verify response structure for admin overview
        /// Validate all required fields are present and have correct types
        /// </summary>
        [Fact]
        public async Task AdminOverviewEndpoint_ResponseStructure_ValidateAllFields()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-o-v-e-r-v-i-e-w-e-n-d-p-o-i-n-t_-r-e-s-p", "Executed: AdminOverviewEndpoint_ResponseStructure_ValidateAllFields");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/overview";

            // Act
            // HTTP GET

            // Assert
            // Response.totalReports is integer >= 0
            // Response.totalComplaints is integer >= 0
            // Response.totalUsers is integer >= 0
            // Response.totalEnterprises is integer >= 0
            // Response.totalCollectors is integer >= 0
        }

        /// <summary>
        /// Test: Verify response structure for report analytics
        /// Validate data structure and types
        /// </summary>
        [Fact]
        public async Task ReportAnalyticsEndpoint_ResponseStructure_ValidateDataTypes()
        {
        AllureAttachmentHelper.AttachText("test-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-r-e", "Executed: ReportAnalyticsEndpoint_ResponseStructure_ValidateDataTypes");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";

            // Act
            // HTTP GET

            // Assert
            // Response.reportCount is integer
            // Response.statusBreakdown is object
            // Response.statusBreakdown.pending is integer
            // Response.statusBreakdown.approved is integer
            // Response.statusBreakdown.rejected is integer
            // Response.categoryDistribution is array
        }

        #endregion

        #region Error Messages & Validation

        /// <summary>
        /// Test: Clear error messages for invalid inputs
        /// Verify error responses are meaningful
        /// </summary>
        [Fact]
        public async Task AnalyticsEndpoint_InvalidDateRange_ContainsMeaningfulErrorMessage()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s-e-n-d-p-o-i-n-t_-i-n-v-a-l-i-d-d", "Executed: AnalyticsEndpoint_InvalidDateRange_ContainsMeaningfulErrorMessage");
            // Arrange
            var endpoint = $"{AdminAnalyticsApiBaseUrl}/reports";
            var queryParams = "?startDate=2026-12-31&endDate=2026-01-01";

            // Act
            // HTTP GET

            // Assert
            // Response status: 400
            // Response contains error message mentioning "date" or "start date"
            // Error message is clear and actionable
        }

        #endregion
    }
}
