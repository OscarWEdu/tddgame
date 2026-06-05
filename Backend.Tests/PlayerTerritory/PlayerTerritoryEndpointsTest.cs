using Moq;
using TddGame;
using Xunit;

namespace Backend.Tests.PlayerTerritoryTesting;

public class PlayerTerritoryEndpointsTests
{
    [Fact]
    public async Task DeletePlayerTerritory_ReturnsTrue_WhenDeleted()
    {
        
        var _mockRepo = new Mock<IPlayerTerritoryRepository>();

        _mockRepo
            .Setup(r => r.DeletePlayerTerritoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepo.Object.DeletePlayerTerritoryAsync(1, CancellationToken.None);

       
        Assert.True(result);
    }
}