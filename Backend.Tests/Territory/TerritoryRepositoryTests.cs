namespace Backend.Tests;

using Moq;
using TddGame;

public class TerritoryRepositoryTests
{
    private readonly Mock<ITerritoryRepository> _mockRepo;
    public TerritoryRepositoryTests()
    {
        _mockRepo = new Mock<ITerritoryRepository>();
    }

    //Check that a territory is returned
    [Fact]
    public async Task GetTerritoriesAsync_ReturnsContinent()
    {
        var territories = new List<TerritoryDto>
        {
            new TerritoryDto(1, "string", -1, -1, -1, -1, 1)
        };
        _mockRepo.Setup(repo => repo.GetTerritoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(territories);

        var results = await _mockRepo.Object.GetTerritoriesAsync(CancellationToken.None);

        Assert.NotNull(results);
        Assert.Single(results);
    }

    //Check that a territory is return when it exists, and that nothing returns when the request is invalid
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public async Task GetTerritoryByIdAsync_ReturnsCorrectResponse(int testId, bool isValidRequest)
    {
        TerritoryDto dto = new TerritoryDto(1, "Test", -1, -1, -1, -1, 1);

        _mockRepo.Setup(repo => repo.GetTerritoryByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var results = await _mockRepo.Object.GetTerritoryByIdAsync(testId, CancellationToken.None);

        if (isValidRequest)
        {
            Assert.NotNull(results);
            Assert.Equal("Test", results.Name);
        }
        else { Assert.Null(results); }
    }

    //Checks that CreateTerritory passes, and returns the created territory (does not verify that the continentId is valid)
    [Fact]
    public async Task CreateTerritoryAsync_ReturnsNewTerritory()
    {
        TerritoryDto dto = new TerritoryDto(1, "Test", -1, -1, -1, -1, 1);

        _mockRepo.Setup(repo => repo.CreateTerritoryAsync("Test", -1, -1, -1, -1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var results = await _mockRepo.Object.CreateTerritoryAsync("Test", -1, -1, -1, -1, 1, CancellationToken.None);

        Assert.NotNull(results);
        Assert.Equal("Test", results.Name);
        Assert.Equal(-1, results.NorthAdjacentId);
        Assert.Equal(-1, results.SouthAdjacentId);
        Assert.Equal(-1, results.WestAdjacentId);
        Assert.Equal(-1, results.EastAdjacentId);
        Assert.Equal(1, results.ContinentId);
    }
}