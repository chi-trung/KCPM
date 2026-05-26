using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Moq;
using WastePlatform.Application.Citizens.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Application.Citizens;

/// <summary>
/// KIEM-13: Citizen Module Testing - Profile Management
/// Tests for UpdateCitizenProfileCommandHandler and profile-related operations
/// </summary>
[AllureEpic("Citizens")]
[AllureFeature("Citizen Profile Management")]
[AllureLabel("story", "Update citizen profile with validation")]
[AllureLabel("parentSuite", "xUnit Backend Tests")]
[AllureLabel("suite", "Application")]
[AllureLabel("subSuite", "CitizenProfileCommandHandlerTests")]
[AllureLabel("package", "WastePlatform.Tests.Application.Citizens")]
[AllureLabel("KIEM", "KIEM-13")]
[AllureLabel("WRP", "WRP-BE-TESTS-013")]
[AllureOwner("backend-team")]
[AllureSeverity(SeverityLevel.critical)]
[AllureTag("unit")]
[AllureTag("backend")]
[AllureTag("citizen")]
[AllureTag("profile")]
public class CitizenProfileCommandHandlerTests
{
    private readonly Mock<ICitizenRepository> _mockCitizenRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public CitizenProfileCommandHandlerTests()
    {
        _mockCitizenRepository = new Mock<ICitizenRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
    }

    #region TC-CIT-001: Profile Update - Valid Data

    [Fact]
    [AllureDescription("Updates citizen profile successfully with valid name, phone, and address")]
    [AllureLabel("testcase", "TC-CIT-001")]
    public async Task UpdateProfile_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var citizen = new Citizen
        {
            Id = citizenId,
            FullName = "Nguyễn Văn A",
            Email = "a@example.com",
            Phone = "+84912345678",
            Address = "123 Main St, HCMC",
            VerificationStatus = "verified",
            PreferredLanguage = "vi",
            TotalPoints = 1000,
            JoinDate = DateTime.UtcNow.AddMonths(-1)
        };

        var command = new UpdateCitizenProfileCommand
        {
            CitizenId = citizenId,
            FullName = "Nguyễn Văn B",
            Phone = "+84987654321",
            Address = "456 Oak Ave, HCMC",
            PreferredLanguage = "en"
        };

