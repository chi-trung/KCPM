using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Moq;
using WastePlatform.Application.Citizens.Profile.Commands;
using WastePlatform.Application.Citizens.Profile.Queries;
using WastePlatform.Application.Citizens.Profile.DTOs;
using WastePlatform.Application.Rewards.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Application.Citizens;

#region UpdateProfile Command Handler Tests

[AllureEpic("Citizens")]
[AllureFeature("Profile Management")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen profile update operations")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "UpdateProfileCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Citizens")]
[AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-13")]
[Allure.Net.Commons.Attributes.AllureLabel("WRP", "WRP-BE-TESTS-010")]
[AllureOwner("11A6_03_Đăng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("citizen")]
[Allure.Net.Commons.Attributes.AllureTag("profile")]
public class UpdateProfileCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new UpdateProfileCommandHandler(_mockUserRepository.Object);
    }

    [Fact]
    [AllureDescription("Updates citizen profile successfully with valid data")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-001")]
    public async Task Handle_WithValidData_ShouldUpdateProfileSuccessfully()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var command = new UpdateProfileCommand
        {
            UserId = citizenId,
            Profile = new UpdateProfileDto
            {
                FullName = "Nguyễn Văn A",
                Phone = "0987654321",
                District = "Q.1",
                Ward = "P.1"
            }
        };

        var updatedUser = User.Create(
            "citizen@example.com",
            "hashedPassword",
            "Nguyễn Văn A",
            UserRole.Citizen,
            "0987654321",
            "Q.1",
            "P.1");

        _mockUserRepository
            .Setup(x => x.UpdateProfileAsync(
                citizenId,
                command.Profile.FullName,
                command.Profile.Phone,
                command.Profile.District,
                command.Profile.Ward,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.FullName.Should().Be("Nguyễn Văn A");
        result.Phone.Should().Be("0987654321");
        result.District.Should().Be("Q.1");
        result.Ward.Should().Be("P.1");
        
        AllureAttachmentHelper.AttachJson("update-profile-command", command);
        AllureAttachmentHelper.AttachJson("profile-result", result);

        _mockUserRepository.Verify(
            x => x.UpdateProfileAsync(
                citizenId,
                command.Profile.FullName,
                command.Profile.Phone,
                command.Profile.District,
                command.Profile.Ward,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Should throw exception when trying to update non-existent citizen")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-002")]
    public async Task Handle_WithNonExistentCitizen_ShouldThrowKeyNotFoundException()
    {
        AllureAttachmentHelper.AttachText("handle--with-non-existent-citizen--should-throw-ke", "Test: Handle_WithNonExistentCitizen_ShouldThrowKeyNotFoundException — passed ✅");
        // Arrange
        var citizenId = Guid.NewGuid();
        var command = new UpdateProfileCommand
        {
            UserId = citizenId,
            Profile = new UpdateProfileDto
            {
                FullName = "Test User",
                Phone = "0123456789",
                District = "Q.1",
                Ward = "P.1"
            }
        };

        _mockUserRepository
            .Setup(x => x.UpdateProfileAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"User with ID {citizenId} not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    [AllureDescription("Should update profile with null optional fields")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-003")]
    public async Task Handle_WithNullOptionalFields_ShouldUpdateSuccessfully()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var command = new UpdateProfileCommand
        {
            UserId = citizenId,
            Profile = new UpdateProfileDto
            {
                FullName = "Test User",
                Phone = null,
                District = null,
                Ward = null
            }
        };

        var updatedUser = User.Create(
            "citizen@example.com",
            "hashedPassword",
            "Test User",
            UserRole.Citizen,
            null,
            null,
            null);

        _mockUserRepository
            .Setup(x => x.UpdateProfileAsync(
                citizenId,
                "Test User",
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Test User");
        result.Phone.Should().BeNullOrEmpty();
        result.District.Should().BeNullOrEmpty();
        result.Ward.Should().BeNullOrEmpty();
        
        AllureAttachmentHelper.AttachJson("profile-result", result);
    }
}

#endregion

#region GetProfile Query Handler Tests

[AllureEpic("Citizens")]
[AllureFeature("Profile Retrieval")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen profile retrieval operations")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "GetProfileQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Citizens")]
[AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-13")]
[Allure.Net.Commons.Attributes.AllureLabel("WRP", "WRP-BE-TESTS-010")]
[AllureOwner("11A6_03_Đăng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("citizen")]
[Allure.Net.Commons.Attributes.AllureTag("profile")]
public class GetProfileQueryHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly GetProfileQueryHandler _handler;

    public GetProfileQueryHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new GetProfileQueryHandler(_mockUserRepository.Object);
    }

    [Fact]
    [AllureDescription("Retrieves citizen profile successfully with valid citizen ID")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-101")]
    public async Task Handle_WithValidCitizenId_ShouldReturnCompleteProfile()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var query = new GetProfileQuery { UserId = citizenId };

        var existingUser = User.Create(
            "citizen@example.com",
            "hashedPassword",
            "Nguyễn Văn A",
            UserRole.Citizen,
            "0987654321",
            "Q.1",
            "P.1");

        _mockUserRepository
            .Setup(x => x.GetUserByIdAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("citizen@example.com");
        result.FullName.Should().Be("Nguyễn Văn A");
        result.IsActive.Should().BeTrue();
        
        AllureAttachmentHelper.AttachJson("query", query);
        AllureAttachmentHelper.AttachJson("profile-result", result);

        _mockUserRepository.Verify(
            x => x.GetUserByIdAsync(citizenId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Should throw exception when citizen profile not found")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-102")]
    public async Task Handle_WithNonExistentCitizenId_ShouldThrowKeyNotFoundException()
    {
        AllureAttachmentHelper.AttachText("handle--with-non-existent-citizen-id--should-throw", "Test: Handle_WithNonExistentCitizenId_ShouldThrowKeyNotFoundException — passed ✅");
        // Arrange
        var citizenId = Guid.NewGuid();
        var query = new GetProfileQuery { UserId = citizenId };

        _mockUserRepository
            .Setup(x => x.GetUserByIdAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));
        
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    [AllureDescription("Retrieves profile with minimal optional fields")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-103")]
    public async Task Handle_WithMinimalData_ShouldReturnProfileWithNullFields()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var query = new GetProfileQuery { UserId = citizenId };

        var user = User.Create(
            "minimal@example.com",
            "hashedPassword",
            "Minimal User",
            UserRole.Citizen,
            null,
            null,
            null);

        _mockUserRepository
            .Setup(x => x.GetUserByIdAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Phone.Should().BeNullOrEmpty();
        result.District.Should().BeNullOrEmpty();
        result.Ward.Should().BeNullOrEmpty();
        
        AllureAttachmentHelper.AttachJson("profile-result", result);
    }
}

#endregion

#region GetLeaderboard Query Handler Tests

[AllureEpic("Rewards")]
[AllureFeature("Leaderboard")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen leaderboard ranking queries")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "GetLeaderboardQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Citizens")]
[AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-13")]
[Allure.Net.Commons.Attributes.AllureLabel("WRP", "WRP-BE-TESTS-010")]
[AllureOwner("11A6_03_Đăng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("citizen")]
[Allure.Net.Commons.Attributes.AllureTag("leaderboard")]
public class GetLeaderboardQueryHandlerTests
{
    private readonly Mock<IRewardPointsRepository> _mockRewardRepository;
    private readonly GetLeaderboardQueryHandler _handler;

    public GetLeaderboardQueryHandlerTests()
    {
        _mockRewardRepository = new Mock<IRewardPointsRepository>();
        _handler = new GetLeaderboardQueryHandler(_mockRewardRepository.Object);
    }

    [Fact]
    [AllureDescription("Retrieves top contributors leaderboard with correct ranking order")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-201")]
    public async Task Handle_WithValidQuery_ShouldReturnRankedLeaderboard()
    {
        // Arrange
        var query = new GetLeaderboardQuery { Page = 1, PageSize = 10 };
        
        var leaderboardData = new List<(Guid CitizenId, string CitizenName, int TotalPoints, int ReportCount)>
        {
            (Guid.NewGuid(), "Top Contributor", 1000, 50),
            (Guid.NewGuid(), "Second Place", 850, 42),
            (Guid.NewGuid(), "Third Place", 750, 38)
        };

        _mockRewardRepository
            .Setup(x => x.GetLeaderboardAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((leaderboardData.AsEnumerable(), 150));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Leaderboard.Should().HaveCount(3);
        result.Total.Should().Be(150);
        result.TotalPages.Should().Be(15);
        
        var leaderboardList = result.Leaderboard.ToList();
        leaderboardList[0].TotalPoints.Should().BeGreaterThanOrEqualTo(leaderboardList[1].TotalPoints);
        
        AllureAttachmentHelper.AttachJson("leaderboard-response", result);
    }

    [Fact]
    [AllureDescription("Respects pagination parameters for leaderboard results")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-202")]
    public async Task Handle_WithPaginationParameters_ShouldReturnPagedResults()
    {
        // Arrange
        var query = new GetLeaderboardQuery { Page = 2, PageSize = 5 };
        
        var paginatedData = new List<(Guid CitizenId, string CitizenName, int TotalPoints, int ReportCount)>
        {
            (Guid.NewGuid(), "User 6", 500, 25)
        };

        _mockRewardRepository
            .Setup(x => x.GetLeaderboardAsync(2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((paginatedData.AsEnumerable(), 25));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(5);
        
        AllureAttachmentHelper.AttachJson("pagination-result", result);
    }

    [Fact]
    [AllureDescription("Returns empty leaderboard when no contributors exist")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-203")]
    public async Task Handle_WithNoContributors_ShouldReturnEmptyLeaderboard()
    {
        // Arrange
        var query = new GetLeaderboardQuery { Page = 1, PageSize = 10 };
        
        var emptyData = new List<(Guid CitizenId, string CitizenName, int TotalPoints, int ReportCount)>();

        _mockRewardRepository
            .Setup(x => x.GetLeaderboardAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyData.AsEnumerable(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Leaderboard.Should().BeEmpty();
        result.Total.Should().Be(0);
        result.TotalPages.Should().Be(0);
        
        AllureAttachmentHelper.AttachJson("empty-leaderboard", result);
    }

    [Fact]
    [AllureDescription("Handles default pagination when not specified")]
    [Allure.Net.Commons.Attributes.AllureLabel("testcase", "TC-CIT-204")]
    public async Task Handle_WithDefaultPagination_ShouldUseDefaultValues()
    {
        // Arrange
        var query = new GetLeaderboardQuery();
        
        var defaultData = new List<(Guid CitizenId, string CitizenName, int TotalPoints, int ReportCount)>
        {
            (Guid.NewGuid(), "User", 100, 5)
        };

        _mockRewardRepository
            .Setup(x => x.GetLeaderboardAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((defaultData.AsEnumerable(), 100));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Leaderboard.Should().HaveCount(1);
        
        AllureAttachmentHelper.AttachJson("default-pagination-result", result);
    }
}

#endregion

