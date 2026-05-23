using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Tests.Domain;

public class WasteReportTests
{
    [Fact]
    public void Create_ShouldInitializePendingReportWithProvidedData()
    {
        var citizenId = Guid.NewGuid();

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
    }

    [Fact]
    public void Accept_WhenPending_ShouldMoveToAccepted()
    {
        var report = CreateReport();

        report.Accept();

        report.Status.Should().Be(ReportStatus.Accepted);
    }

    [Fact]
    public void Reject_WhenPending_ShouldMoveToRejected()
    {
        var report = CreateReport();

        report.Reject();

        report.Status.Should().Be(ReportStatus.Rejected);
    }

    [Fact]
    public void Assign_WhenAccepted_ShouldMoveToAssigned()
    {
        var report = CreateReport();

        report.Accept();
        report.Assign();

        report.Status.Should().Be(ReportStatus.Assigned);
    }

    [Fact]
    public void Collect_WhenAssigned_ShouldMoveToCollected()
    {
        var report = CreateReport();

        report.Accept();
        report.Assign();
        report.Collect();

        report.Status.Should().Be(ReportStatus.Collected);
    }

    [Fact]
    public void Accept_AfterReject_ShouldThrowInvalidOperationException()
    {
        var report = CreateReport();

        report.Reject();

        var act = () => report.Accept();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot transition report from Rejected to Accepted");
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