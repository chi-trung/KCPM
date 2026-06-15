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
        AllureAttachmentHelper.AttachText("test-r-e-j-e-c-t_-w-h-e-n-p-e-n-d-i-n-g_-s-h-o-u-l-d-s-", "Executed: Reject_WhenPending_ShouldSucceed");
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
        AllureAttachmentHelper.AttachText("test-a-s-s-i-g-n_-w-h-e-n-p-e-n-d-i-n-g_-s-h-o-u-l-d-t-", "Executed: Assign_WhenPending_ShouldThrow");
        var report = CreateReport();

        var act = () => report.Assign();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot transition report from Pending to Assigned");
    }

    [Fact]
    [AllureDescription("Rejected → Accepted is invalid (rejected is a terminal state for accept).")]
    public void Accept_WhenRejected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("test-a-c-c-e-p-t_-w-h-e-n-r-e-j-e-c-t-e-d_-s-h-o-u-l-d-", "Executed: Accept_WhenRejected_ShouldThrow");
        var report = CreateReport();
        report.Reject();

        var act = () => report.Accept();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Rejected → Assigned is invalid.")]
    public void Assign_WhenRejected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("test-a-s-s-i-g-n_-w-h-e-n-r-e-j-e-c-t-e-d_-s-h-o-u-l-d-", "Executed: Assign_WhenRejected_ShouldThrow");
        var report = CreateReport();
        report.Reject();

        var act = () => report.Assign();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Rejected → Collected is invalid.")]
    public void Collect_WhenRejected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("test-c-o-l-l-e-c-t_-w-h-e-n-r-e-j-e-c-t-e-d_-s-h-o-u-l-", "Executed: Collect_WhenRejected_ShouldThrow");
        var report = CreateReport();
        report.Reject();

        var act = () => report.Collect();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Rejected → Rejected is invalid (double reject).")]
    public void Reject_WhenRejected_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("test-r-e-j-e-c-t_-w-h-e-n-r-e-j-e-c-t-e-d_-s-h-o-u-l-d-", "Executed: Reject_WhenRejected_ShouldThrow");
        var report = CreateReport();
        report.Reject();

        var act = () => report.Reject();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [AllureDescription("Assigned → Accepted is invalid (cannot go backwards).")]
    public void Accept_WhenAssigned_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("test-a-c-c-e-p-t_-w-h-e-n-a-s-s-i-g-n-e-d_-s-h-o-u-l-d-", "Executed: Accept_WhenAssigned_ShouldThrow");
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
        AllureAttachmentHelper.AttachText("test-a-s-s-i-g-n_-w-h-e-n-a-s-s-i-g-n-e-d_-s-h-o-u-l-d-", "Executed: Assign_WhenAssigned_ShouldThrow");
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
        AllureAttachmentHelper.AttachText("test-a-c-c-e-p-t_-w-h-e-n-c-o-l-l-e-c-t-e-d_-s-h-o-u-l-", "Executed: Accept_WhenCollected_ShouldThrow");
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
        AllureAttachmentHelper.AttachText("test-a-s-s-i-g-n_-w-h-e-n-c-o-l-l-e-c-t-e-d_-s-h-o-u-l-", "Executed: Assign_WhenCollected_ShouldThrow");
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
        AllureAttachmentHelper.AttachText("test-c-o-l-l-e-c-t_-w-h-e-n-c-o-l-l-e-c-t-e-d_-s-h-o-u-", "Executed: Collect_WhenCollected_ShouldThrow");
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
        AllureAttachmentHelper.AttachText("test-r-e-j-e-c-t_-w-h-e-n-c-o-l-l-e-c-t-e-d_-s-h-o-u-l-", "Executed: Reject_WhenCollected_ShouldThrow");
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
        AllureAttachmentHelper.AttachText("test-c-r-e-a-t-e_-w-i-t-h-n-u-l-l-d-e-s-c-r-i-p-t-i-o-n", "Executed: Create_WithNullDescription_ShouldBeNull");
        var report = WasteReport.Create(Guid.NewGuid(), 1, 10m, 106m, description: null);

        report.Description.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Create with null address should set Address to null.")]
    public void Create_WithNullAddress_ShouldBeNull()
    {
        AllureAttachmentHelper.AttachText("test-c-r-e-a-t-e_-w-i-t-h-n-u-l-l-a-d-d-r-e-s-s_-s-h-o-", "Executed: Create_WithNullAddress_ShouldBeNull");
        var report = WasteReport.Create(Guid.NewGuid(), 1, 10m, 106m, address: null);

        report.Address.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Create initializes empty navigation collections.")]
    public void Create_ShouldInitializeEmptyCollections()
    {
        AllureAttachmentHelper.AttachText("test-c-r-e-a-t-e_-s-h-o-u-l-d-i-n-i-t-i-a-l-i-z-e-e-m-p", "Executed: Create_ShouldInitializeEmptyCollections");
        var report = CreateReport();

        report.Images.Should().NotBeNull().And.BeEmpty();
        report.RewardPoints.Should().NotBeNull().And.BeEmpty();
        report.Complaints.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [AllureDescription("Two reports created in sequence should have unique IDs.")]
    public void Create_TwoReports_ShouldHaveUniqueIds()
    {
        AllureAttachmentHelper.AttachText("test-c-r-e-a-t-e_-t-w-o-r-e-p-o-r-t-s_-s-h-o-u-l-d-h-a-", "Executed: Create_TwoReports_ShouldHaveUniqueIds");
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
