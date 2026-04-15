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
    Task<IEnumerable<PlayerDto>> GetPlayersByGameSessionAsync(int gameSessionId, CancellationToken ct);
    Task<PlayerDto> GetPlayerByIdAsync(int playerId, CancellationToken ct);
}