        _mockCitizenRepository
            .Setup(x => x.GetByIdAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(citizen);

        _mockCitizenRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Citizen>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Citizen c, CancellationToken _) => c);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var handler = new UpdateCitizenProfileCommandHandler(_mockCitizenRepository.Object, _mockUnitOfWork.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Nguyễn Văn B");
        result.Phone.Should().Be("+84987654321");
        result.Address.Should().Be("456 Oak Ave, HCMC");
        result.PreferredLanguage.Should().Be("en");

        _mockCitizenRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Citizen>(), It.IsAny<CancellationToken>()),
            Times.Once);

        AllureAttachmentHelper.AttachJson("Updated Profile", result);
    }

    #endregion

    #region TC-CIT-002: Profile Update - Invalid Email

    [Fact]
    [AllureDescription("Rejects profile update with invalid email format")]
    [AllureLabel("testcase", "TC-CIT-002")]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task UpdateProfile_WithInvalidEmail_ShouldThrowValidationException()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var command = new UpdateCitizenProfileCommand
        {
            CitizenId = citizenId,
            Email = "invalid-email-format"
        };

        // Act & Assert
        var handler = new UpdateCitizenProfileCommandHandler(_mockCitizenRepository.Object, _mockUnitOfWork.Object);
        
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region TC-CIT-003: Profile Update - Citizen Not Found

    [Fact]
    [AllureDescription("Throws exception when updating non-existent citizen")]
    [AllureLabel("testcase", "TC-CIT-003")]
    public async Task UpdateProfile_WithNonExistentCitizen_ShouldThrowNotFoundException()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var command = new UpdateCitizenProfileCommand
        {
            CitizenId = citizenId,
            FullName = "New Name"
        };

        _mockCitizenRepository
            .Setup(x => x.GetByIdAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Citizen)null);

        // Act & Assert
        var handler = new UpdateCitizenProfileCommandHandler(_mockCitizenRepository.Object, _mockUnitOfWork.Object);
        
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region TC-CIT-004: Profile Update - Empty Required Fields

    [Fact]
    [AllureDescription("Rejects update with empty required fields")]
    [AllureLabel("testcase", "TC-CIT-004")]
    public async Task UpdateProfile_WithEmptyRequiredFields_ShouldThrowValidationException()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var command = new UpdateCitizenProfileCommand
        {
            CitizenId = citizenId,
            FullName = "" // Empty required field
        };

        // Act & Assert
        var handler = new UpdateCitizenProfileCommandHandler(_mockCitizenRepository.Object, _mockUnitOfWork.Object);
        
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region TC-CIT-005: Profile Update - Oversized String

    [Fact]
    [AllureDescription("Rejects update with oversized string field")]
    [AllureLabel("testcase", "TC-CIT-005")]
    public async Task UpdateProfile_WithOversizedString_ShouldThrowValidationException()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var command = new UpdateCitizenProfileCommand
        {
            CitizenId = citizenId,
            FullName = new string('a', 1000) // Exceeds max length
        };

        // Act & Assert
        var handler = new UpdateCitizenProfileCommandHandler(_mockCitizenRepository.Object, _mockUnitOfWork.Object);
        
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region TC-CIT-006: Profile Get - Successful Retrieval

    [Fact]
    [AllureDescription("Retrieves complete citizen profile successfully")]
    [AllureLabel("testcase", "TC-CIT-006")]
    public async Task GetProfile_WithValidCitizenId_ShouldReturnCompleteProfile()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var expectedProfile = new Citizen
        {
            Id = citizenId,
            FullName = "Nguyễn Văn C",
            Email = "c@example.com",
            Phone = "+84912345678",
            Address = "789 Main Street",
            Avatar = "https://avatar.example.com/user.jpg",
            VerificationStatus = "verified",
            TotalPoints = 2500,
            JoinDate = DateTime.UtcNow.AddYears(-1),
            PreferredLanguage = "vi"
        };

        _mockCitizenRepository
            .Setup(x => x.GetByIdAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        // Act
        var query = new GetCitizenProfileQuery { CitizenId = citizenId };
        var handler = new GetCitizenProfileQueryHandler(_mockCitizenRepository.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Nguyễn Văn C");
        result.Email.Should().Be("c@example.com");
        result.VerificationStatus.Should().Be("verified");
        result.TotalPoints.Should().Be(2500);

        AllureAttachmentHelper.AttachJson("Retrieved Profile", result);
    }

    #endregion
}

/// <summary>
/// KIEM-13: Citizen Module Testing - Rewards Management
/// Tests for citizen rewards retrieval and filtering
/// </summary>
[AllureEpic("Citizens")]
[AllureFeature("Citizen Rewards")]
[AllureLabel("story", "Retrieve and filter citizen rewards")]
[AllureLabel("parentSuite", "xUnit Backend Tests")]
[AllureLabel("suite", "Application")]
[AllureLabel("subSuite", "CitizenRewardsQueryHandlerTests")]
[AllureLabel("package", "WastePlatform.Tests.Application.Citizens")]
[AllureLabel("KIEM", "KIEM-13")]
[AllureOwner("backend-team")]
[AllureSeverity(SeverityLevel.normal)]
[AllureTag("unit")]
[AllureTag("backend")]
[AllureTag("citizen")]
[AllureTag("rewards")]
public class CitizenRewardsQueryHandlerTests
{
    private readonly Mock<IRewardRepository> _mockRewardRepository;

    public CitizenRewardsQueryHandlerTests()
    {
        _mockRewardRepository = new Mock<IRewardRepository>();
    }

    #region TC-CIT-101: Get Rewards - Success

