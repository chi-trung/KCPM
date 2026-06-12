using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Domain;

[AllureEpic("Domain Model")]
[AllureFeature("User Entity")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "User lifecycle: creation, activation, role, profile")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Domain")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "UserTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Domain")]
[AllureOwner("Chi Trung")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("domain")]
public class UserTests
{
    [Fact]
    [AllureDescription("Creates a user with all required fields and verifies initial state.")]
    public void Create_ShouldInitializeUserWithCorrectDefaults()
    {
        var user = User.Create("test@example.com", "hashedPassword", "Test User", UserRole.Citizen);

        AllureAttachmentHelper.AttachJson("user-create-result", new
        {
            user.Id, user.Email, user.FullName, user.Role, user.IsActive
        });

        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be("test@example.com");
        user.FullName.Should().Be("Test User");
        user.Role.Should().Be(UserRole.Citizen);
        user.IsActive.Should().BeTrue();
        user.Phone.Should().BeNull();
        user.District.Should().BeNull();
        user.Ward.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Verifies email is normalized to lowercase on creation.")]
    public void Create_ShouldNormalizeEmailToLowercase()
    {
        var user = User.Create("Test@EXAMPLE.Com", "hash", "Test", UserRole.Citizen);

        user.Email.Should().Be("test@example.com");
    }

    [Fact]
    [AllureDescription("Creates a user with optional fields: phone, district, ward.")]
    public void Create_WithOptionalFields_ShouldSetAllProperties()
    {
        var user = User.Create(
            "user@example.com", "hash", "Full Name", UserRole.Enterprise,
            phone: "0901234567", district: "Quận 1", ward: "Phường Bến Nghé");

        AllureAttachmentHelper.AttachJson("user-create-full", new
        {
            user.Phone, user.District, user.Ward, user.Role
        });

        user.Phone.Should().Be("0901234567");
        user.District.Should().Be("Quận 1");
        user.Ward.Should().Be("Phường Bến Nghé");
        user.Role.Should().Be(UserRole.Enterprise);
    }

    [Fact]
    [AllureDescription("Creates users with all 4 roles and verifies each role is set correctly.")]
    public void Create_WithAllRoles_ShouldSetCorrectRole()
    {
        var citizen = User.Create("c@x.com", "h", "C", UserRole.Citizen);
        var enterprise = User.Create("e@x.com", "h", "E", UserRole.Enterprise);
        var collector = User.Create("co@x.com", "h", "Co", UserRole.Collector);
        var admin = User.Create("a@x.com", "h", "A", UserRole.Admin);

        citizen.Role.Should().Be(UserRole.Citizen);
        enterprise.Role.Should().Be(UserRole.Enterprise);
        collector.Role.Should().Be(UserRole.Collector);
        admin.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    [AllureDescription("Deactivates a user and verifies IsActive is false and UpdatedAt is set.")]
    public void Deactivate_ShouldSetIsActiveFalseAndUpdateTimestamp()
    {
        var user = User.Create("user@x.com", "hash", "User", UserRole.Citizen);
        user.IsActive.Should().BeTrue();

        user.Deactivate();

        AllureAttachmentHelper.AttachJson("user-deactivate", new { user.IsActive, user.UpdatedAt });

        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [AllureDescription("Activates a deactivated user and verifies IsActive is restored.")]
    public void Activate_AfterDeactivate_ShouldRestoreIsActive()
    {
        var user = User.Create("user@x.com", "hash", "User", UserRole.Citizen);
        user.Deactivate();
        user.IsActive.Should().BeFalse();

        user.Activate();

        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    [AllureDescription("Updates user role and verifies the new role is persisted.")]
    public void UpdateRole_ShouldChangeRoleAndUpdateTimestamp()
    {
        var user = User.Create("user@x.com", "hash", "User", UserRole.Citizen);

        user.UpdateRole(UserRole.Admin);

        AllureAttachmentHelper.AttachJson("user-role-update", new { user.Role, user.UpdatedAt });

        user.Role.Should().Be(UserRole.Admin);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    [AllureDescription("Updates user profile fields and verifies all fields are updated.")]
    public void UpdateProfile_ShouldUpdateAllFields()
    {
        var user = User.Create("user@x.com", "hash", "Old Name", UserRole.Citizen);

        user.UpdateProfile("New Name", "0909999888", "Quận 7", "Phường Tân Phong");

        AllureAttachmentHelper.AttachJson("user-profile-update", new
        {
            user.FullName, user.Phone, user.District, user.Ward, user.UpdatedAt
        });

        user.FullName.Should().Be("New Name");
        user.Phone.Should().Be("0909999888");
        user.District.Should().Be("Quận 7");
        user.Ward.Should().Be("Phường Tân Phong");
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    [AllureDescription("Updates profile with null optional fields, clearing previous values.")]
    public void UpdateProfile_WithNulls_ShouldClearOptionalFields()
    {
        var user = User.Create("user@x.com", "hash", "Name", UserRole.Citizen,
            phone: "123", district: "D1", ward: "W1");

        user.UpdateProfile("Name", null, null, null);

        user.Phone.Should().BeNull();
        user.District.Should().BeNull();
        user.Ward.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Creates two users and verifies they have unique IDs.")]
    public void Create_TwoUsers_ShouldHaveUniqueIds()
    {
        var user1 = User.Create("u1@x.com", "hash", "User1", UserRole.Citizen);
        var user2 = User.Create("u2@x.com", "hash", "User2", UserRole.Citizen);

        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    [AllureDescription("Verifies CreatedAt is set to approximately now on creation.")]
    public void Create_ShouldSetCreatedAtToNow()
    {
        var before = DateTime.UtcNow;
        var user = User.Create("u@x.com", "hash", "U", UserRole.Citizen);
        var after = DateTime.UtcNow;

        user.CreatedAt.Should().BeOnOrAfter(before.AddSeconds(-1));
        user.CreatedAt.Should().BeOnOrBefore(after.AddSeconds(1));
    }

    [Fact]
    [AllureDescription("Verifies UpdatedAt is null on fresh user (no mutations yet).")]
    public void Create_ShouldHaveNullUpdatedAt()
    {
        var user = User.Create("u@x.com", "hash", "U", UserRole.Citizen);

        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Navigation collections should be initialized as empty lists.")]
    public void Create_ShouldInitializeEmptyNavigationCollections()
    {
        var user = User.Create("u@x.com", "hash", "U", UserRole.Citizen);

        user.WasteReports.Should().NotBeNull().And.BeEmpty();
        user.RewardPoints.Should().NotBeNull().And.BeEmpty();
        user.Complaints.Should().NotBeNull().And.BeEmpty();
        user.AuditLogs.Should().NotBeNull().And.BeEmpty();
    }
}
