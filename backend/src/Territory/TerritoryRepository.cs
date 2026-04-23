namespace TddGame;

using MySqlConnector;

public class TerritoryRepository(MySqlDataSource db) : ITerritoryRepository
{
    //Get all territories
    public async Task<IEnumerable<TerritoryDto>> GetTerritoriesAsync(CancellationToken ct)
    {
        await using var connection = await db.OpenConnectionAsync(ct);

        var adjacenciesByTerritoryId = await LoadAllAdjacenciesAsync(connection, ct);

        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM Territories";

        var Territories = new List<TerritoryDto>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var territoryId = reader.GetInt32("id");
            Territories.Add(
                new TerritoryDto(
                    Id: territoryId,
                    Name: reader.GetString("name"),
                    NorthAdjacentId: reader.GetInt32("NorthAdjacentId"),
                    SouthAdjacentId: reader.GetInt32("SouthAdjacentId"),
                    WestAdjacentId: reader.GetInt32("WestAdjacentId"),
                    EastAdjacentId: reader.GetInt32("EastAdjacentId"),
                    ContinentId: reader.GetInt32("continentId"),
                    AdjacentTerritoryIds: adjacenciesByTerritoryId.TryGetValue(territoryId, out var ids)
                        ? ids.ToArray()
                        : Array.Empty<int>()
                )
            );
        }

        return Territories;
    }

    //Get Territory by Id
    public async Task<TerritoryDto?> GetTerritoryByIdAsync(int id, CancellationToken ct)
    {
        await using var connection = await db.OpenConnectionAsync(ct);

        var adjacentIds = await LoadAdjacenciesForAsync(connection, id, ct);

        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM Territories WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new TerritoryDto(
            Id: reader.GetInt32("id"),
            Name: reader.GetString("name"),
            NorthAdjacentId: reader.GetInt32("NorthAdjacentId"),
            SouthAdjacentId: reader.GetInt32("SouthAdjacentId"),
            WestAdjacentId: reader.GetInt32("WestAdjacentId"),
            EastAdjacentId: reader.GetInt32("EastAdjacentId"),
            ContinentId: reader.GetInt32("continentId"),
            AdjacentTerritoryIds: adjacentIds
        );
    }

    //Insert new Territory
    public async Task<TerritoryDto> CreateTerritoryAsync(string name, int northAdjacentId, int southAdjacentId, int westAdjacentId, int eastAdjacentId, int continentId, CancellationToken ct)
    {
        string sqlQuery = @"INSERT INTO Territories (name, NorthAdjacentId, SouthAdjacentId, WestAdjacentId, EastAdjacentId, Continentid) VALUES (@name, @NorthAdjacentId, @SouthAdjacentId, @WestAdjacentId, @EastAdjacentId, @Continentid)";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@NorthAdjacentId", northAdjacentId);
        command.Parameters.AddWithValue("@SouthAdjacentId", southAdjacentId);
        command.Parameters.AddWithValue("@WestAdjacentId", westAdjacentId);
        command.Parameters.AddWithValue("@EastAdjacentId", eastAdjacentId);
        command.Parameters.AddWithValue("@Continentid", continentId);

        await command.ExecuteNonQueryAsync(ct);
        var territoryId = await SqlUtils.GetAutoIncrementID(connection, ct);

        return new TerritoryDto(
            Id: territoryId,
            Name: name,
            NorthAdjacentId: northAdjacentId,
            SouthAdjacentId: southAdjacentId,
            WestAdjacentId: westAdjacentId,
            EastAdjacentId: eastAdjacentId,
            ContinentId: continentId,
            AdjacentTerritoryIds: Array.Empty<int>()
        );
    }

    private static async Task<Dictionary<int, List<int>>> LoadAllAdjacenciesAsync(
        MySqlConnection connection, CancellationToken ct)
    {
        var map = new Dictionary<int, List<int>>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT territoryId, adjacentTerritoryId FROM TerritoryAdjacencies";
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var territoryId = reader.GetInt32("territoryId");
            var adjacentId = reader.GetInt32("adjacentTerritoryId");
            if (!map.TryGetValue(territoryId, out var list))
            {
                list = new List<int>();
                map[territoryId] = list;
            }
            list.Add(adjacentId);
        }
        return map;
    }

    private static async Task<int[]> LoadAdjacenciesForAsync(
        MySqlConnection connection, int territoryId, CancellationToken ct)
    {
        var ids = new List<int>();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT adjacentTerritoryId FROM TerritoryAdjacencies WHERE territoryId = @id";
        command.Parameters.AddWithValue("@id", territoryId);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetInt32("adjacentTerritoryId"));
        }
        return ids.ToArray();
    }
}
