// - submit attacker and defender typing data
// - calculate attacker and defender score
// - defender wins ties
// - calculate troop loss (1 or 2)
// - save the final resolved result row

namespace Backend.Tests.ResultTesting;

using Moq;
using TddGame;
using Xunit;

public class ResultRepositoryTests
{
    private readonly Mock<IResultsRepository> _mockRepo;

    public ResultRepositoryTests()
    {
        _mockRepo = new Mock<IResultsRepository>();
    }

  // Check that a result is returned when it exists.
  [Fact]
  public async Task GetResultByIdAsync_ReturnResult_WhenFound()
  {
    var dto = new ResultDto(
        Id: 1,
        BattleId: 10,
        Winner: BattleWinner.attacker,
        AttackerScore: 60,
        DefenderScore: 35,
        AttackerMistakes: 0,
        DefenderMistakes: 3,
        AttackerCompleted: true,
        DefenderCompleted: true,
        AttackerTroopLoss: 0,
        DefenderTroopLoss: 2
    );

    _mockRepo.Setup(repo => repo.GetResultByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

    var result = await _mockRepo.Object.GetResultByIdAsync(1, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(BattleWinner.attacker, result.Winner);
    Assert.Equal(60, result.AttackerScore);
  }

  // Check that null is returned when the requested result does not exist.
  [Fact]
  public async Task GetResultByIdAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo.Setup(repo => repo.GetResultByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((ResultDto?)null);

    var result = await _mockRepo.Object.GetResultByIdAsync(999, CancellationToken.None);

    Assert.Null(result);
  }

  // Check that the result connected to one battle is returned when it exists.
  [Fact]
  public async Task GetResultByBattleIdAsync_ReturnResult_WhenFound()
  {
    var dto = new ResultDto(
        Id: 1,
        BattleId: 10,
        Winner: BattleWinner.defender,
        AttackerScore: 18,
        DefenderScore: 25,
        AttackerMistakes: 4,
        DefenderMistakes: 2,
        AttackerCompleted: true,
        DefenderCompleted: true,
        AttackerTroopLoss: 1,
        DefenderTroopLoss: 0
    );

    _mockRepo.Setup(repo => repo.GetResultByBattleIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

    var result = await _mockRepo.Object.GetResultByBattleIdAsync(10, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(10, result.BattleId);
    Assert.Equal(BattleWinner.defender, result.Winner);
  }

  // Check that null is returned when no result exists for the given battle.
  [Fact]
  public async Task GetResultByBattleIdAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo.Setup(repo => repo.GetResultByBattleIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((ResultDto?)null);

    var result = await _mockRepo.Object.GetResultByBattleIdAsync(999, CancellationToken.None);

    Assert.Null(result);
  }

  // Check that a new resolved result is created and returned with the expected values.
  [Fact]
  public async Task CreateResultAsync_ReturnCreatedResult()
  {
    var createDto = new CreateResultDto(
        BattleId: 10,
        Winner: BattleWinner.attacker,
        AttackerScore: 55,
        DefenderScore: 20,
        AttackerMistakes: 1,
        DefenderMistakes: 5,
        AttackerCompleted: true,
        DefenderCompleted: true,
        AttackerTroopLoss: 0,
        DefenderTroopLoss: 2
    );

    var createdDto = new ResultDto(
        Id: 1,
        BattleId: 10,
        Winner: BattleWinner.attacker,
        AttackerScore: 55,
        DefenderScore: 20,
        AttackerMistakes: 1,
        DefenderMistakes: 5,
        AttackerCompleted: true,
        DefenderCompleted: true,
        AttackerTroopLoss: 0,
        DefenderTroopLoss: 2
    );

    _mockRepo.Setup(repo => repo.CreateResultAsync(createDto, It.IsAny<CancellationToken>())).ReturnsAsync(createdDto);

    var result = await _mockRepo.Object.CreateResultAsync(createDto, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(BattleWinner.attacker, result.Winner);
    Assert.Equal(0, result.AttackerTroopLoss);
    Assert.Equal(2, result.DefenderTroopLoss);
  }

  // Check that battle data used during result validation is returned when the battle exists.
  [Fact]
  public async Task GetBattleForResultAsync_ReturnBattle_WhenFound()
  {
    var dto = new ResultBattleValidationDto(
        Id: 10,
        AttackingTroops: 3,
        DefenderTroops: 2
    );

    _mockRepo.Setup(repo => repo.GetBattleForResultAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

    var result = await _mockRepo.Object.GetBattleForResultAsync(10, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(3, result.AttackingTroops);
    Assert.Equal(2, result.DefenderTroops);
  }

  // Check that null is returned when the battle used for result validation does not exist.
  [Fact]
  public async Task GetBattleForResultAsync_ReturnNull_WhenNotFound()
  {
    _mockRepo.Setup(repo => repo.GetBattleForResultAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((ResultBattleValidationDto?)null);

    var result = await _mockRepo.Object.GetBattleForResultAsync(999, CancellationToken.None);

    Assert.Null(result);
  }

  // Check that true is returned when a typing challenge exists for the battle.
  [Fact]
  public async Task TypingChallengeExistsForBattleAsync_ReturnTrue_WhenFound()
  {
    _mockRepo.Setup(repo => repo.TypingChallengeExistsForBattleAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var result = await _mockRepo.Object.TypingChallengeExistsForBattleAsync(10, CancellationToken.None);

    Assert.True(result);
  }

  // Check that false is returned when no typing challenge exists for the battle.
  [Fact]
  public async Task TypingChallengeExistsForBattleAsync_ReturnFalse_WhenNotFound()
  {
    _mockRepo.Setup(repo => repo.TypingChallengeExistsForBattleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var result = await _mockRepo.Object.TypingChallengeExistsForBattleAsync(999, CancellationToken.None);

    Assert.False(result);
  }
}