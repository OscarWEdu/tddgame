namespace Backend.Tests;

using Moq;
using TddGame;

public class GameSessionRepositoryTests
{
    private readonly Mock<IGameSessionsRepository> _mockRepo;

    public GameSessionRepositoryTests()
    {
        _mockRepo = new Mock<IGameSessionsRepository>();
    }

    [Fact]
    public async Task GetGameSessionsAsync_ReturnSession()
    {
        var sessions = new List<GameSessionDto>
        {
            new("550e8400-e29b-41d4-a716-446655440000", "Session One", GameSessionStatus.lobby)
        };
        _mockRepo.Setup(repo => repo.GetGameSessionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sessions);

        var results = await _mockRepo.Object.GetGameSessionsAsync(CancellationToken.None);


        Assert.NotNull(results);
        Assert.Single(results);
    }

    [Fact]
    public async Task GetGameSessionByIdAsync_ReturnSession_WhenFound()
    {
        var id = Guid.NewGuid();
        var dto = new GameSessionDto(id.ToString(), "Test", GameSessionStatus.lobby);
        _mockRepo.Setup(repo => repo.GetGameSessionByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((dto));

        var result = await _mockRepo.Object.GetGameSessionByIdAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetGameSessionByIdAsync_ReturnNull_WhenNotFound()
    {
        _mockRepo.Setup(repo => repo.GetGameSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((GameSessionDto?)null);

        var result = await _mockRepo.Object.GetGameSessionByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateGameSessionAsync_ReturnNewSession()
    {
        var dto = new GameSessionDto(Guid.NewGuid().ToString(), "New Test Game", GameSessionStatus.lobby);
        _mockRepo.Setup(repo => repo.CreateGameSessionAsync("New Test Game", It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _mockRepo.Object.CreateGameSessionAsync("New Test Game", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New Test Game", result.Name);
        Assert.NotEmpty(result.Id);
        Assert.Equal("lobby", result.Status.ToString());
    }

    [Fact]
    public async Task UpdateGameSessionStatusAsync_ReturnsTrue_WhenFound()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(repo => repo.UpdateGameSessionStatusAsync(id, "started", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _mockRepo.Object.UpdateGameSessionStatusAsync(id, "started", CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateGameSessionStatusAsync_ReturnsFalse_WhenNotFound()
    {
        _mockRepo.Setup(repo => repo.UpdateGameSessionStatusAsync(It.IsAny<Guid>(), "started", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _mockRepo.Object.UpdateGameSessionStatusAsync(Guid.NewGuid(), "started", CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteGameSessionAsync_ReturnsTrue_WhenFound()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(repo => repo.DeleteGameSessionAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _mockRepo.Object.DeleteGameSessionAsync(id, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteGameSessionAsync_ReturnsFalse_WhenNotFound()
    {
        _mockRepo.Setup(repo => repo.DeleteGameSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _mockRepo.Object.DeleteGameSessionAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result);
    }
}