    [Fact]
    [AllureDescription("Retrieves all rewards for citizen successfully")]
    [AllureLabel("testcase", "TC-CIT-101")]
    public async Task GetRewards_WithValidCitizenId_ShouldReturnRewardsList()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var rewards = new List<CitizenReward>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Green Hero Badge",
                Points = 100,
                Category = "reporting",
                UnlockedDate = DateTime.UtcNow.AddMonths(-1),
                Active = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Silver Contributor",
                Points = 250,
                Category = "engagement",
                UnlockedDate = DateTime.UtcNow.AddMonths(-2),
                Active = true
            }
        };

        _mockRewardRepository
            .Setup(x => x.GetCitizenRewardsAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rewards);

        // Act
        var query = new GetCitizenRewardsQuery { CitizenId = citizenId };
        var handler = new GetCitizenRewardsQueryHandler(_mockRewardRepository.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Green Hero Badge");
        result.First().Points.Should().Be(100);

        AllureAttachmentHelper.AttachJson("Retrieved Rewards", result);
    }

    #endregion

    #region TC-CIT-102: Get Rewards - With Category Filter

    [Fact]
    [AllureDescription("Retrieves rewards filtered by category")]
    [AllureLabel("testcase", "TC-CIT-102")]
    public async Task GetRewards_WithCategoryFilter_ShouldReturnFilteredRewards()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var category = "reporting";
        var rewards = new List<CitizenReward>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Green Hero Badge",
                Points = 100,
                Category = category,
                UnlockedDate = DateTime.UtcNow,
                Active = true
            }
        };

        _mockRewardRepository
            .Setup(x => x.GetCitizenRewardsByCategoryAsync(citizenId, category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rewards);

        // Act
        var query = new GetCitizenRewardsByCategory { CitizenId = citizenId, Category = category };
        var handler = new GetCitizenRewardsByCategoryHandler(_mockRewardRepository.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().AllSatisfy(r => r.Category.Should().Be(category));
    }

    #endregion

    #region TC-CIT-103: Get Rewards - Empty List

    [Fact]
    [AllureDescription("Returns empty list when citizen has no rewards")]
    [AllureLabel("testcase", "TC-CIT-103")]
    public async Task GetRewards_WithNoCitizenRewards_ShouldReturnEmptyList()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        _mockRewardRepository
            .Setup(x => x.GetCitizenRewardsAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CitizenReward>());

        // Act
        var query = new GetCitizenRewardsQuery { CitizenId = citizenId };
        var handler = new GetCitizenRewardsQueryHandler(_mockRewardRepository.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion
}

/// <summary>
/// KIEM-13: Citizen Module Testing - Leaderboards
/// Tests for leaderboard query and ranking operations
/// </summary>
[AllureEpic("Citizens")]
[AllureFeature("Citizen Leaderboards")]
[AllureLabel("story", "Retrieve leaderboard rankings and personal stats")]
[AllureLabel("parentSuite", "xUnit Backend Tests")]
[AllureLabel("suite", "Application")]
[AllureLabel("subSuite", "CitizenLeaderboardQueryHandlerTests")]
[AllureLabel("package", "WastePlatform.Tests.Application.Citizens")]
[AllureLabel("KIEM", "KIEM-13")]
[AllureOwner("backend-team")]
[AllureSeverity(SeverityLevel.normal)]
[AllureTag("unit")]
[AllureTag("backend")]
[AllureTag("citizen")]
[AllureTag("leaderboard")]
public class CitizenLeaderboardQueryHandlerTests
{
    private readonly Mock<ILeaderboardRepository> _mockLeaderboardRepository;

    public CitizenLeaderboardQueryHandlerTests()
    {
        _mockLeaderboardRepository = new Mock<ILeaderboardRepository>();
    }

    #region TC-CIT-201: Get Leaderboard - Top Contributors

    [Fact]
    [AllureDescription("Retrieves top contributors leaderboard successfully")]
    [AllureLabel("testcase", "TC-CIT-201")]
    public async Task GetTopContributorsLeaderboard_ShouldReturnRankedList()
    {
        // Arrange
        var leaderboard = new List<LeaderboardEntry>
        {
            new()
            {
                Rank = 1,
                CitizenId = Guid.NewGuid(),
                CitizenName = "Nguyễn Văn A",
                ReportsSubmitted = 45,
                Points = 4500,
                BadgeCount = 8
            },
            new()
            {
                Rank = 2,
                CitizenId = Guid.NewGuid(),
                CitizenName = "Trần Thị B",
                ReportsSubmitted = 38,
                Points = 3800,
                BadgeCount = 6
            },
            new()
            {
                Rank = 3,
                CitizenId = Guid.NewGuid(),
                CitizenName = "Phạm Văn C",
                ReportsSubmitted = 32,
                Points = 3200,
                BadgeCount = 5
            }
        };

        _mockLeaderboardRepository
            .Setup(x => x.GetTopContributorsAsync(10, "month", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaderboard);

        // Act
        var query = new GetTopContributorsLeaderboardQuery { Limit = 10, Period = "month" };
        var handler = new GetTopContributorsLeaderboardHandler(_mockLeaderboardRepository.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.First().Rank.Should().Be(1);
        result.First().CitizenName.Should().Be("Nguyễn Văn A");
        result.First().Points.Should().Be(4500);

        // Verify ranking order
        for (int i = 1; i < result.Count; i++)
        {
            result[i - 1].Points.Should().BeGreaterThan(result[i].Points);
        }

        AllureAttachmentHelper.AttachJson("Top Contributors Leaderboard", result);
    }

    #endregion

    #region TC-CIT-202: Get Personal Leaderboard Stats

    [Fact]
    [AllureDescription("Retrieves personal leaderboard statistics for citizen")]
    [AllureLabel("testcase", "TC-CIT-202")]
    public async Task GetPersonalLeaderboardStats_WithValidCitizenId_ShouldReturnPersonalStats()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var personalStats = new PersonalLeaderboardStats
        {
            CitizenId = citizenId,
            MyRank = 25,
            MyPoints = 1850,
            MyReportsCount = 18,
            Percentile = 75,
            TopInRegion = 5,
            NextMilestonePoints = 2000,
            ProgressPercentage = 92.5m
        };

        _mockLeaderboardRepository
            .Setup(x => x.GetPersonalStatsAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(personalStats);

        // Act
        var query = new GetPersonalLeaderboardStatsQuery { CitizenId = citizenId };
        var handler = new GetPersonalLeaderboardStatsHandler(_mockLeaderboardRepository.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MyRank.Should().Be(25);
        result.MyPoints.Should().Be(1850);
        result.Percentile.Should().Be(75);
        result.ProgressPercentage.Should().Be(92.5m);

        AllureAttachmentHelper.AttachJson("Personal Leaderboard Stats", result);
    }

    #endregion

    #region TC-CIT-203: Get Leaderboard - Invalid Period

    [Fact]
    [AllureDescription("Rejects leaderboard query with invalid period")]
    [AllureLabel("testcase", "TC-CIT-203")]
    public async Task GetLeaderboard_WithInvalidPeriod_ShouldThrowValidationException()
    {
        // Arrange
        var query = new GetTopContributorsLeaderboardQuery { Period = "invalid_period" };

        // Act & Assert
        var handler = new GetTopContributorsLeaderboardHandler(_mockLeaderboardRepository.Object);
        
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(query, CancellationToken.None));
    }

    #endregion

    #region TC-CIT-204: Get Leaderboard - Limit Capping

    [Fact]
    [AllureDescription("Caps leaderboard limit to maximum 100 records")]
    [AllureLabel("testcase", "TC-CIT-204")]
    public async Task GetLeaderboard_WithExcessiveLimit_ShouldCapToMax()
    {
        // Arrange
        var requestedLimit = 1000;
        var maxLimit = 100;

        _mockLeaderboardRepository
            .Setup(x => x.GetTopContributorsAsync(maxLimit, "month", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Range(1, 100)
                .Select(i => new LeaderboardEntry
                {
                    Rank = i,
                    CitizenId = Guid.NewGuid(),
                    CitizenName = $"Citizen {i}",
                    ReportsSubmitted = 100 - i,
                    Points = (100 - i) * 100,
                    BadgeCount = (100 - i) / 10
                })
                .ToList());

        // Act
        var query = new GetTopContributorsLeaderboardQuery { Limit = requestedLimit, Period = "month" };
        var handler = new GetTopContributorsLeaderboardHandler(_mockLeaderboardRepository.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(100);
        result.Should().HaveCount(x => x <= maxLimit);
    }

    #endregion
}
