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
            new PlayerDto(1, "Mario", "Red", 1, 0, false, true, "1", 1),
            new PlayerDto(2, "Wario", "Yellow", 2, 0, false, false, "1", 1)
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
        var player = new PlayerDto(1, "Mario", "Red", 1, 0, false, true, "1", 1);

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
        var createdPlayer = new PlayerDto(2, "Luigi", "Green", 2, 0, false, false, "1", 1);
        _mockRepo.Setup(repo => repo.AddPlayerToGameAsync("1", CreatePlayerDto, false, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(createdPlayer);

        var result = await _mockRepo.Object.AddPlayerToGameAsync("1", CreatePlayerDto, false, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Luigi", result.Name);
        Assert.Equal("Green", result.Colour);
        Assert.Equal(0, result.NumGold);
        Assert.False(result.IsDead);
    }

    [Fact]
    public async Task UpdatePlayer_ReturnsUpdatedPlayer()
    {
        var newState = new PlayerStateDto(10, true);
        var updatedPlayer = new PlayerDto(1, "Mario", "Red", 1, 10, true, true, "1", 1);
        _mockRepo.Setup(repo => repo.UpdatePlayerAsync(1, newState, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(updatedPlayer);

        var result = await _mockRepo.Object.UpdatePlayerAsync(1, newState, CancellationToken.None);

        Assert.Equal(10, result.NumGold);
        Assert.True(result.IsDead);
    }

    [Fact]
    public async Task DeletePlayer_ReturnsTrue()
    {
        _mockRepo.Setup(repo => repo.DeletePlayerAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        
        var result = await _mockRepo.Object.DeletePlayerAsync(1, CancellationToken.None);

        Assert.True(result);
    }

      [Fact]
    public async Task DeletePlayer_ReturnsFalse()
    {
        _mockRepo.Setup(repo => repo.DeletePlayerAsync(99, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
        
        var result = await _mockRepo.Object.DeletePlayerAsync(99, CancellationToken.None);

        Assert.False(result);
    }


}