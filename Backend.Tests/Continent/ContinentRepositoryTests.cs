namespace Backend.Tests;

using System.Globalization;
using Moq;
using TddGame;

public class ContinentRepositoryTests
{
    private readonly Mock<IContinentRepository> _mockRepo;
    public ContinentRepositoryTests()
    {
        _mockRepo = new Mock<IContinentRepository>();
    }

    //Check that a continent is returned
    [Fact]
    public async Task GetContinentsAsync_ReturnsContinent()
    {
        var continents = new List<ContinentDto>
        {
            new ContinentDto(1, "string", 1)
        };
        _mockRepo.Setup(repo => repo.GetContinentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(continents);

        var results = await _mockRepo.Object.GetContinentsAsync(CancellationToken.None);

        Assert.NotNull(results);
        Assert.Single(results);
    }

    //Check that a continent is return when it exists, and that nothing returns when the request is invalid
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public async Task GetContinentByIdAsync_ReturnsCorrectResponse(int testId, bool isValidRequest)
    {
        ContinentDto dto = new ContinentDto(1, "Test", 1);

        _mockRepo.Setup(repo => repo.GetContinentByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var results = await _mockRepo.Object.GetContinentByIdAsync(testId, CancellationToken.None);

        if (isValidRequest)
        {
            Assert.NotNull(results);
            Assert.Equal("Test", results.name);
        }
        else { Assert.Null(results); }
    }

    //Checks that CreateContinent passes, and returns the created continent
    [Fact]
    public async Task CreateContinentAsync_ReturnsNewContinent()
    {
        ContinentDto dto = new ContinentDto(1, "test", 1);

        _mockRepo.Setup(repo => repo.CreateContinentAsync("test", 1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var results = await _mockRepo.Object.CreateContinentAsync("test", 1, CancellationToken.None);

        Assert.NotNull(results);
        Assert.Equal("test", results.name);
        Assert.Equal(1, results.bonusConst);
    }
}