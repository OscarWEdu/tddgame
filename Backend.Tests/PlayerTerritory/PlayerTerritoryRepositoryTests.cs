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

    //Check that a playerTerritory is returned
    [Fact]
    public async Task GetPlayerPlayerTerritoriesAsync_ReturnsPlayerTerritory()
    {
        var playerTerritories = new List<PlayerTerritoryDto>
        {
            new PlayerTerritoryDto(1, 0, false, 1, 1)
        };
        _mockRepo.Setup(repo => repo.GetPlayerPlayerTerritoriesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(playerTerritories);

        var results = await _mockRepo.Object.GetPlayerPlayerTerritoriesAsync(1, CancellationToken.None);

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

    //Checks that CreatePlayerTerritory passes, and returns the created PlayerTerritory
    [Fact]
    public async Task CreatePlayerTerritoryAsync_ReturnsNewPlayerTerritory()
    {
        PlayerTerritoryDto dto = new PlayerTerritoryDto(1, 0, false, 1, 1);

        _mockRepo.Setup(repo => repo.CreatePlayerTerritoryAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var results = await _mockRepo.Object.CreatePlayerTerritoryAsync(1, 1, CancellationToken.None);

        Assert.NotNull(results);
        Assert.Equal(0, results.TroopNum);
        Assert.False(results.HasCity);
        Assert.Equal(1, results.PlayerId);
        Assert.Equal(1, results.TerritoryId);
    }

    //Check that true is returned when an entry exists and can be deleted, and false if not
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public async Task DeletePlayerTerritoryAsync_ReturnsCorrectResponse(int testId, bool isValidRequest)
    {
    _mockRepo
        .Setup(repo => repo.DeletePlayerTerritoryAsync(testId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(isValidRequest);

    var result = await _mockRepo.Object.DeletePlayerTerritoryAsync(testId, CancellationToken.None);

    Assert.Equal(isValidRequest, result);
    }

    //Check that true is returned when an entry exists and can be deleted, and false if not
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public async Task UpdatePlayerTerritoryTroopsAsync_ReturnsCorrectResponse(int testId, bool isValidRequest)
    {
    _mockRepo
        .Setup(repo => repo.UpdatePlayerTerritoryTroopsAsync(testId, 5, It.IsAny<CancellationToken>()))
        .ReturnsAsync(isValidRequest);

    var result = await _mockRepo.Object.UpdatePlayerTerritoryTroopsAsync(testId, 5, CancellationToken.None);

    Assert.Equal(isValidRequest, result);
    }

    //Check that true is returned when an entry exists and can be deleted, and false if not
   [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public async Task UpdatePlayerTerritoryCityAsync_ReturnsCorrectResponse(int testId, bool isValidRequest)
    {
    _mockRepo
        .Setup(repo => repo.UpdatePlayerTerritoryCityAsync(testId, true, It.IsAny<CancellationToken>()))
        .ReturnsAsync(isValidRequest);

    var result = await _mockRepo.Object.UpdatePlayerTerritoryCityAsync(testId, true, CancellationToken.None);

    Assert.Equal(isValidRequest, result);
    }
}