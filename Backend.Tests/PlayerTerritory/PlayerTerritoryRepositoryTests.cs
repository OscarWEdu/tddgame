using Moq;
using TddGame;
using Xunit;

namespace Backend.Tests.PlayerTerritoryTesting;

public class PlayerTerritoryRepositoryTests
{
    private readonly Mock<IPlayerTerritoryRepository> _mockRepo;

    public PlayerTerritoryRepositoryTests()
    {
        _mockRepo = new Mock<IPlayerTerritoryRepository>();
    }

    //Check that a playerTerritory is returned
    [Fact]
    public async Task GetPlayerTerritoriesAsync_ReturnsPlayerTerritory()
    {
        var playerTerritories = new List<PlayerTerritoryDto>
        {
            new PlayerTerritoryDto(1, 0, false, 0, 0)
        };
        _mockRepo.Setup(repo => repo.GetPlayerTerritoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(playerTerritories);

        var results = await _mockRepo.Object.GetPlayerTerritoriesAsync(CancellationToken.None);

        Assert.NotNull(results);
        Assert.Single(results);
    }

    //Check that a playerTerritory is return when it exists, and that nothing returns when the request is invalid
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public async Task GetPlayerTerritoryByIdAsync_ReturnsCorrectResponse(int testId, bool isValidRequest)
    {
        PlayerTerritoryDto dto = new PlayerTerritoryDto(1, 0, false, 1, 1);

        _mockRepo.Setup(repo => repo.GetPlayerTerritoryByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var results = await _mockRepo.Object.GetPlayerTerritoryByIdAsync(testId, CancellationToken.None);

        if (isValidRequest)
        {
            Assert.NotNull(results);
            Assert.Equal(1, results.TerritoryId);
        }
        else { Assert.Null(results); }
    }
}