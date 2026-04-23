namespace TddGame;

public static class TerritoryGeneration
{
    public static async Task<IResult> AddTerritories(int width, int height, ITerritoryRepository repo, CancellationToken ct)
    {
        var territories = new List<TerritoryDto>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                string name = $"Territory_{x}_{y}";
                int north = -1;
                int south = -1;
                int west = x-1;
                int east = -1;

                if (y != height) { north = y-1; }
                if (y < height - 1) { south = y + 1; }
                if (x != width) { west = x-1; }
                if (x < width - 1) { east = x + 1; }
                
                territories.Add(await repo.CreateTerritoryAsync(name, north, south, west, east, 1, ct));
            }
        }
        return TypedResults.Ok(territories);
    }
}