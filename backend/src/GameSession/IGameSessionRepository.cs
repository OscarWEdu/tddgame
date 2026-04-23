
namespace TddGame;

public interface IGameSessionsRepository
{
    Task<IEnumerable<GameSessionDto>> GetGameSessionsAsync(CancellationToken ct);
    Task<GameSessionDto?> GetGameSessionByIdAsync(Guid id, CancellationToken ct);
    Task<GameSessionDto> CreateGameSessionAsync(string name, int maxPlayers, CancellationToken ct);
    Task<bool> UpdateGameSessionStatusAsync(Guid id, string status, CancellationToken ct);
    Task<bool> DeleteGameSessionAsync(Guid id, CancellationToken ct);
}