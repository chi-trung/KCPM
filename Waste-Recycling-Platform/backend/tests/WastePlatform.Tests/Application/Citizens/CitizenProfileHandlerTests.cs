using Moq;
using WastePlatform.Application.Citizens.Profile.Commands;
using WastePlatform.Application.Citizens.Profile.DTOs;
using WastePlatform.Application.Citizens.Profile.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Enterprise.Queries;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Tests.Application.Citizens;

[AllureEpic("Citizens")]
[AllureFeature("Citizen Profile and Enterprise Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen profile CRUD and enterprise context queries")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CitizenProfileHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Citizens")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("citizens")]
public class CitizenProfileHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IEnterpriseRepository> _mockEnterpriseRepo;

    public CitizenProfileHandlerTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockEnterpriseRepo = new Mock<IEnterpriseRepository>();
    }

    private static User CreateUser(string name = "Test User")
    {
        return User.Create(
            email: "test@example.com",
            passwordHash: "hashed_password",
            fullName: name,
            role: UserRole.Citizen,
            phone: "0901234567",
            district: "District 1",
            ward: "Ward 1"
        );
    }

    #region GetProfileQueryHandler

    [Fact]
    [AllureDescription("GetProfile returns ProfileDto when user is found.")]
    public async Task GetProfile_WhenUserFound_ShouldReturnProfileDto()
    {
        // Arrange
        var user = CreateUser("Nguyen Van A");

        _mockUserRepo
            .Setup(x => x.GetUserByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetProfileQueryHandler(_mockUserRepo.Object);
        var query = new GetProfileQuery { UserId = user.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.FullName.Should().Be("Nguyen Van A");
        result.Email.Should().Be("test@example.com");
        result.Phone.Should().Be("0901234567");
        result.District.Should().Be("District 1");
        result.Ward.Should().Be("Ward 1");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    [AllureDescription("GetProfile throws KeyNotFoundException when user does not exist.")]
    public async Task GetProfile_WhenUserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepo
            .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new GetProfileQueryHandler(_mockUserRepo.Object);
        var query = new GetProfileQuery { UserId = userId };

        // Act & Assert
        var act = () => handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{userId}*");
    }

    #endregion

    #region UpdateProfileCommandHandler

    [Fact]
    [AllureDescription("UpdateProfile returns updated ProfileDto with new information.")]
    public async Task UpdateProfile_ShouldReturnUpdatedProfileDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updatedUser = User.Create("updated@example.com", "hash", "Updated Name",
            UserRole.Citizen, "0987654321", "District 3", "Ward 5");

        _mockUserRepo
            .Setup(x => x.UpdateProfileAsync(
                userId,
                "Updated Name",
                "0987654321",
                "District 3",
                "Ward 5",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        var handler = new UpdateProfileCommandHandler(_mockUserRepo.Object);
        var command = new UpdateProfileCommand
        {
            UserId = userId,
            Profile = new UpdateProfileDto
            {
                FullName = "Updated Name",
                Phone = "0987654321",
                District = "District 3",
                Ward = "Ward 5"
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Updated Name");
        result.Phone.Should().Be("0987654321");
        result.District.Should().Be("District 3");
        result.Ward.Should().Be("Ward 5");
        _mockUserRepo.Verify(
            x => x.UpdateProfileAsync(userId, "Updated Name", "0987654321", "District 3", "Ward 5", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetEnterpriseByUserIdQueryHandler

    [Fact]
    [AllureDescription("GetEnterpriseByUserId returns enterprise DTO when enterprise exists for user.")]
    public async Task GetEnterpriseByUserId_WhenFound_ShouldReturnDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var enterpriseDto = new EnterpriseDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyName = "Green Enterprise Co",
            IsVerified = true
        };

        _mockEnterpriseRepo
            .Setup(x => x.GetEnterpriseByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enterpriseDto);

        var handler = new GetEnterpriseByUserIdQueryHandler(_mockEnterpriseRepo.Object);
        var query = new GetEnterpriseByUserIdQuery { UserId = userId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Green Enterprise Co");
        result.IsVerified.Should().BeTrue();
    }

    [Fact]
    [AllureDescription("GetEnterpriseByUserId returns null when no enterprise is associated with user.")]
    public async Task GetEnterpriseByUserId_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockEnterpriseRepo
            .Setup(x => x.GetEnterpriseByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnterpriseDto?)null);

        var handler = new GetEnterpriseByUserIdQueryHandler(_mockEnterpriseRepo.Object);
        var query = new GetEnterpriseByUserIdQuery { UserId = userId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
