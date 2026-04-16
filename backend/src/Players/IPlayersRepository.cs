// Defines the database operations for players.
// Example methods:
// GetPlayersByGameSessionAsync(gameSessionId)
// GetPlayerByIdAsync(playerId)
// AddPlayerToGameAsync(...)
// UpdatePlayerAsync(...)
// SetPlayerMissionAsync(...)
// SetPlayerColourAsync(...)
// Makes the player data access layer testable and replaceable.
namespace TddGame;
public interface IPlayersRepository
{
    Task<IEnumerable<PlayerDto>> GetPlayersByGameSessionAsync(string gameSessionId, CancellationToken ct);
    Task<PlayerDto> GetPlayerByIdAsync(int playerId, CancellationToken ct);
    Task<PlayerDto> AddPlayerToGameAsync(string gameSessionId, CreatePlayerDto createPlayerDto, CancellationToken ct);
    Task<PlayerDto> UpdatePlayerAsync(int playerId, PlayerStateDto state, CancellationToken ct);
    Task<bool> DeletePlayerAsync(int playerId, CancellationToken ct);
    Task<PlayerDto> SetPlayerMissionAsync(int playerId, int missionId, CancellationToken ct);
    Task<PlayerDto> SetPlayerColourAsync(int playerId, string colour, CancellationToken ct);
}