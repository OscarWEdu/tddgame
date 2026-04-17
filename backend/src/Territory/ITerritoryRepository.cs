namespace TddGame;

public interface ITerritoryRepository
{
    Task<IEnumerable<TerritoryDto>> GetTerritoriesAsync(CancellationToken ct);
    Task<TerritoryDto?> GetTerritoryByIdAsync(int id, CancellationToken ct);
    Task<TerritoryDto> CreateTerritoryAsync(string name, int NorthAdjacentId, int SouthAdjacentId, int WestAdjacentId, int EastAdjacentId, int ContinentId, CancellationToken ct);
}