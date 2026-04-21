// Defines database operations for battles.
// Example methods:
// CreateBattleAsync(...)
// GetBattleByIdAsync(...)
// UpdateBattleAsync(...)


namespace TddGame;

public interface IBattlesRepository
{
    Task<BattleDto?> GetBattleByIdAsync(int battleId, CancellationToken ct);      // Get a single battle by id.

    // Get all battles that belong to a game session.
    // Since Battles does not store gameSessions_id directly in the current schema,
    // this is resolved through joins to PlayerTerritories -> Players.
    Task<IEnumerable<BattleDto>> GetBattlesByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct);
    Task<BattleDto> CreateBattleAsync(CreateBattleDto battle, CancellationToken ct);    // Create and returns a new battle row.
    Task<TurnDto?> GetCurrentTurnByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct);    // Get the current active turn for validation.
    Task<BattleTerritoryValidationDto?> GetBattleTerritoryValidationAsync(int playerTerritoryId, CancellationToken ct);   // Get attacker/defender territory data for validation.
    Task<bool> AreTerritoriesAdjacentAsync(int attackerTerritoryId, int defenderTerritoryId, CancellationToken ct);    // Check whether two territories are adjacent.
}