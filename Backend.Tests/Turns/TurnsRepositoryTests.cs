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
    var gameSessionId = "550e8400-e29b-41d4-a716-446655440000";

    // Create a fake turn object that the mock repository should return.
    var dto = new TurnDto(
        Id: 1, // Fake turn id.
        Round: 1, // Fake round number.
        Phase: "build", // Fake phase.
        CreateAt: DateTime.Today, // Fake creation date.
        GameSessionId: gameSessionId, // Matches the fake game session id.
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
    Assert.NotNull(result); // Verifies that the result is not null.
    Assert.Equal(1, result.Id); // Verifies that the turn id is correct.
    Assert.Equal(1, result.Round); // Verifies that the round is correct.
    Assert.Equal("build", result.Phase); // Verifies that the phase is correct.
    Assert.Equal(gameSessionId, result.GameSessionId); // Verifies that the game session id is correct.
    Assert.Equal(10, result.PlayerId); // Verifies that the player id is correct.
  }

  [Fact]
  // Test name describing expected behavior.
  public async Task GetCurrentTurnByGameSessionIdAsync_ReturnNull_WhenNotFound()
  {
    // Start the mock setup
    _mockRepo
        .Setup(repo => repo.GetCurrentTurnByGameSessionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())) // for any session id...
        .ReturnsAsync((TurnDto?)null); // ...return null.

    var result = await _mockRepo.Object.GetCurrentTurnByGameSessionIdAsync("unknown-session", CancellationToken.None);

    // Verify that the result is null.
    Assert.Null(result);
  }

  [Fact]
  public async Task CreateTurnAsync_ReturnCreatedTurn()
  {
    // Create the input object that would normally be sent to the repository.
    var createDto = new CreateTurnDto(
        Round: 1, // The round to create.
        Phase: "build", // The phase to create.
        GameSessionId: "550e8400-e29b-41d4-a716-446655440000", // The game session id for the new turn.
        PlayerId: 10 // The player id for the new turn.
    );

    // Create the fake repository return value.
    var createdDto = new TurnDto(
        Id: 1, // Fake generated turn id.
        Round: 1, // Same round.
        Phase: "build", // Same phase.
        CreateAt: DateTime.Today, // Fake created date.
        GameSessionId: createDto.GameSessionId, // Same session id.
        PlayerId: createDto.PlayerId // Same player id.
    );

    // Start mock setup.
    _mockRepo
        .Setup(repo => repo.CreateTurnAsync(createDto, It.IsAny<CancellationToken>())) //when CreateTurnAsync is called with this DTO...
        .ReturnsAsync(createdDto); // ...return this fake created TurnDto.

    // Call the mocked method.
    var result = await _mockRepo.Object.CreateTurnAsync(createDto, CancellationToken.None); 

    Assert.NotNull(result); // Verifies that the returned object is not null.
    Assert.Equal(1, result.Id); // Verifies the id.
    Assert.Equal(createDto.Round, result.Round); // Verifies the round.
    Assert.Equal(createDto.Phase, result.Phase); // Verifies the phase.
    Assert.Equal(createDto.GameSessionId, result.GameSessionId); // Verifies the session id.
    Assert.Equal(createDto.PlayerId, result.PlayerId); // Verifies the player id.
  }

  [Fact]
  public async Task GetFirstPlayerIdByGameSessionIdAsync_ReturnFirstPlayerId_WhenFound()
  {
    // Fake game session id.
    var gameSessionId = "550e8400-e29b-41d4-a716-446655440000";

    _mockRepo
        .Setup(repo => repo.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, It.IsAny<CancellationToken>())) // when this method is called with the session id above...
        .ReturnsAsync(5); // ...return player id 5.

    var result = await _mockRepo.Object.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result); // Verify the result is not null.
    Assert.Equal(5, result); // Verify the correct player id is returned.
  }

  [Fact]
  public async Task GetFirstPlayerIdByGameSessionIdAsync_ReturnNull_WhenNotFound()
  {
    // Start mock setup.
    _mockRepo
        .Setup(repo => repo.GetFirstPlayerIdByGameSessionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())) // this method says, for any session id...
        .ReturnsAsync((int?)null); // ...return null.

    var result = await _mockRepo.Object.GetFirstPlayerIdByGameSessionIdAsync("unknown-session", CancellationToken.None); // Calls the mocked method.

    Assert.Null(result); // Verifies that the result is null.
  }

  [Fact]
  public async Task GetNextPlayerIdAsync_ReturnNextPlayerId_WhenFound()
  {
    var gameSessionId = "550e8400-e29b-41d4-a716-446655440000"; // Fake session id.
    var currentPlayerId = 10; // Fake current player id.

    // Starts mock setup.
    _mockRepo
        .Setup(repo => repo.GetNextPlayerIdAsync(gameSessionId, currentPlayerId, It.IsAny<CancellationToken>())) // when this exact call happens...
        .ReturnsAsync(11); // ...return player id 11.

    var result = await _mockRepo.Object.GetNextPlayerIdAsync(gameSessionId, currentPlayerId, CancellationToken.None); // Calls the mocked method.

    Assert.NotNull(result); // Verifies the result is not null.
    Assert.Equal(11, result); // Verifies the next player id.
  }

  [Fact] // Marks this method as a test.
  public async Task GetNextPlayerIdAsync_ReturnNull_WhenNotFound() // Test name describing expected behavior.
  {
    _mockRepo // Starts mock setup.
        .Setup(repo => repo.GetNextPlayerIdAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())) // this method says, for any arguments...
        .ReturnsAsync((int?)null); // ...return null.

    var result = await _mockRepo.Object.GetNextPlayerIdAsync("unknown-session", 99, CancellationToken.None);

    Assert.Null(result); // Verify the result is null.
  }

  [Fact]
  public async Task GetCurrentRoundAsync_ReturnRound_WhenFound()
  {
    var gameSessionId = "550e8400-e29b-41d4-a716-446655440000"; // Fake session id.

    _mockRepo
        .Setup(repo => repo.GetCurrentRoundAsync(gameSessionId, It.IsAny<CancellationToken>())) // this method says, when called with the game session id above...
        .ReturnsAsync(3); // ...return round 3.

    var result = await _mockRepo.Object.GetCurrentRoundAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result); // Verify the result is not null.
    Assert.Equal(3, result); // Verify the round number.
  }

  [Fact]
  public async Task GetCurrentRoundAsync_ReturnNull_WhenNotFound()
  {
    // Starts mock setup
    _mockRepo 
        .Setup(repo => repo.GetCurrentRoundAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())) // this method says, for any game session id given...
        .ReturnsAsync((int?)null); // ...return null.

    var result = await _mockRepo.Object.GetCurrentRoundAsync("unknown-session", CancellationToken.None);

    Assert.Null(result); // Verify the result is null.
  }   
}