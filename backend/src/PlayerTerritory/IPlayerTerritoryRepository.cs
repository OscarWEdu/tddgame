namespace TddGame;

public interface IPlayerTerritoryRepository
{
    Task<IEnumerable<PlayerTerritoryDto>> GetPlayerTerritoriesAsync(CancellationToken ct);
}