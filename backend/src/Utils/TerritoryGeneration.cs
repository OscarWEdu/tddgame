namespace TddGame;

using Microsoft.AspNetCore.Http.HttpResults;

public static class TerritoryGeneration
{
    public static async Task AddTerritories(int width, int height, ITerritoryRepository repo, CancellationToken ct)
    {
        await repo.CreateTerritoryAsync("Michigan", -1, -1, -1, -1, 1, ct);
    }
}