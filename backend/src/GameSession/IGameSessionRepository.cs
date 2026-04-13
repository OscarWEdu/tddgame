
namespace TddGame;

public interface IGameSessionsRepository
{
    Task<IEnumerable<GameSessionDto>> GetGameSessionsAsync(CancellationToken ct);
}