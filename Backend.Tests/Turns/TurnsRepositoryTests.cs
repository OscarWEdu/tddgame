// These test the repository interface behavior as mocked.
// create Mock<ITurnsRepository>
// tell it what to return
// verify that the returned value matches what you configured

namespace Backend.Tests;
using Moq;
using TddGame; // Import your application namespace so the test can access TurnDto, CreateTurnDto, and ITurnsRepository.

public class TurnsRepositoryTests
{
  // Create a private field to hold a mock version of ITurnsRepository.
  private readonly Mock<ITurnsRepository> _mockRepo;

  public TurnsRepositoryTests()
  {
    // Initializes the mock repository.
      _mockRepo = new Mock<ITurnsRepository>();
  }

  [Fact]
  public async Task GetCurrentTurnByGameSessionIdAsync_ReturnTurn_WhenFound()
  {
    // Arrange
    // Create a fake game session id for the test.
    var gameSessionId = Guid.NewGuid();

    // Create a fake active turn object that the mock repository should return.
    var dto = new TurnDto(
        Id: 1, // Fake turn id.
        Round: 1, // Fake round number.
        Phase: "build", // Fake phase.
        Status: "active", // Fake status
        CreateAt: DateTime.Today, // Fake creation date.
        GameSessionId: gameSessionId.ToString(), // Matches the fake game session id.
        PlayerId: 10 // Fake player id.
    );

    // Start the mock setup.
    _mockRepo
        .Setup(repo => repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, It.IsAny<CancellationToken>())) // when this method is called with this session id...
        .ReturnsAsync(dto); // ...return this fake TurnDto.

    // Act
    // Call the mocked method.
    var result = await _mockRepo.Object.GetCurrentTurnByGameSessionIdAsync(gameSessionId, CancellationToken.None);

