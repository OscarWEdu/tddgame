namespace TddGame;

public interface IPlayerTerritoryRepository
{
    Task<IEnumerable<PlayerTerritoryDto>> GetPlayerTerritoriesAsync(CancellationToken ct);
    Task<IEnumerable<PlayerTerritoryDto>> GetPlayerPlayerTerritoriesAsync(int playerId, CancellationToken ct);
    Task<PlayerTerritoryDto?> GetPlayerTerritoryByIdAsync(int id, CancellationToken ct);
    Task<PlayerTerritoryDto> CreatePlayerTerritoryAsync(int playerId, int territoryId, CancellationToken ct);
    Task<IEnumerable<PlayerTerritoryDto>> AssignInitialTerritoriesAsync(string gameSessionId, CancellationToken ct);
    Task<bool> DeletePlayerTerritoryAsync(int id, CancellationToken ct);
    Task<bool> UpdatePlayerTerritoryTroopsAsync(int id, int numTroops, CancellationToken ct);
    Task<bool> UpdatePlayerTerritoryCityAsync(int id, bool hasCity, CancellationToken ct);
}