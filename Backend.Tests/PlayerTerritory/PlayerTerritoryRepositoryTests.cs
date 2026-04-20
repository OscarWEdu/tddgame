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
}