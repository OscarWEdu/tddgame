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
    public async Task GetContinentAsync_ReturnsContinent()
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
}