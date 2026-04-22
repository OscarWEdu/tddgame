
namespace Backend.Tests;

using Moq;
using TddGame; // Import the application namespace so the tests can access turn models and repository interfaces.

// Contain unit tests for the mocked turns repository.
public class TurnsRepositoryTests
{
  // Store a mock version of ITurnsRepository.
  private readonly Mock<ITurnsRepository> _mockRepo;

  // Initialize the mock repository before each test.
  public TurnsRepositoryTests()
  {
    _mockRepo = new Mock<ITurnsRepository>();
  }

  [Fact]
  // Verify that the current active turn is returned when it exists.
  public async Task GetCurrentTurnByGameSessionIdAsync_ReturnTurn_WhenFound()
  {
    // Arrange
    var gameSessionId = Guid.NewGuid();        // Create a fake game session id for the test.
    
    // Create a fake active turn object that the mock repository should return.
    var dto = new TurnDto(
      Id: 1,
      Round: 1,
      Phase: TurnPhase.build,
      Status: TurnStatus.active,
      CreateAt: DateTime.Today,
      GameSessionId: gameSessionId.ToString(),
      PlayerId: 10
    );

        // Start the mock setup.
        _mockRepo
      .Setup(repo => repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(dto);

    // Act
    // Call the mocked method.
    var result = await _mockRepo.Object.GetCurrentTurnByGameSessionIdAsync(gameSessionId, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result!.Id);
    Assert.Equal(1, result.Round);
    Assert.Equal(TurnPhase.build, result.Phase);
    Assert.Equal(TurnStatus.active, result.Status);
    Assert.Equal(gameSessionId.ToString(), result.GameSessionId);
    Assert.Equal(10, result.PlayerId);
  }

  [Fact]
  // Verify that null is returned when no active turn exists.
  public async Task GetCurrentTurnByGameSessionIdAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo
      .Setup(repo => repo.GetCurrentTurnByGameSessionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((TurnDto?)null);

    var result = await _mockRepo.Object.GetCurrentTurnByGameSessionIdAsync(Guid.NewGuid(), CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  // Verify that all turns for a game session are returned correctly.
  public async Task GetTurnsByGameSessionIdAsync_ReturnTurnsList()
  {
    var gameSessionId = Guid.NewGuid();

    var turns = new List<TurnDto>
    {
      new TurnDto(1, 1, TurnPhase.build, TurnStatus.inactive, DateTime.Today, gameSessionId.ToString(), 10),
      new TurnDto(2, 1, TurnPhase.attack, TurnStatus.inactive, DateTime.Today, gameSessionId.ToString(), 10),
      new TurnDto(3, 1, TurnPhase.build, TurnStatus.active, DateTime.Today, gameSessionId.ToString(), 11)
    };

    _mockRepo
      .Setup(repo => repo.GetTurnsByGameSessionIdAsync(gameSessionId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(turns);

    var result = await _mockRepo.Object.GetTurnsByGameSessionIdAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(3, result.Count());
    Assert.Contains(result, t => t.Status == TurnStatus.active);
  }

  [Fact]
  // Verify that a new turn is created and returned with the expected enum values.
  public async Task CreateTurnAsync_ReturnCreatedTurn()
  {
    var gameSessionId = Guid.NewGuid();

    var createDto = new CreateTurnDto(
      Round: 2,
      Phase: TurnPhase.build,
      Status: TurnStatus.active,
      GameSessionId: gameSessionId.ToString(),
      PlayerId: 11
    );

    var createdDto = new TurnDto(
      Id: 5,
      Round: 2,
      Phase: TurnPhase.build,
      Status: TurnStatus.active,
      CreateAt: DateTime.Today,
      GameSessionId: gameSessionId.ToString(),
      PlayerId: 11
    );

    _mockRepo
      .Setup(repo => repo.CreateTurnAsync(createDto, It.IsAny<CancellationToken>()))
      .ReturnsAsync(createdDto);

    var result = await _mockRepo.Object.CreateTurnAsync(createDto, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(5, result.Id);
    Assert.Equal(createDto.Round, result.Round);
    Assert.Equal(createDto.Phase, result.Phase);
    Assert.Equal(createDto.Status, result.Status);
    Assert.Equal(createDto.GameSessionId, result.GameSessionId);
    Assert.Equal(createDto.PlayerId, result.PlayerId);
  }

  [Fact]
  // Verify that phase update succeeds when a turn is updated.
  public async Task ChangeCurrentTurnPhaseAsync_ReturnTrue_WhenTurnWasUpdated()
  {
    var gameSessionId = Guid.NewGuid();

    _mockRepo
      .Setup(repo => repo.ChangeCurrentTurnPhaseAsync(gameSessionId, TurnPhase.attack, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);

    var result = await _mockRepo.Object.ChangeCurrentTurnPhaseAsync(gameSessionId, TurnPhase.attack, CancellationToken.None);

    Assert.True(result);
  }

  [Fact]
  // Verify that phase update returns false when no turn was updated.
  public async Task ChangeCurrentTurnPhaseAsync_ReturnFalse_WhenNoTurnWasUpdated()
  {
    _mockRepo
      .Setup(repo => repo.ChangeCurrentTurnPhaseAsync(It.IsAny<Guid>(), TurnPhase.attack, It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);

    var result = await _mockRepo.Object.ChangeCurrentTurnPhaseAsync(Guid.NewGuid(), TurnPhase.attack, CancellationToken.None);

    Assert.False(result);
  }

  [Fact]
  // Verify that turn status update succeeds when the row is updated.
  public async Task SetTurnStatusAsync_ReturnTrue_WhenTurnStatusWasUpdated()
  {
    _mockRepo
      .Setup(repo => repo.SetTurnStatusAsync(1, TurnStatus.inactive, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);

    var result = await _mockRepo.Object.SetTurnStatusAsync(1, TurnStatus.inactive, CancellationToken.None);

    Assert.True(result);
  }

  [Fact]
  // Verify that turn status update returns false when no row was updated.
  public async Task SetTurnStatusAsync_ReturnFalse_WhenTurnStatusWasNotUpdated()
  {
    _mockRepo
      .Setup(repo => repo.SetTurnStatusAsync(It.IsAny<int>(), TurnStatus.inactive, It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);

    var result = await _mockRepo.Object.SetTurnStatusAsync(999, TurnStatus.inactive, CancellationToken.None);

    Assert.False(result);
  }

  [Fact]
  // Verify that the first player id is returned when a player exists.
  public async Task GetFirstPlayerIdByGameSessionIdAsync_ReturnFirstPlayerId_WhenFound()
  {
    var gameSessionId = Guid.NewGuid();

    _mockRepo
      .Setup(repo => repo.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(5);

    var result = await _mockRepo.Object.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(5, result);
  }

  [Fact]
  // Verify that null is returned when no player exists.
  public async Task GetFirstPlayerIdByGameSessionIdAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo
      .Setup(repo => repo.GetFirstPlayerIdByGameSessionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((int?)null);

    var result = await _mockRepo.Object.GetFirstPlayerIdByGameSessionIdAsync(Guid.NewGuid(), CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  // Verify that the next player id is returned when it exists.
  public async Task GetNextPlayerIdAsync_ReturnNextPlayerId_WhenFound()
  {
    var gameSessionId = Guid.NewGuid();
    var currentPlayerId = 10;

    _mockRepo
      .Setup(repo => repo.GetNextPlayerIdAsync(gameSessionId, currentPlayerId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(11);

    var result = await _mockRepo.Object.GetNextPlayerIdAsync(gameSessionId, currentPlayerId, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(11, result);
  }

  [Fact]
  // Verify that null is returned when no next player can be found.
  public async Task GetNextPlayerIdAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo
      .Setup(repo => repo.GetNextPlayerIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((int?)null);

    var result = await _mockRepo.Object.GetNextPlayerIdAsync(Guid.NewGuid(), 99, CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  // Verify that the current round number is returned when it exists.
  public async Task GetCurrentRoundAsync_ReturnRound_WhenFound()
  {
    var gameSessionId = Guid.NewGuid();

    _mockRepo
      .Setup(repo => repo.GetCurrentRoundAsync(gameSessionId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(3);

    var result = await _mockRepo.Object.GetCurrentRoundAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(3, result);
  }

  [Fact]
  // Verify that null is returned when no round exists.
  public async Task GetCurrentRoundAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo
      .Setup(repo => repo.GetCurrentRoundAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((int?)null);

    var result = await _mockRepo.Object.GetCurrentRoundAsync(Guid.NewGuid(), CancellationToken.None);

    Assert.Null(result);
  }
}