    // Assert
    Assert.NotNull(result); // Verify that the result is not null.
    Assert.Equal(1, result.Id); // Verify that the turn id is correct.
    Assert.Equal(1, result.Round); // Verify that the round is correct.
    Assert.Equal("build", result.Phase); // Verify that the phase is correct.
    Assert.Equal("active", result.Status); // Verify the status
    Assert.Equal(gameSessionId.ToString(), result.GameSessionId); // Verify that the game session id is correct.
    Assert.Equal(10, result.PlayerId); // Verify that the player id is correct.
  }

  [Fact]
  // Test name describing expected behavior.
  public async Task GetCurrentTurnByGameSessionIdAsync_ReturnNull_WhenNotFound()
  {
    // Start the mock setup
    _mockRepo
        .Setup(repo => repo.GetCurrentTurnByGameSessionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())) // for any session id...
        .ReturnsAsync((TurnDto?)null); // ...return null.

    var result = await _mockRepo.Object.GetCurrentTurnByGameSessionIdAsync(Guid.NewGuid(), CancellationToken.None);

    // Verify that the result is null.
    Assert.Null(result);
  }

  [Fact]
  // Test that all turns for a game session are returned.
  public async Task GetTurnsByGameSessionIdAsync_ReturnTurnsList()
  {
    // Create a fake game session id.
    var gameSessionId = Guid.NewGuid(); 

    var turns = new List<TurnDto> // Create fake turn history for the game session.
        {
            new TurnDto(1, 1, "build", "inactive", DateTime.Today, gameSessionId.ToString(), 10), // First turn row.
            new TurnDto(2, 1, "attack", "inactive", DateTime.Today, gameSessionId.ToString(), 10), // Second turn row.
            new TurnDto(3, 1, "build", "active", DateTime.Today, gameSessionId.ToString(), 11) // Current active turn row.
        };

    _mockRepo
        .Setup(repo => repo.GetTurnsByGameSessionIdAsync(gameSessionId, It.IsAny<CancellationToken>())) // Match the game session id.
        .ReturnsAsync(turns); // Return the fake turn list.

    var result = await _mockRepo.Object.GetTurnsByGameSessionIdAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result); // Verify the result is not null.
    Assert.Equal(3, result.Count()); // Verify the list contains three rows.
    Assert.Contains(result, t => t.Status == "active"); // Verify one active turn exists in the result.
  }
    

  [Fact]
  public async Task CreateTurnAsync_ReturnCreatedTurn()
  {
    // Create a fake game session id
    var gameSessionId = Guid.NewGuid();

    // Create fake input object that would normally be sent to the repository.
    var createDto = new CreateTurnDto(
        Round: 2, // The round to create.
        Phase: "build", // The phase to create.
        Status: "active",
        GameSessionId: gameSessionId.ToString(), // The game session id for the new turn.
        PlayerId: 11 // The player id for the new turn.
    );

    // Create the fake rturn that the mock should return.
    var createdDto = new TurnDto(
        Id: 5, // Fake generated turn id.
        Round: 2, // Same round.
        Phase: "build", // Same phase.
        Status: "active",
        CreateAt: DateTime.Today, // Fake created date.
        GameSessionId: gameSessionId.ToString(), // Same session id.
        PlayerId: 11 // Same player id.
    );

    _mockRepo
        .Setup(repo => repo.CreateTurnAsync(createDto, It.IsAny<CancellationToken>())) // Match the exact CreateTurnDto
        .ReturnsAsync(createdDto); // Return this fake created TurnDto.

    // Call the mocked method.
    var result = await _mockRepo.Object.CreateTurnAsync(createDto, CancellationToken.None); 

    Assert.NotNull(result); // Verify that the returned object is not null.
    Assert.Equal(5, result.Id); // Verify the id.
    Assert.Equal(createDto.Round, result.Round); // Verify the round.
    Assert.Equal(createDto.Phase, result.Phase); // Verify the phase.
    Assert.Equal(createDto.Status, result.Status); // Verify the status.
    Assert.Equal(createDto.GameSessionId, result.GameSessionId); // Verify the session id.
    Assert.Equal(createDto.PlayerId, result.PlayerId); // Verify the player id.
  }

  [Fact]
  // Test that phase change succeeds.
  public async Task ChangeCurrentTurnPhaseAsync_ReturnTrue_WhenTurnWasUpdated()
  {
      var gameSessionId = Guid.NewGuid();

      _mockRepo
          .Setup(repo => repo.ChangeCurrentTurnPhaseAsync(gameSessionId, "attack", It.IsAny<CancellationToken>())) // Match expected phase change call.
          .ReturnsAsync(true); // Return true to simulate successful update.

      var result = await _mockRepo.Object.ChangeCurrentTurnPhaseAsync(gameSessionId, "attack", CancellationToken.None);

      Assert.True(result);
  }

  [Fact]
  // Tests that phase change fails when no active turn exists.
  public async Task ChangeCurrentTurnPhaseAsync_ReturnFalse_WhenNoTurnWasUpdated()
  {
      _mockRepo
          .Setup(repo => repo.ChangeCurrentTurnPhaseAsync(It.IsAny<Guid>(), "attack", It.IsAny<CancellationToken>())) // Matches any Guid.
          .ReturnsAsync(false);

      var result = await _mockRepo.Object.ChangeCurrentTurnPhaseAsync(Guid.NewGuid(), "attack", CancellationToken.None);

      Assert.False(result);
  }

  [Fact]
  // Test that changing status works.
  public async Task SetTurnStatusAsync_ReturnTrue_WhenTurnStatusWasUpdated()
  {
      _mockRepo
          .Setup(repo => repo.SetTurnStatusAsync(1, "inactive", It.IsAny<CancellationToken>())) // Matches expected turn id and status.
          .ReturnsAsync(true); // Return true to simulate successful update.

      var result = await _mockRepo.Object.SetTurnStatusAsync(1, "inactive", CancellationToken.None);

      Assert.True(result);
  }

  [Fact]
  // Test that changing status can fail.
  public async Task SetTurnStatusAsync_ReturnFalse_WhenTurnStatusWasNotUpdated()
  {
    _mockRepo
        .Setup(repo => repo.SetTurnStatusAsync(It.IsAny<int>(), "inactive", It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    var result = await _mockRepo.Object.SetTurnStatusAsync(999, "inactive", CancellationToken.None);

    Assert.False(result);
  }
    
  [Fact]
  public async Task GetFirstPlayerIdByGameSessionIdAsync_ReturnFirstPlayerId_WhenFound()
  {
    // Fake game session id.
    var gameSessionId = Guid.NewGuid();

    _mockRepo
        .Setup(repo => repo.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, It.IsAny<CancellationToken>())) // when this method is called with the session id above...
        .ReturnsAsync(5); // ...return player id 5.

    var result = await _mockRepo.Object.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result); // Verify the result is not null.
    Assert.Equal(5, result); // Verify the correct player id is returned.
  }

  [Fact]
  // Test null when no player exists.
  public async Task GetFirstPlayerIdByGameSessionIdAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo
        .Setup(repo => repo.GetFirstPlayerIdByGameSessionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())) // this method says, for any session id...
        .ReturnsAsync((int?)null); // ...return null.

    var result = await _mockRepo.Object.GetFirstPlayerIdByGameSessionIdAsync(Guid.NewGuid(), CancellationToken.None);

    Assert.Null(result); // Verify that the result is null.
  }

  [Fact]
  // Test that the next player id is returned.
  public async Task GetNextPlayerIdAsync_ReturnNextPlayerId_WhenFound()
  {
    var gameSessionId = Guid.NewGuid(); // Fake session id.
    var currentPlayerId = 10; // Fake current player id.

    _mockRepo
        .Setup(repo => repo.GetNextPlayerIdAsync(gameSessionId, currentPlayerId, It.IsAny<CancellationToken>())) // when this exact call happens...
        .ReturnsAsync(11); // ...return player id 11.

    var result = await _mockRepo.Object.GetNextPlayerIdAsync(gameSessionId, currentPlayerId, CancellationToken.None); // Calls the mocked method.

    Assert.NotNull(result); // Verify the result is not null.
    Assert.Equal(11, result); // Verify the next player id.
  }

  [Fact]
  // Test null when no next player can be found.
  public async Task GetNextPlayerIdAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo // Starts mock setup.
        .Setup(repo => repo.GetNextPlayerIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>())) // this method says, for any arguments...
        .ReturnsAsync((int?)null); // ...return null.

    var result = await _mockRepo.Object.GetNextPlayerIdAsync(Guid.NewGuid(), 99, CancellationToken.None);

    Assert.Null(result); // Verify the result is null.
  }

  [Fact]
  // Test that the current round is returned.
  public async Task GetCurrentRoundAsync_ReturnRound_WhenFound()
  {
    var gameSessionId = Guid.NewGuid(); // Fake session id.

    _mockRepo
        .Setup(repo => repo.GetCurrentRoundAsync(gameSessionId, It.IsAny<CancellationToken>())) // this method says, when called with the fake game session id generated above...
        .ReturnsAsync(3); // ...return round 3.

    var result = await _mockRepo.Object.GetCurrentRoundAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result); // Verify the result is not null.
    Assert.Equal(3, result); // Verify the round value.
  }

  [Fact]
  public async Task GetCurrentRoundAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo 
        .Setup(repo => repo.GetCurrentRoundAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())) // this method says, for any game session id given...
        .ReturnsAsync((int?)null); // ...return null.

    var result = await _mockRepo.Object.GetCurrentRoundAsync(Guid.NewGuid(), CancellationToken.None);

    Assert.Null(result); // Verify the result is null.
  }   
}