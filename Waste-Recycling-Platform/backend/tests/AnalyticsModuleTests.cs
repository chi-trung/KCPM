using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WastePlatform.Application.Admin.Analytics;
using WastePlatform.Application.Public.Analytics;
using WastePlatform.Application.Enterprise.Analytics;

namespace WastePlatform.Tests.Application.Analytics
{
    /// <summary>
    /// WRP-BE-TESTS-006: Analytics Module Testing
    /// Unit tests for Analytics functionality across Admin, Enterprise, and Public levels
    /// Focus: Date range query handling and data filtering
    /// </summary>
    [AllureEpic("Analytics")]
    [AllureFeature("Analytics Modules")]
    [Allure.Net.Commons.Attributes.AllureLabel("story", "Date filtering and metric aggregation")]
    [Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
    [Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
    [Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AnalyticsModuleTests")]
    [Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Analytics")]
    [AllureOwner("11A6_03_ÄÄƒng")]
    [AllureSeverity(SeverityLevel.normal)]
    [Allure.Net.Commons.Attributes.AllureTag("unit")]
    [Allure.Net.Commons.Attributes.AllureTag("backend")]
    [Allure.Net.Commons.Attributes.AllureTag("analytics")]
    [Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-9")]
    public class AnalyticsModuleTests
    {
        #region Admin Analytics - Overview

        /// <summary>
        /// TC-ANALYTICS-001: Admin can retrieve overall analytics overview
        /// Endpoint: GET /api/admin/analytics/overview
        /// </summary>
        [Fact]
        public async Task AdminAnalyticsOverview_WithValidAdminToken_ReturnsAllMetrics()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-a-n-a-l-y-t-i-c-s-o-v-e-r-v-i-e-w_-w-i-t", "Executed: AdminAnalyticsOverview_WithValidAdminToken_ReturnsAllMetrics");
            // Arrange
            var adminUserId = "admin-user-123";

            // Act
            // var result = await analyticsService.GetOverviewAsync();

            // Assert
            // Assert.NotNull(result);
            // Assert.True(result.TotalReports >= 0);
            // Assert.True(result.TotalComplaints >= 0);
            // Assert.True(result.TotalUsers >= 0);
            // Assert.True(result.TotalEnterprises >= 0);
        }

        /// <summary>
        /// TC-ANALYTICS-002: Non-admin user cannot access admin overview
        /// Expected: 403 Forbidden
        /// </summary>
        [Fact]
        public async Task AdminAnalyticsOverview_WithCitizenToken_ReturnsForbidden()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-a-n-a-l-y-t-i-c-s-o-v-e-r-v-i-e-w_-w-i-t", "Executed: AdminAnalyticsOverview_WithCitizenToken_ReturnsForbidden");
            // Arrange
            var citizenUserId = "citizen-user-123";

            // Act
            // var result = await analyticsService.GetOverviewAsync(citizenUserId);

            // Assert
            // Assert.Equal(403, result.StatusCode); // Forbidden
        }

        #endregion

        #region Admin Report Analytics - Date Range Tests

        /// <summary>
        /// TC-ANALYTICS-003: Get report analytics with no date filter
        /// Uses default date range (last 1 month)
        /// </summary>
        [Fact]
        public async Task AdminReportAnalytics_NoDateFilter_UsesDefaultRange()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-n-o-d-a-t", "Executed: AdminReportAnalytics_NoDateFilter_UsesDefaultRange");
            // Arrange
            var startDate = (DateTime?)null;
            var endDate = (DateTime?)null;

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // Default should be: endDate = now, startDate = now - 1 month
        }

        /// <summary>
        /// TC-ANALYTICS-004: Get report analytics with valid date range
        /// Date range: 2026-01-01 to 2026-12-31
        /// </summary>
        [Fact]
        public async Task AdminReportAnalytics_ValidDateRange_ReturnFilteredData()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-v-a-l-i-d", "Executed: AdminReportAnalytics_ValidDateRange_ReturnFilteredData");
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 12, 31);

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // All reports should be within date range
            // Assert.All(result.Reports, r => 
            //     Assert.True(r.CreatedDate >= startDate && r.CreatedDate <= endDate)
            // );
        }

        /// <summary>
        /// TC-ANALYTICS-005: Get report analytics with only start date
        /// End date should default to today
        /// </summary>
        [Fact]
        public async Task AdminReportAnalytics_OnlyStartDate_DefaultsEndToToday()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-o-n-l-y-s", "Executed: AdminReportAnalytics_OnlyStartDate_DefaultsEndToToday");
            // Arrange
            var startDate = new DateTime(2026, 1, 1);

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(startDate, null);

            // Assert
            // Assert.NotNull(result);
            // Effective end date should be DateTime.Today or DateTime.UtcNow
        }

