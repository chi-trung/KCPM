using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Domain;

[AllureEpic("Domain Model")]
[AllureFeature("Waste Report Entity")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Status transitions for waste reports")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Domain")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "WasteReportTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Domain")]
[AllureOwner("Minh Phụng")]
[AllureSeverity(SeverityLevel.minor)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("domain")]
public class WasteReportTests
{
    [Fact]
    [AllureDescription("Creates a pending waste report and retains the submitted report data.")]
    public void Create_ShouldInitializePendingReportWithProvidedData()
    {
        var citizenId = Guid.NewGuid();

        AllureAttachmentHelper.AttachJson("waste-report-create-input", new
        {
            citizenId,
            wasteCategoryId = 7,
            latitude = 10.1234m,
            longitude = 106.5678m,
            description = "Báo cáo rác thải",
            address = "Q1, TP.HCM",
            aiSuggestion = "Recyclable"
        });

        var report = WasteReport.Create(
            citizenId,
            wasteCategoryId: 7,
            latitude: 10.1234m,
            longitude: 106.5678m,
            description: "Báo cáo rác thải",
            address: "Q1, TP.HCM",
            aiSuggestion: "Recyclable");

        report.CitizenId.Should().Be(citizenId);
        report.WasteCategoryId.Should().Be(7);
        report.Latitude.Should().Be(10.1234m);
        report.Longitude.Should().Be(106.5678m);
        report.Description.Should().Be("Báo cáo rác thải");
        report.Address.Should().Be("Q1, TP.HCM");
        report.AiSuggestion.Should().Be("Recyclable");
        report.Status.Should().Be(ReportStatus.Pending);

        AllureAttachmentHelper.AttachJson("waste-report-create-result", new
        {
            report.Id,
            report.CitizenId,
            report.WasteCategoryId,
            report.Status,
            report.Latitude,
            report.Longitude,
            report.Description,
            report.Address,
            report.AiSuggestion
        });
    }

    [Fact]
    [AllureDescription("Transitions a pending report to Accepted.")]
    public void Accept_WhenPending_ShouldMoveToAccepted()
    {
        var report = CreateReport();

        AllureAttachmentHelper.AttachText("waste-report-accept-start", $"reportId={report.Id}\nstatus={report.Status}");

        report.Accept();

        report.Status.Should().Be(ReportStatus.Accepted);

        AllureAttachmentHelper.AttachText("waste-report-accept-result", $"reportId={report.Id}\nstatus={report.Status}");
    }

    [Fact]
    [AllureDescription("Transitions a pending report to Rejected.")]
    public void Reject_WhenPending_ShouldMoveToRejected()
    {
        var report = CreateReport();

        AllureAttachmentHelper.AttachText("waste-report-reject-start", $"reportId={report.Id}\nstatus={report.Status}");

        report.Reject();

        report.Status.Should().Be(ReportStatus.Rejected);

        AllureAttachmentHelper.AttachText("waste-report-reject-result", $"reportId={report.Id}\nstatus={report.Status}");
    }

    [Fact]
    [AllureDescription("Transitions an accepted report to Assigned.")]
    public void Assign_WhenAccepted_ShouldMoveToAssigned()
    {
        var report = CreateReport();

        AllureAttachmentHelper.AttachText("waste-report-assign-start", $"reportId={report.Id}\nstatus={report.Status}");

        report.Accept();
        report.Assign();

        report.Status.Should().Be(ReportStatus.Assigned);

        AllureAttachmentHelper.AttachText("waste-report-assign-result", $"reportId={report.Id}\nstatus={report.Status}");
    }

    [Fact]
    [AllureDescription("Transitions an assigned report to Collected after collection is completed.")]
    public void Collect_WhenAssigned_ShouldMoveToCollected()
    {
        var report = CreateReport();

        AllureAttachmentHelper.AttachText("waste-report-collect-start", $"reportId={report.Id}\nstatus={report.Status}");

        report.Accept();
        report.Assign();
        report.Collect();

        report.Status.Should().Be(ReportStatus.Collected);

        AllureAttachmentHelper.AttachText("waste-report-collect-result", $"reportId={report.Id}\nstatus={report.Status}");
    }

    [Fact]
    [AllureDescription("Rejects invalid state recovery after a report has already been rejected.")]
    public void Accept_AfterReject_ShouldThrowInvalidOperationException()
    {
        var report = CreateReport();

        report.Reject();

        AllureAttachmentHelper.AttachText("waste-report-invalid-transition-start", $"reportId={report.Id}\nstatusBeforeRetry={report.Status}");

        var act = () => report.Accept();

        var exception = act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot transition report from Rejected to Accepted");

        AllureAttachmentHelper.AttachText("waste-report-invalid-transition-error", exception.Which.Message);
    }

    private static WasteReport CreateReport()
    {
        return WasteReport.Create(
            citizenId: Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude: 10m,
            longitude: 106m,
            description: "Test report",
            address: "Test address",
            aiSuggestion: "Mixed");
    }
}