using Moq;
using TddGame;
using Xunit;

namespace Backend.Tests.PlayerTesting;

public class PlayerRepositoryTests
{
    private readonly Mock<IPlayersRepository> _mockRepo;

    public PlayerRepositoryTests()
    {
        _mockRepo = new Mock<IPlayersRepository>();
    }

    [Fact]
    public async Task GetPlayers_ReturnList()
    {
        var players = new List<PlayerDto>
        {
            new PlayerDto(1, "Mario", "Red", 1, 0, false, "1", 1),
            new PlayerDto(2, "Wario", "Yellow", 2, 0, false, "1", 1)
        };

        _mockRepo.Setup(repo => repo.GetPlayersByGameSessionAsync("1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(players);

        var result = await _mockRepo.Object.GetPlayersByGameSessionAsync("1", CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

    }

    [Fact]
    public async Task GetPlayer_ReturnsEmpty()
    {
        _mockRepo.Setup(repo => repo.GetPlayersByGameSessionAsync("99", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<PlayerDto>());

        var result = await _mockRepo.Object.GetPlayersByGameSessionAsync("99", CancellationToken.None);
        Assert.Empty(result);
    }   

    [Fact]
    public async Task GetPlayerById_ReturnsPlayer()
    {
        var player = new PlayerDto(1, "Mario", "Red", 1, 0, false, "1", 1);

        _mockRepo.Setup(repo => repo.GetPlayerByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(player);

        var result = await _mockRepo.Object.GetPlayerByIdAsync(1, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal("Mario", result.Name);
        Assert.Equal("Red", result.Colour);
    }

    [Fact]
    public async Task GetPlayerById_ReturnsNull()
    {
        _mockRepo.Setup(repo => repo.GetPlayerByIdAsync(99, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PlayerDto?)null);

        var result = await _mockRepo.Object.GetPlayerByIdAsync(99, CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task AddPlayer_ReturnsCreatedPlayer()
    {
        var CreatePlayerDto = new CreatePlayerDto("Luigi", "Green", 2, 1);
        var createdPlayer = new PlayerDto(2, "Luigi", "Green", 2, 0, false, "1", 1);
        _mockRepo.Setup(repo => repo.AddPlayerToGameAsync("1", CreatePlayerDto, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(createdPlayer);

        var result = await _mockRepo.Object.AddPlayerToGameAsync("1", CreatePlayerDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Luigi", result.Name);
        Assert.Equal("Green", result.Colour);
        Assert.Equal(0, result.NumGold);
        Assert.False(result.IsDead);
    }
}