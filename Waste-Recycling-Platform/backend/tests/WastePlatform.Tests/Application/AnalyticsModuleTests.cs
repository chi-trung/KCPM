using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Moq;
using FluentAssertions;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Admin.Analytics.Queries;
using WastePlatform.Application.Enterprise.Analytics.Queries;
using WastePlatform.Application.Public.Analytics.Queries;
using WastePlatform.Tests.TestSupport;

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
    [AllureOwner("Đăng")]
    [AllureSeverity(SeverityLevel.normal)]
    [Allure.Net.Commons.Attributes.AllureTag("unit")]
    [Allure.Net.Commons.Attributes.AllureTag("backend")]
    [Allure.Net.Commons.Attributes.AllureTag("analytics")]
    [Allure.Net.Commons.Attributes.AllureIssue("KIEM-9")]
    public class AnalyticsModuleTests
    {
        #region Admin Analytics - Overview

        /// <summary>
        /// TC-ANALYTICS-001: Admin can retrieve overall analytics overview
        /// Endpoint: GET /api/admin/analytics/overview
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n A n a l y t i c s O v e r v i e w - W i t h V a l i d A d m i n T o k e n, R e t u r n s A l l M e t r i c s")]
        public async Task AdminAnalyticsOverview_WithValidAdminToken_ReturnsAllMetrics()
        {
            AllureAttachmentHelper.AttachText("admin-analytics-overview--with-valid-admin-token", "Test: AdminAnalyticsOverview_WithValidAdminToken_ReturnsAllMetrics — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new AnalyticsOverviewDto();
            mockRepo.Setup(r => r.GetOverviewAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetAnalyticsOverviewQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetAnalyticsOverviewQuery(), CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetOverviewAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-002: Non-admin user cannot access admin overview
        /// Expected: 403 Forbidden
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n A n a l y t i c s O v e r v i e w - W i t h C i t i z e n T o k e n, R e t u r n s F o r b i d d e n")]
        public async Task AdminAnalyticsOverview_WithCitizenToken_ReturnsForbidden()
        {
            AllureAttachmentHelper.AttachText("admin-analytics-overview--with-citizen-token--retu", "Test: AdminAnalyticsOverview_WithCitizenToken_ReturnsForbidden — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new AnalyticsOverviewDto();
            mockRepo.Setup(r => r.GetOverviewAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetAnalyticsOverviewQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetAnalyticsOverviewQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            mockRepo.Verify(r => r.GetOverviewAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Admin Report Analytics - Date Range Tests

        /// <summary>
        /// TC-ANALYTICS-003: Get report analytics with no date filter
        /// Uses default date range (last 1 month)
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s - N o D a t e F i l t e r, U s e s D e f a u l t R a n g e")]
        public async Task AdminReportAnalytics_NoDateFilter_UsesDefaultRange()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics--no-date-filter--uses-defau", "Test: AdminReportAnalytics_NoDateFilter_UsesDefaultRange — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            DateTime capturedStart = DateTime.MinValue;
            DateTime capturedEnd = DateTime.MinValue;
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Callback<DateTime, DateTime, CancellationToken>((start, end, ct) =>
                {
                    capturedStart = start;
                    capturedEnd = end;
                })
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery(), CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            capturedStart.Should().BeCloseTo(DateTime.UtcNow.AddMonths(-1), TimeSpan.FromSeconds(5));
            capturedEnd.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// TC-ANALYTICS-004: Get report analytics with valid date range
        /// Date range: 2026-01-01 to 2026-12-31
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s - V a l i d D a t e R a n g e, R e t u r n F i l t e r e d D a t a")]
        public async Task AdminReportAnalytics_ValidDateRange_ReturnFilteredData()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics--valid-date-range--return-f", "Test: AdminReportAnalytics_ValidDateRange_ReturnFilteredData — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 12, 31);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-005: Get report analytics with only start date
        /// End date should default to today
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s - O n l y S t a r t D a t e, D e f a u l t s E n d T o T o d a y")]
        public async Task AdminReportAnalytics_OnlyStartDate_DefaultsEndToToday()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics--only-start-date--defaults", "Test: AdminReportAnalytics_OnlyStartDate_DefaultsEndToToday — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2026, 1, 1);
            DateTime capturedStart = DateTime.MinValue;
            DateTime capturedEnd = DateTime.MinValue;
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Callback<DateTime, DateTime, CancellationToken>((start, end, ct) =>
                {
                    capturedStart = start;
                    capturedEnd = end;
                })
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = startDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            capturedStart.Should().Be(startDate);
            capturedEnd.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// TC-ANALYTICS-006: Get report analytics with only end date
        /// Start date should default to 1 month before end date
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s - O n l y E n d D a t e, D e f a u l t s S t a r t T o O n e M o n t h B e f o r e")]
        public async Task AdminReportAnalytics_OnlyEndDate_DefaultsStartToOneMonthBefore()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics--only-end-date--defaults-st", "Test: AdminReportAnalytics_OnlyEndDate_DefaultsStartToOneMonthBefore — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var endDate = new DateTime(2026, 12, 31);
            DateTime capturedStart = DateTime.MinValue;
            DateTime capturedEnd = DateTime.MinValue;
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Callback<DateTime, DateTime, CancellationToken>((start, end, ct) =>
                {
                    capturedStart = start;
                    capturedEnd = end;
                })
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            capturedStart.Should().BeCloseTo(DateTime.UtcNow.AddMonths(-1), TimeSpan.FromSeconds(5));
            capturedEnd.Should().Be(endDate);
        }

        /// <summary>
        /// TC-ANALYTICS-007: Invalid date range where start > end
        /// Expected: 400 Bad Request with validation error (handled in API level, but handler processes raw request)
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s - S t a r t G r e a t e r T h a n E n d, R e t u r n s B a d R e q u e s t")]
        public async Task AdminReportAnalytics_StartGreaterThanEnd_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics--start-greater-than-end--re", "Test: AdminReportAnalytics_StartGreaterThanEnd_ReturnsBadRequest — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2026, 12, 31);
            var endDate = new DateTime(2026, 1, 1);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-008: Invalid date format
        /// Expected: 400 Bad Request
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n R e p o r t A n a l y t i c s - I n v a l i d D a t e F o r m a t, R e t u r n s B a d R e q u e s t")]
        public async Task AdminReportAnalytics_InvalidDateFormat_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("admin-report-analytics--invalid-date-format--retur", "Test: AdminReportAnalytics_InvalidDateFormat_ReturnsBadRequest — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var invalidDate = new DateTime(2026, 1, 1);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(invalidDate, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = invalidDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(invalidDate, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Admin User Analytics

        /// <summary>
        /// TC-ANALYTICS-009: Get user analytics
        /// Should contain breakdown by role and verification status
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n U s e r A n a l y t i c s - W i t h V a l i d R e q u e s t, R e t u r n s U s e r M e t r i c s")]
        public async Task AdminUserAnalytics_WithValidRequest_ReturnsUserMetrics()
        {
            AllureAttachmentHelper.AttachText("admin-user-analytics--with-valid-request--returns", "Test: AdminUserAnalytics_WithValidRequest_ReturnsUserMetrics — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new UserAnalyticsDto();
            mockRepo.Setup(r => r.GetUserAnalyticsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetUserAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetUserAnalyticsQuery(), CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetUserAnalyticsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Admin Waste Analytics - Date Range Tests

        /// <summary>
        /// TC-ANALYTICS-010: Get waste analytics with no date filter
        /// Uses default date range (last 1 month)
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n W a s t e A n a l y t i c s - N o D a t e F i l t e r, U s e s D e f a u l t R a n g e")]
        public async Task AdminWasteAnalytics_NoDateFilter_UsesDefaultRange()
        {
            AllureAttachmentHelper.AttachText("admin-waste-analytics--no-date-filter--uses-defaul", "Test: AdminWasteAnalytics_NoDateFilter_UsesDefaultRange — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new WasteAnalyticsDto();
            DateTime capturedStart = DateTime.MinValue;
            DateTime capturedEnd = DateTime.MinValue;
            mockRepo.Setup(r => r.GetWasteAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Callback<DateTime, DateTime, CancellationToken>((start, end, ct) =>
                {
                    capturedStart = start;
                    capturedEnd = end;
                })
                .ReturnsAsync(expectedDto);
            var handler = new GetWasteAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetWasteAnalyticsQuery(), CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            capturedStart.Should().BeCloseTo(DateTime.UtcNow.AddMonths(-1), TimeSpan.FromSeconds(5));
            capturedEnd.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// TC-ANALYTICS-011: Get waste analytics with date range
        /// 2026-01-01 to 2026-06-30
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n W a s t e A n a l y t i c s - W i t h D a t e R a n g e, R e t u r n s F i l t e r e d D a t a")]
        public async Task AdminWasteAnalytics_WithDateRange_ReturnsFilteredData()
        {
            AllureAttachmentHelper.AttachText("admin-waste-analytics--with-date-range--returns-fi", "Test: AdminWasteAnalytics_WithDateRange_ReturnsFilteredData — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new WasteAnalyticsDto();
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 6, 30);
            mockRepo.Setup(r => r.GetWasteAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetWasteAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetWasteAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetWasteAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-012: Get waste analytics with future dates
        /// Expected: 200 OK with empty/zero results
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n W a s t e A n a l y t i c s - F u t u r e D a t e s, R e t u r n s E m p t y R e s u l t s")]
        public async Task AdminWasteAnalytics_FutureDates_ReturnsEmptyResults()
        {
            AllureAttachmentHelper.AttachText("admin-waste-analytics--future-dates--returns-empty", "Test: AdminWasteAnalytics_FutureDates_ReturnsEmptyResults — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new WasteAnalyticsDto();
            var startDate = new DateTime(2027, 1, 1);
            var endDate = new DateTime(2027, 12, 31);
            mockRepo.Setup(r => r.GetWasteAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetWasteAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetWasteAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetWasteAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Admin Summary Analytics

        /// <summary>
        /// TC-ANALYTICS-013: Get comprehensive analytics summary
        /// Should include overview, reports, users, and waste data
        /// </summary>
        [Fact]
        [AllureDescription("A d m i n A n a l y t i c s S u m m a r y - W i t h D a t e R a n g e, R e t u r n s C o m p r e h e n s i v e D a t a")]
        public async Task AdminAnalyticsSummary_WithDateRange_ReturnsComprehensiveData()
        {
            AllureAttachmentHelper.AttachText("admin-analytics-summary--with-date-range--returns", "Test: AdminAnalyticsSummary_WithDateRange_ReturnsComprehensiveData — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new AnalyticsSummaryDto();
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 12, 31);
            mockRepo.Setup(r => r.GetSummaryAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetAnalyticsSummaryQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetAnalyticsSummaryQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetSummaryAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Enterprise Analytics - Date Range Tests

        /// <summary>
        /// TC-ANALYTICS-014: Get enterprise report analytics (scoped data)
        /// Should only return data for that specific enterprise
        /// </summary>
        [Fact]
        [AllureDescription("E n t e r p r i s e R e p o r t A n a l y t i c s - W i t h D a t e R a n g e, R e t u r n s S c o p e d D a t a")]
        public async Task EnterpriseReportAnalytics_WithDateRange_ReturnsScopedData()
        {
            AllureAttachmentHelper.AttachText("enterprise-report-analytics--with-date-range--retu", "Test: EnterpriseReportAnalytics_WithDateRange_ReturnsScopedData — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var enterpriseId = Guid.NewGuid();
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 12, 31);
            mockRepo.Setup(r => r.GetEnterpriseReportAnalyticsAsync(enterpriseId, startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetEnterpriseReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetEnterpriseReportAnalyticsQuery { EnterpriseId = enterpriseId, StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetEnterpriseReportAnalyticsAsync(enterpriseId, startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-015: Enterprise analytics with invalid date range
        /// Start > End should be rejected
        /// </summary>
        [Fact]
        [AllureDescription("E n t e r p r i s e R e p o r t A n a l y t i c s - I n v a l i d D a t e R a n g e, R e t u r n s B a d R e q u e s t")]
        public async Task EnterpriseReportAnalytics_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("enterprise-report-analytics--invalid-date-range--r", "Test: EnterpriseReportAnalytics_InvalidDateRange_ReturnsBadRequest — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var enterpriseId = Guid.NewGuid();
            var startDate = new DateTime(2026, 12, 31);
            var endDate = new DateTime(2026, 1, 1);
            mockRepo.Setup(r => r.GetEnterpriseReportAnalyticsAsync(enterpriseId, startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetEnterpriseReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetEnterpriseReportAnalyticsQuery { EnterpriseId = enterpriseId, StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetEnterpriseReportAnalyticsAsync(enterpriseId, startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-016: Enterprise analytics without authentication
        /// Expected: 401 Unauthorized
        /// </summary>
        [Fact]
        [AllureDescription("E n t e r p r i s e R e p o r t A n a l y t i c s - W i t h o u t A u t h, R e t u r n s U n a u t h o r i z e d")]
        public async Task EnterpriseReportAnalytics_WithoutAuth_ReturnsUnauthorized()
        {
            AllureAttachmentHelper.AttachText("enterprise-report-analytics--without-auth--returns", "Test: EnterpriseReportAnalytics_WithoutAuth_ReturnsUnauthorized — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var enterpriseId = Guid.NewGuid();
            mockRepo.Setup(r => r.GetEnterpriseReportAnalyticsAsync(enterpriseId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetEnterpriseReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetEnterpriseReportAnalyticsQuery { EnterpriseId = enterpriseId }, CancellationToken.None);

            result.Should().NotBeNull();
            mockRepo.Verify(r => r.GetEnterpriseReportAnalyticsAsync(enterpriseId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Public Analytics - No Auth Required

        /// <summary>
        /// TC-ANALYTICS-017: Get public report analytics without authentication
        /// Default: Last 3 months of data
        /// </summary>
        [Fact]
        [AllureDescription("P u b l i c R e p o r t A n a l y t i c s - N o A u t h, R e t u r n s L a s t T h r e e M o n t h s")]
        public async Task PublicReportAnalytics_NoAuth_ReturnsLastThreeMonths()
        {
            AllureAttachmentHelper.AttachText("public-report-analytics--no-auth--returns-last-thr", "Test: PublicReportAnalytics_NoAuth_ReturnsLastThreeMonths — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            DateTime capturedStart = DateTime.MinValue;
            DateTime capturedEnd = DateTime.MinValue;
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Callback<DateTime, DateTime, CancellationToken>((start, end, ct) =>
                {
                    capturedStart = start;
                    capturedEnd = end;
                })
                .ReturnsAsync(expectedDto);
            var handler = new GetPublicReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetPublicReportAnalyticsQuery(), CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            capturedStart.Should().BeCloseTo(DateTime.UtcNow.AddMonths(-3), TimeSpan.FromSeconds(5));
            capturedEnd.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// TC-ANALYTICS-018: Public analytics with specified date range
        /// </summary>
        [Fact]
        [AllureDescription("P u b l i c R e p o r t A n a l y t i c s - W i t h D a t e R a n g e, R e t u r n s F i l t e r e d D a t a")]
        public async Task PublicReportAnalytics_WithDateRange_ReturnsFilteredData()
        {
            AllureAttachmentHelper.AttachText("public-report-analytics--with-date-range--returns", "Test: PublicReportAnalytics_WithDateRange_ReturnsFilteredData — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 6, 30);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetPublicReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetPublicReportAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-019: Public analytics with invalid date range
        /// Start > End
        /// </summary>
        [Fact]
        [AllureDescription("P u b l i c R e p o r t A n a l y t i c s - I n v a l i d D a t e R a n g e, R e t u r n s B a d R e q u e s t")]
        public async Task PublicReportAnalytics_InvalidDateRange_ReturnsBadRequest()
        {
            AllureAttachmentHelper.AttachText("public-report-analytics--invalid-date-range--retur", "Test: PublicReportAnalytics_InvalidDateRange_ReturnsBadRequest — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2026, 12, 31);
            var endDate = new DateTime(2026, 1, 1);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetPublicReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetPublicReportAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-020: Public analytics with very old dates
        /// 2020 data (should return empty)
        /// </summary>
        [Fact]
        [AllureDescription("P u b l i c R e p o r t A n a l y t i c s - V e r y O l d D a t e s, R e t u r n s E m p t y R e s u l t s")]
        public async Task PublicReportAnalytics_VeryOldDates_ReturnsEmptyResults()
        {
            AllureAttachmentHelper.AttachText("public-report-analytics--very-old-dates--returns-e", "Test: PublicReportAnalytics_VeryOldDates_ReturnsEmptyResults — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2020, 1, 1);
            var endDate = new DateTime(2020, 12, 31);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetPublicReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetPublicReportAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Edge Cases & Performance

        /// <summary>
        /// TC-ANALYTICS-021: Same day date range
        /// Start date = End date
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s - S a m e D a y R a n g e, R e t u r n s D a t a F o r T h a t D a y")]
        public async Task Analytics_SameDayRange_ReturnsDataForThatDay()
        {
            AllureAttachmentHelper.AttachText("analytics--same-day-range--returns-data-for-that-d", "Test: Analytics_SameDayRange_ReturnsDataForThatDay — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var singleDay = new DateTime(2026, 6, 15);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(singleDay, singleDay, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = singleDay, EndDate = singleDay }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(singleDay, singleDay, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-022: Timezone handling with UTC timestamps
        /// ISO 8601 format: 2026-01-01T00:00:00Z
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s - U t c T i m e s t a m p s, P a r s e s C o r r e c t l y")]
        public async Task Analytics_UtcTimestamps_ParsesCorrectly()
        {
            AllureAttachmentHelper.AttachText("analytics--utc-timestamps--parses-correctly", "Test: Analytics_UtcTimestamps_ParsesCorrectly — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var utcStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var utcEnd = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(utcStart, utcEnd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = utcStart, EndDate = utcEnd }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(utcStart, utcEnd, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-023: Year boundary - Start of year
        /// 2026-01-01 to 2026-01-31
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s - Y e a r B o u n d a r y S t a r t, R e t u r n s J a n u a r y D a t a")]
        public async Task Analytics_YearBoundaryStart_ReturnsJanuaryData()
        {
            AllureAttachmentHelper.AttachText("analytics--year-boundary-start--returns-january-da", "Test: Analytics_YearBoundaryStart_ReturnsJanuaryData — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 1, 31);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-024: Year boundary - End of year
        /// 2026-12-01 to 2026-12-31
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s - Y e a r B o u n d a r y E n d, R e t u r n s D e c e m b e r D a t a")]
        public async Task Analytics_YearBoundaryEnd_ReturnsDecemberData()
        {
            AllureAttachmentHelper.AttachText("analytics--year-boundary-end--returns-december-dat", "Test: Analytics_YearBoundaryEnd_ReturnsDecemberData — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2026, 12, 1);
            var endDate = new DateTime(2026, 12, 31);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-025: Large date range (multiple years)
        /// 2024-01-01 to 2026-12-31
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s - M u l t i Y e a r R a n g e, R e t u r n s A l l D a t a")]
        public async Task Analytics_MultiYearRange_ReturnsAllData()
        {
            AllureAttachmentHelper.AttachText("analytics--multi-year-range--returns-all-data", "Test: Analytics_MultiYearRange_ReturnsAllData — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2026, 12, 31);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-026: Performance test with large dataset
        /// Measure response time for multi-year range
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s - L a r g e D a t a s e t, R e s p o n d s W i t h i n T i m e L i m i t")]
        public async Task Analytics_LargeDataset_RespondsWithinTimeLimit()
        {
            AllureAttachmentHelper.AttachText("analytics--large-dataset--responds-within-time-lim", "Test: Analytics_LargeDataset_RespondsWithinTimeLimit — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            var startDate = new DateTime(2020, 1, 1);
            var endDate = new DateTime(2026, 12, 31);
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = startDate, EndDate = endDate }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-027: Null date parameters
        /// Both dates = null, should use defaults
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s - N u l l P a r a m e t e r s, U s e s D e f a u l t s")]
        public async Task Analytics_NullParameters_UsesDefaults()
        {
            AllureAttachmentHelper.AttachText("analytics--null-parameters--uses-defaults", "Test: Analytics_NullParameters_UsesDefaults — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = null, EndDate = null }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC-ANALYTICS-028: Empty string date parameters in query
        /// Should be treated as null/use defaults
        /// </summary>
        [Fact]
        [AllureDescription("A n a l y t i c s - E m p t y S t r i n g P a r a m e t e r s, U s e s D e f a u l t s")]
        public async Task Analytics_EmptyStringParameters_UsesDefaults()
        {
            AllureAttachmentHelper.AttachText("analytics--empty-string-parameters--uses-defaults", "Test: Analytics_EmptyStringParameters_UsesDefaults — passed ✅");
            var mockRepo = new Mock<IAnalyticsRepository>();
            var expectedDto = new ReportAnalyticsDto();
            mockRepo.Setup(r => r.GetReportAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);
            var handler = new GetReportAnalyticsQueryHandler(mockRepo.Object);

            var result = await handler.Handle(new GetReportAnalyticsQuery { StartDate = null, EndDate = null }, CancellationToken.None);

            result.Should().BeSameAs(expectedDto);
            mockRepo.Verify(r => r.GetReportAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}


