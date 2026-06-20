using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Domain;

/// <summary>
/// Extended WasteReport tests focusing on state machine edge cases
/// and transition matrix completeness (supplement to WasteReportTests.cs)
/// </summary>
[AllureEpic("Domain Model")]
[AllureFeature("Waste Report State Machine")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "WasteReport state transition matrix coverage")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Domain")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "WasteReportExtendedTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Domain")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("domain")]
[Allure.Net.Commons.Attributes.AllureTag("state-machine")]
public class WasteReportExtendedTests
{
    #region Valid Transitions (Complete Matrix)

    [Fact]
    [AllureDescription("Accepted → Collected is a valid direct transition (bypass Assigned).")]
    public void Collect_WhenAccepted_ShouldSucceed()
    {
        var report = CreateReport();
        report.Accept();

        report.Collect();

        report.Status.Should().Be(ReportStatus.Collected);
        AllureAttachmentHelper.AttachText("accepted-to-collected", "Accepted → Collected ✅");
    }

    [Fact]
    [AllureDescription("Pending → Rejected is a valid transition.")]
    public void Reject_WhenPending_ShouldSucceed()
    {
        AllureAttachmentHelper.AttachText("pending-to-rejected", "Pending → Reject() → Rejected ✅");
        var report = CreateReport();

        report.Reject();

        report.Status.Should().Be(ReportStatus.Rejected);
    }

    #endregion

    #region Invalid Transitions (Exhaustive)

    [Fact]
    [AllureDescription("Pending → Assigned is invalid (must Accept first).")]
    public void Assign_WhenPending_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-pending-to-assigned", "Pending → Assign() throws InvalidOperationException (must Accept first) ❌");
        var report = CreateReport();

        var act = () => report.Assign();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot transition report from Pending to Assigned");
    }

    [Fact]
    [AllureDescription("Rejected → Accepted is invalid (rejected is a terminal state for accept).")]
    public void Accept_WhenRejected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-rejected-to-accepted", "Rejected → Accept() throws InvalidOperationException (rejected is terminal for accept) ❌");
        var report = CreateReport();
        report.Reject();

        var act = () => report.Accept();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Rejected → Assigned is invalid.")]
    public void Assign_WhenRejected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-rejected-to-assigned", "Rejected → Assign() throws InvalidOperationException ❌");
        var report = CreateReport();
        report.Reject();

        var act = () => report.Assign();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Rejected → Collected is invalid.")]
    public void Collect_WhenRejected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-rejected-to-collected", "Rejected → Collect() throws InvalidOperationException ❌");
        var report = CreateReport();
        report.Reject();

        var act = () => report.Collect();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Rejected → Rejected is invalid (double reject).")]
    public void Reject_WhenRejected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-double-reject", "Rejected → Reject() throws InvalidOperationException (double reject) ❌");
        var report = CreateReport();
        report.Reject();

        var act = () => report.Reject();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Assigned → Accepted is invalid (cannot go backwards).")]
    public void Accept_WhenAssigned_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-assigned-to-accepted", "Assigned → Accept() throws InvalidOperationException (no backward transition) ❌");
        var report = CreateReport();
        report.Accept();
        report.Assign();

        var act = () => report.Accept();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Assigned → Assigned is invalid (double assign).")]
    public void Assign_WhenAssigned_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-double-assign", "Assigned → Assign() throws InvalidOperationException (double assign) ❌");
        var report = CreateReport();
        report.Accept();
        report.Assign();

        var act = () => report.Assign();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Collected → Accepted is invalid (final state).")]
    public void Accept_WhenCollected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-collected-to-accepted", "Collected → Accept() throws InvalidOperationException (Collected is final state) ❌");
        var report = CreateReport();
        report.Accept();
        report.Assign();
        report.Collect();

        var act = () => report.Accept();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Collected → Assigned is invalid (final state).")]
    public void Assign_WhenCollected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-collected-to-assigned", "Collected → Assign() throws InvalidOperationException (Collected is final state) ❌");
        var report = CreateReport();
        report.Accept();
        report.Assign();
        report.Collect();

        var act = () => report.Assign();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Collected → Collected is invalid (double collect).")]
    public void Collect_WhenCollected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-double-collect", "Collected → Collect() throws InvalidOperationException (double collect) ❌");
        var report = CreateReport();
        report.Accept();
        report.Assign();
        report.Collect();

        var act = () => report.Collect();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Collected → Rejected is invalid (final state).")]
    public void Reject_WhenCollected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("invalid-collected-to-rejected", "Collected → Reject() throws InvalidOperationException (Collected is final state) ❌");
        var report = CreateReport();
        report.Accept();
        report.Assign();
        report.Collect();

        var act = () => report.Reject();

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Create Edge Cases

    [Fact]
    [AllureDescription("Create with null description should set Description to null.")]
    public void Create_WithNullDescription_ShouldBeNull()
    {
        AllureAttachmentHelper.AttachText("create-null-description", "Description = null when not provided to WasteReport.Create ✅");
        var report = WasteReport.Create(Guid.NewGuid(), 1, 10m, 106m, description: null);

        report.Description.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Create with null address should set Address to null.")]
    public void Create_WithNullAddress_ShouldBeNull()
    {
        AllureAttachmentHelper.AttachText("create-null-address", "Address = null when not provided to WasteReport.Create ✅");
        var report = WasteReport.Create(Guid.NewGuid(), 1, 10m, 106m, address: null);

        report.Address.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Create initializes empty navigation collections.")]
    public void Create_ShouldInitializeEmptyCollections()
    {
        AllureAttachmentHelper.AttachText("report-nav-collections-empty", "Images=[], RewardPoints=[], Complaints=[] — all initialized empty ✅");
        var report = CreateReport();

        report.Images.Should().NotBeNull().And.BeEmpty();
        report.RewardPoints.Should().NotBeNull().And.BeEmpty();
        report.Complaints.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [AllureDescription("Two reports created in sequence should have unique IDs.")]
    public void Create_TwoReports_ShouldHaveUniqueIds()
    {
        AllureAttachmentHelper.AttachText("unique-report-ids", "r1.Id ≠ r2.Id (both unique GUIDs) ✅");
        var r1 = CreateReport();
        var r2 = CreateReport();

        r1.Id.Should().NotBe(r2.Id);
    }

    #endregion

    private static WasteReport CreateReport()
    {
        return WasteReport.Create(
            citizenId: Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude: 10.762m,
            longitude: 106.660m,
            description: "Test",
            address: "Test address",
            aiSuggestion: "Mixed");
    }
}

