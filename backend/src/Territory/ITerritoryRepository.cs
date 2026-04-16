namespace TddGame;

public interface ITerritoryRepository
{
    Task<IEnumerable<TerritoryDto>> GetTerritoriesAsync(CancellationToken ct);
}