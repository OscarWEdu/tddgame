namespace Backend.Tests;

using Moq;
using TddGame;

public class TypingChallengesRepositoryTests
{
    private readonly Mock<ITypingChallengesRepository> _mockRepo;

    public TypingChallengesRepositoryTests()
    {
        _mockRepo = new Mock<ITypingChallengesRepository>();
    }

    [Fact]
    public async Task GetTypingChallengeByIdAsync_ReturnTypingChallenge_WhenFound()
    {
        var dto = new TypingChallengeDto(
            Id: 1,
            Speed: 0,
            Mistakes: 0,
            PromptText: "To the moon!",
            BattleId: 10
        );

        _mockRepo
            .Setup(repo => repo.GetTypingChallengeByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _mockRepo.Object.GetTypingChallengeByIdAsync(1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("To the moon!", result.PromptText);
        Assert.Equal(10, result.BattleId);
    }

    [Fact]
    public async Task GetTypingChallengeByIdAsync_ReturnNull_WhenNotFound()
    {
        _mockRepo
            .Setup(repo => repo.GetTypingChallengeByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TypingChallengeDto?)null);

        var result = await _mockRepo.Object.GetTypingChallengeByIdAsync(999, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTypingChallengeByBattleIdAsync_ReturnTypingChallenge_WhenFound()
    {
        var dto = new TypingChallengeDto(
            Id: 2,
            Speed: 0,
            Mistakes: 0,
            PromptText: "Attack now",
            BattleId: 20
        );

        _mockRepo
            .Setup(repo => repo.GetTypingChallengeByBattleIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _mockRepo.Object.GetTypingChallengeByBattleIdAsync(20, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Attack now", result.PromptText);
        Assert.Equal(20, result.BattleId);
    }

    [Fact]
    public async Task GetTypingChallengeByBattleIdAsync_ReturnNull_WhenNotFound()
    {
        _mockRepo
            .Setup(repo => repo.GetTypingChallengeByBattleIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TypingChallengeDto?)null);

        var result = await _mockRepo.Object.GetTypingChallengeByBattleIdAsync(999, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTypingChallengeAsync_ReturnCreatedTypingChallenge()
    {
        var createDto = new CreateTypingChallengeDto(
            BattleId: 15,
            PromptText: "Write this sentence"
        );

        var createdDto = new TypingChallengeDto(
            Id: 3,
            Speed: 0,
            Mistakes: 0,
            PromptText: "Write this sentence",
            BattleId: 15
        );

        _mockRepo
            .Setup(repo => repo.CreateTypingChallengeAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        var result = await _mockRepo.Object.CreateTypingChallengeAsync(createDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal(createDto.PromptText, result.PromptText);
        Assert.Equal(createDto.BattleId, result.BattleId);
        Assert.Equal(0, result.Speed);
        Assert.Equal(0, result.Mistakes);
    }
}