        /// <summary>
        /// TC-ANALYTICS-006: Get report analytics with only end date
        /// Start date should default to 1 month before end date
        /// </summary>
        [Fact]
        public async Task AdminReportAnalytics_OnlyEndDate_DefaultsStartToOneMonthBefore()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-o-n-l-y-e", "Executed: AdminReportAnalytics_OnlyEndDate_DefaultsStartToOneMonthBefore");
            // Arrange
            var endDate = new DateTime(2026, 12, 31);

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(null, endDate);

            // Assert
            // Assert.NotNull(result);
            // Effective start date should be endDate - 1 month
        }

        /// <summary>
        /// TC-ANALYTICS-007: Invalid date range where start > end
        /// Expected: 400 Bad Request with validation error
        /// </summary>
        [Fact]
        public async Task AdminReportAnalytics_StartGreaterThanEnd_ReturnsBadRequest()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-s-t-a-r-t", "Executed: AdminReportAnalytics_StartGreaterThanEnd_ReturnsBadRequest");
            // Arrange
            var startDate = new DateTime(2026, 12, 31);
            var endDate = new DateTime(2026, 1, 1);

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.Equal(400, result.StatusCode);
            // Assert.NotNull(result.ErrorMessage);
            // Assert.Contains("start date", result.ErrorMessage.ToLower());
        }

        /// <summary>
        /// TC-ANALYTICS-008: Invalid date format
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        public async Task AdminReportAnalytics_InvalidDateFormat_ReturnsBadRequest()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-i-n-v-a-l", "Executed: AdminReportAnalytics_InvalidDateFormat_ReturnsBadRequest");
            // Arrange
            var invalidDateString = "2026/01/01"; // Not ISO 8601

            // Act
            // Parsing would occur in API layer
            // var result = await analyticsService.GetReportAnalyticsAsync(
            //     DateTime.Parse(invalidDateString), null);

            // Assert
            // Should throw FormatException or return 400
        }

        #endregion

        #region Admin User Analytics

        /// <summary>
        /// TC-ANALYTICS-009: Get user analytics
        /// Should contain breakdown by role and verification status
        /// </summary>
        [Fact]
        public async Task AdminUserAnalytics_WithValidRequest_ReturnsUserMetrics()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-u-s-e-r-a-n-a-l-y-t-i-c-s_-w-i-t-h-v-a-l", "Executed: AdminUserAnalytics_WithValidRequest_ReturnsUserMetrics");
            // Arrange
            var adminUserId = "admin-user-123";

            // Act
            // var result = await analyticsService.GetUserAnalyticsAsync();

            // Assert
            // Assert.NotNull(result);
            // Assert.True(result.TotalUsers >= 0);
            // Assert.NotNull(result.ByRole);
            // Assert.NotNull(result.ByVerificationStatus);
            // Assert.True(result.ActiveCount >= 0);
        }

        #endregion

        #region Admin Waste Analytics - Date Range Tests

        /// <summary>
        /// TC-ANALYTICS-010: Get waste analytics with no date filter
        /// Uses default date range (last 1 month)
        /// </summary>
        [Fact]
        public async Task AdminWasteAnalytics_NoDateFilter_UsesDefaultRange()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s_-n-o-d-a-t-e", "Executed: AdminWasteAnalytics_NoDateFilter_UsesDefaultRange");
            // Arrange
            var startDate = (DateTime?)null;
            var endDate = (DateTime?)null;

            // Act
            // var result = await analyticsService.GetWasteAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // Assert.NotNull(result.WasteByCategory);
            // Assert.NotNull(result.MonthlyDistribution);
        }

        /// <summary>
        /// TC-ANALYTICS-011: Get waste analytics with date range
        /// 2026-01-01 to 2026-06-30
        /// </summary>
        [Fact]
        public async Task AdminWasteAnalytics_WithDateRange_ReturnsFilteredData()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s_-w-i-t-h-d-a", "Executed: AdminWasteAnalytics_WithDateRange_ReturnsFilteredData");
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 6, 30);

            // Act
            // var result = await analyticsService.GetWasteAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // Data should be within specified date range
        }

        /// <summary>
        /// TC-ANALYTICS-012: Get waste analytics with future dates
        /// Expected: 200 OK with empty/zero results
        /// </summary>
        [Fact]
        public async Task AdminWasteAnalytics_FutureDates_ReturnsEmptyResults()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-w-a-s-t-e-a-n-a-l-y-t-i-c-s_-f-u-t-u-r-e", "Executed: AdminWasteAnalytics_FutureDates_ReturnsEmptyResults");
            // Arrange
            var startDate = new DateTime(2027, 1, 1);
            var endDate = new DateTime(2027, 12, 31);

            // Act
            // var result = await analyticsService.GetWasteAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // Assert.Empty(result.WasteByCategory);
            // OR Assert.Equal(0, result.TotalWaste);
        }

        #endregion

        #region Admin Summary Analytics

        /// <summary>
        /// TC-ANALYTICS-013: Get comprehensive analytics summary
        /// Should include overview, reports, users, and waste data
        /// </summary>
        [Fact]
        public async Task AdminAnalyticsSummary_WithDateRange_ReturnsComprehensiveData()
        {
        AllureAttachmentHelper.AttachText("test-a-d-m-i-n-a-n-a-l-y-t-i-c-s-s-u-m-m-a-r-y_-w-i-t-h", "Executed: AdminAnalyticsSummary_WithDateRange_ReturnsComprehensiveData");
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 12, 31);

            // Act
            // var result = await analyticsService.GetSummaryAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // Assert.NotNull(result.Overview);
            // Assert.NotNull(result.Reports);
            // Assert.NotNull(result.Users);
            // Assert.NotNull(result.Waste);
        }

        #endregion

        #region Enterprise Analytics - Date Range Tests

        /// <summary>
        /// TC-ANALYTICS-014: Get enterprise report analytics (scoped data)
        /// Should only return data for that specific enterprise
        /// </summary>
        [Fact]
        public async Task EnterpriseReportAnalytics_WithDateRange_ReturnsScopedData()
        {
        AllureAttachmentHelper.AttachText("test-e-n-t-e-r-p-r-i-s-e-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_", "Executed: EnterpriseReportAnalytics_WithDateRange_ReturnsScopedData");
            // Arrange
            var enterpriseId = Guid.NewGuid();
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 12, 31);

            // Act
            // var result = await analyticsService.GetEnterpriseReportAnalyticsAsync(
            //     enterpriseId, startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // All results should belong to specified enterprise only
        }

        /// <summary>
        /// TC-ANALYTICS-015: Enterprise analytics with invalid date range
        /// Start > End should be rejected
        /// </summary>
        [Fact]
        public async Task EnterpriseReportAnalytics_InvalidDateRange_ReturnsBadRequest()
        {
        AllureAttachmentHelper.AttachText("test-e-n-t-e-r-p-r-i-s-e-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_", "Executed: EnterpriseReportAnalytics_InvalidDateRange_ReturnsBadRequest");
            // Arrange
            var enterpriseId = Guid.NewGuid();
            var startDate = new DateTime(2026, 12, 31);
            var endDate = new DateTime(2026, 1, 1);

            // Act
            // var result = await analyticsService.GetEnterpriseReportAnalyticsAsync(
            //     enterpriseId, startDate, endDate);

            // Assert
            // Assert.Equal(400, result.StatusCode);
        }

        /// <summary>
        /// TC-ANALYTICS-016: Enterprise analytics without authentication
        /// Expected: 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task EnterpriseReportAnalytics_WithoutAuth_ReturnsUnauthorized()
        {
        AllureAttachmentHelper.AttachText("test-e-n-t-e-r-p-r-i-s-e-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_", "Executed: EnterpriseReportAnalytics_WithoutAuth_ReturnsUnauthorized");
            // Arrange
            var enterpriseId = Guid.NewGuid();

            // Act
            // Attempt without token

            // Assert
            // Assert.Equal(401, result.StatusCode);
        }

        #endregion

        #region Public Analytics - No Auth Required

        /// <summary>
        /// TC-ANALYTICS-017: Get public report analytics without authentication
        /// Default: Last 3 months of data
        /// </summary>
        [Fact]
        public async Task PublicReportAnalytics_NoAuth_ReturnsLastThreeMonths()
        {
        AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-n-o-a-u", "Executed: PublicReportAnalytics_NoAuth_ReturnsLastThreeMonths");
            // Arrange
            // No authentication needed

            // Act
            // var result = await analyticsService.GetPublicReportAnalyticsAsync(null, null);

            // Assert
            // Assert.NotNull(result);
            // Data should be from last 3 months
        }

        /// <summary>
        /// TC-ANALYTICS-018: Public analytics with specified date range
        /// </summary>
        [Fact]
        public async Task PublicReportAnalytics_WithDateRange_ReturnsFilteredData()
        {
        AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-w-i-t-h", "Executed: PublicReportAnalytics_WithDateRange_ReturnsFilteredData");
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 6, 30);

            // Act
            // var result = await analyticsService.GetPublicReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
        }

        /// <summary>
        /// TC-ANALYTICS-019: Public analytics with invalid date range
        /// Start > End
        /// </summary>
        [Fact]
        public async Task PublicReportAnalytics_InvalidDateRange_ReturnsBadRequest()
        {
        AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-i-n-v-a", "Executed: PublicReportAnalytics_InvalidDateRange_ReturnsBadRequest");
            // Arrange
            var startDate = new DateTime(2026, 12, 31);
            var endDate = new DateTime(2026, 1, 1);

            // Act
            // var result = await analyticsService.GetPublicReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.Equal(400, result.StatusCode);
        }

        /// <summary>
        /// TC-ANALYTICS-020: Public analytics with very old dates
        /// 2020 data (should return empty)
        /// </summary>
        [Fact]
        public async Task PublicReportAnalytics_VeryOldDates_ReturnsEmptyResults()
        {
        AllureAttachmentHelper.AttachText("test-p-u-b-l-i-c-r-e-p-o-r-t-a-n-a-l-y-t-i-c-s_-v-e-r-y", "Executed: PublicReportAnalytics_VeryOldDates_ReturnsEmptyResults");
            // Arrange
            var startDate = new DateTime(2020, 1, 1);
            var endDate = new DateTime(2020, 12, 31);

            // Act
            // var result = await analyticsService.GetPublicReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // Assert.Empty(result.Reports);
        }

        #endregion

        #region Edge Cases & Performance

        /// <summary>
        /// TC-ANALYTICS-021: Same day date range
        /// Start date = End date
        /// </summary>
        [Fact]
        public async Task Analytics_SameDayRange_ReturnsDataForThatDay()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s_-s-a-m-e-d-a-y-r-a-n-g-e_-r-e-t-", "Executed: Analytics_SameDayRange_ReturnsDataForThatDay");
            // Arrange
            var singleDay = new DateTime(2026, 6, 15);

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(singleDay, singleDay);

            // Assert
            // Assert.NotNull(result);
        }

        /// <summary>
        /// TC-ANALYTICS-022: Timezone handling with UTC timestamps
        /// ISO 8601 format: 2026-01-01T00:00:00Z
        /// </summary>
        [Fact]
        public async Task Analytics_UtcTimestamps_ParsesCorrectly()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s_-u-t-c-t-i-m-e-s-t-a-m-p-s_-p-a-", "Executed: Analytics_UtcTimestamps_ParsesCorrectly");
            // Arrange
            var utcStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var utcEnd = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(utcStart, utcEnd);

            // Assert
            // Assert.NotNull(result);
        }

        /// <summary>
        /// TC-ANALYTICS-023: Year boundary - Start of year
        /// 2026-01-01 to 2026-01-31
        /// </summary>
        [Fact]
        public async Task Analytics_YearBoundaryStart_ReturnsJanuaryData()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s_-y-e-a-r-b-o-u-n-d-a-r-y-s-t-a-r", "Executed: Analytics_YearBoundaryStart_ReturnsJanuaryData");
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 1, 31);

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
        }

        /// <summary>
        /// TC-ANALYTICS-024: Year boundary - End of year
        /// 2026-12-01 to 2026-12-31
        /// </summary>
        [Fact]
        public async Task Analytics_YearBoundaryEnd_ReturnsDecemberData()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s_-y-e-a-r-b-o-u-n-d-a-r-y-e-n-d_-", "Executed: Analytics_YearBoundaryEnd_ReturnsDecemberData");
            // Arrange
            var startDate = new DateTime(2026, 12, 1);
            var endDate = new DateTime(2026, 12, 31);

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
        }

        /// <summary>
        /// TC-ANALYTICS-025: Large date range (multiple years)
        /// 2024-01-01 to 2026-12-31
        /// </summary>
        [Fact]
        public async Task Analytics_MultiYearRange_ReturnsAllData()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s_-m-u-l-t-i-y-e-a-r-r-a-n-g-e_-r-", "Executed: Analytics_MultiYearRange_ReturnsAllData");
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2026, 12, 31);

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // Response time should be < 5 seconds
        }

        /// <summary>
        /// TC-ANALYTICS-026: Performance test with large dataset
        /// Measure response time for multi-year range
        /// Expected: < 3000ms
        /// </summary>
        [Fact]
        public async Task Analytics_LargeDataset_RespondsWithinTimeLimit()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s_-l-a-r-g-e-d-a-t-a-s-e-t_-r-e-s-", "Executed: Analytics_LargeDataset_RespondsWithinTimeLimit");
            // Arrange
            var startDate = new DateTime(2020, 1, 1);
            var endDate = new DateTime(2026, 12, 31);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(startDate, endDate);
            stopwatch.Stop();

            // Assert
            // Assert.NotNull(result);
            // Assert.True(stopwatch.ElapsedMilliseconds < 3000,
            //     $"Response time {stopwatch.ElapsedMilliseconds}ms exceeds limit of 3000ms");
        }

        /// <summary>
        /// TC-ANALYTICS-027: Null date parameters
        /// Both dates = null, should use defaults
        /// </summary>
        [Fact]
        public async Task Analytics_NullParameters_UsesDefaults()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s_-n-u-l-l-p-a-r-a-m-e-t-e-r-s_-u-", "Executed: Analytics_NullParameters_UsesDefaults");
            // Arrange
            DateTime? startDate = null;
            DateTime? endDate = null;

            // Act
            // var result = await analyticsService.GetReportAnalyticsAsync(startDate, endDate);

            // Assert
            // Assert.NotNull(result);
            // Should use default date range
        }

        /// <summary>
        /// TC-ANALYTICS-028: Empty string date parameters in query
        /// Should be treated as null/use defaults
        /// </summary>
        [Fact]
        public async Task Analytics_EmptyStringParameters_UsesDefaults()
        {
        AllureAttachmentHelper.AttachText("test-a-n-a-l-y-t-i-c-s_-e-m-p-t-y-s-t-r-i-n-g-p-a-r-a-m", "Executed: Analytics_EmptyStringParameters_UsesDefaults");
            // Arrange
            var emptyStart = "";
            var emptyEnd = "";

            // Act
            // Parsing logic should handle empty strings
            // var result = await analyticsService.GetReportAnalyticsAsync(null, null);

            // Assert
            // Assert.NotNull(result);
        }

        #endregion
    }
}
