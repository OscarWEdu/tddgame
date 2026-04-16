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
}