
using Moq;
using TddGame;
using Xunit;

namespace Backend.Tests.PlayerTerritoryTesting;


public async Task DeletePlayerTerritory_ReturnsTrue_WhenDeleted()
{
    var mockRepo = new Mock<IPlayerTerritoryRepository>();

    mockRepo
        .Setup(r => r.DeletePlayerTerritoryAsync(1, It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    var result = await _mockRepo.Object.DeletePlayerTerritoryAsync(1, CancellationToken.None);

    Assert.True(result);
}