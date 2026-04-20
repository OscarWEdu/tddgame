namespace TddGame;

public interface IPlayerTerritoryRepository
{
    Task<IEnumerable<PlayerTerritoryDto>> GetPlayerTerritoriesAsync(CancellationToken ct);
    Task<IEnumerable<PlayerTerritoryDto>> GetPlayerPlayerTerritoriesAsync(int playerId, CancellationToken ct);
    Task<PlayerTerritoryDto?> GetPlayerTerritoryByIdAsync(int id, CancellationToken ct);
}