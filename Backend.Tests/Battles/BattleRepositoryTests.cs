namespace Backend.Tests;

using Moq;
using TddGame;

public class BattlesRepositoryTests
{
    // Create a private field to hold a mock version of IBattlesRepository.
    private readonly Mock<IBattlesRepository> _mockRepo;

    public BattlesRepositoryTests()
    {
        // Initializes the mock repository.
        _mockRepo = new Mock<IBattlesRepository>();
    }

  [Fact]
  // Verify that a battle is correctly returned when it exists in the database
  public async Task GetBattleByIdAsync_ReturnBattle_WhenFound()
  {
    var battleId = 1;

    var dto = new BattleDto(
        Id: 1,
        AttackingTroops: 3,
        AttackerTerritoryId: 10,
        DefenderTerritoryId: 20
    );

    _mockRepo
        .Setup(repo => repo.GetBattleByIdAsync(battleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(dto);

    var result = await _mockRepo.Object.GetBattleByIdAsync(battleId, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
    Assert.Equal(3, result.AttackingTroops);
    Assert.Equal(10, result.AttackerTerritoryId);
    Assert.Equal(20, result.DefenderTerritoryId);
  }

  [Fact]
  // Verify that null is returned when the requested battle does not exist
  public async Task GetBattleByIdAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo
        .Setup(repo => repo.GetBattleByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((BattleDto?)null);

    var result = await _mockRepo.Object.GetBattleByIdAsync(999, CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  // Verify that all battles for a given game session are retrieved correctly
  public async Task GetBattlesByGameSessionIdAsync_ReturnBattlesList()
  {
    var gameSessionId = Guid.NewGuid();

    var battles = new List<BattleDto>
        {
            new BattleDto(1, 2, 10, 20),
            new BattleDto(2, 3, 11, 21),
            new BattleDto(3, 1, 12, 22)
        };

    _mockRepo
        .Setup(repo => repo.GetBattlesByGameSessionIdAsync(gameSessionId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(battles);

    var result = await _mockRepo.Object.GetBattlesByGameSessionIdAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(3, result.Count());
    Assert.Contains(result, b => b.AttackerTerritoryId == 10);
  }

  [Fact]
  // Verify that a new battle is successfully created and returned with correct data.
  public async Task CreateBattleAsync_ReturnCreatedBattle()
  {
    var createDto = new CreateBattleDto(
        AttackingTroops: 3,
        AttackerTerritoryId: 10,
        DefenderTerritoryId: 20
    );

    var createdDto = new BattleDto(
        Id: 5,
        AttackingTroops: 3,
        AttackerTerritoryId: 10,
        DefenderTerritoryId: 20
    );

    _mockRepo
        .Setup(repo => repo.CreateBattleAsync(createDto, It.IsAny<CancellationToken>()))
        .ReturnsAsync(createdDto);

    var result = await _mockRepo.Object.CreateBattleAsync(createDto, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(createdDto.Id, result.Id);
    Assert.Equal(createDto.AttackingTroops, result.AttackingTroops);
    Assert.Equal(createDto.AttackerTerritoryId, result.AttackerTerritoryId);
    Assert.Equal(createDto.DefenderTerritoryId, result.DefenderTerritoryId);
  }

  [Fact]
  // Verify that the current active turn is returned when it exists for the game session
  public async Task GetCurrentTurnByGameSessionIdAsync_ReturnTurn_WhenFound()
  {
    var gameSessionId = Guid.NewGuid();

    var turn = new TurnDto(
        Id: 7,
        Round: 2,
        Phase: TurnPhase.attack,
      Status: TurnStatus.active,
        CreateAt: DateTime.Today,
        GameSessionId: gameSessionId.ToString(),
        PlayerId: 15
    );

    _mockRepo
        .Setup(repo => repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(turn);

    var result = await _mockRepo.Object.GetCurrentTurnByGameSessionIdAsync(gameSessionId, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(TurnPhase.attack, result.Phase);
    Assert.Equal(15, result.PlayerId);
  }


  [Fact]
  // Verify that null is returned when no active turn exists for the game session
  public async Task GetCurrentTurnByGameSessionIdAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo
        .Setup(repo => repo.GetCurrentTurnByGameSessionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((TurnDto?)null);

    var result = await _mockRepo.Object.GetCurrentTurnByGameSessionIdAsync(Guid.NewGuid(), CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  // Verify that a valid territory is returned when it exists for battle validation
  public async Task GetBattleTerritoryValidationAsync_ReturnTerritory_WhenFound()
  {
    var validationDto = new BattleTerritoryValidationDto(
        PlayerTerritoryId: 10,
        PlayerId: 3,
        TroopNum: 5,
        TerritoryId: 100
    );

    _mockRepo
        .Setup(repo => repo.GetBattleTerritoryValidationAsync(10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(validationDto);

    var result = await _mockRepo.Object.GetBattleTerritoryValidationAsync(10, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(3, result.PlayerId);
    Assert.Equal(5, result.TroopNum);
    Assert.Equal(100, result.TerritoryId);
  }

  [Fact]
  // Verify that null is returned when the territory used for validation does not exist
  public async Task GetBattleTerritoryValidationAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo
        .Setup(repo => repo.GetBattleTerritoryValidationAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((BattleTerritoryValidationDto?)null);

    var result = await _mockRepo.Object.GetBattleTerritoryValidationAsync(999, CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  // Verify that true is returned when two territories are adjacent
  public async Task AreTerritoriesAdjacentAsync_ReturnTrue_WhenAdjacent()
  {
    _mockRepo
        .Setup(repo => repo.AreTerritoriesAdjacentAsync(100, 200, It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    var result = await _mockRepo.Object.AreTerritoriesAdjacentAsync(100, 200, CancellationToken.None);

    Assert.True(result);
  }

  [Fact]
  // Verify that false is returned when two territories are not adjacent
  public async Task AreTerritoriesAdjacentAsync_ReturnFalse_WhenNotAdjacent()
  {
    _mockRepo
        .Setup(repo => repo.AreTerritoriesAdjacentAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    var result = await _mockRepo.Object.AreTerritoriesAdjacentAsync(100, 999, CancellationToken.None);

    Assert.False(result);
  }
}