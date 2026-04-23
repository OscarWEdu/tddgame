namespace Backend.Tests;

using Moq;
using TddGame; // Import the application namespace so the tests can access mission models and repository

public class MissionRepositoryTests
{
  // Store a mock version of IMissionRepository.
  private readonly Mock<IMissionsRepository> _mockRepo;

  // Initialize the mock repository before each test.
  public MissionRepositoryTests()
  {
    _mockRepo = new Mock<IMissionsRepository>();
  }

  [Fact]
  // Verify that the current active mission is returned when it exists.
  public async Task GetMissionsAsync_ReturnMission_WhenFound()
  {
    // Arrange
    var dto = new MissionDto(
      Id: 1,
      Name: "Mission1",
      Description: "Description1"
    );

    // Start the mock setup.
    _mockRepo
  .Setup(repo => repo.GetMissionsAsync(It.IsAny<CancellationToken>()))
  .ReturnsAsync(new List<MissionDto> { dto });

    // Act
    // Call the mocked method.
    var result = await _mockRepo.Object.GetMissionsAsync(CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Single(result!);
    Assert.Equal(1, result.First().Id);
    Assert.Equal("Mission1", result.First().Name);
    Assert.Equal("Description1", result.First().Description);
  }

  [Fact]
  // Verify that null is returned when no active mission exists.
  public async Task GetMissionsAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo
      .Setup(repo => repo.GetMissionsAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync((IEnumerable<MissionDto>?)null);

    var result = await _mockRepo.Object.GetMissionsAsync(CancellationToken.None);

    Assert.Null(result);
  }
}