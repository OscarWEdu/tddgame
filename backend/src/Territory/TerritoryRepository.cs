namespace TddGame;

using MySqlConnector;

public class TerritoryRepository(MySqlDataSource db) : ITerritoryRepository
{
    //Get all territories
    public async Task<IEnumerable<TerritoryDto>> GetTerritoriesAsync(CancellationToken ct)
    {
        string sqlQuery = @"SELECT * FROM Territories";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;

        var Territories = new List<TerritoryDto>();

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            Territories.Add(
                new TerritoryDto(
                    Id: reader.GetInt32("id"),
                    Name: reader.GetString("name"),
                    NorthAdjacentId: reader.GetInt32("NorthAdjacentId"),
                    SouthAdjacentId: reader.GetInt32("SouthAdjacentId"),
                    WestAdjacentId: reader.GetInt32("WestAdjacentId"),
                    EastAdjacentId: reader.GetInt32("EastAdjacentId"),
                    ContinentId: reader.GetInt32("continentId")
                )
            );
        }

        return Territories;
    }

    //Get Territory by Id
    public async Task<TerritoryDto?> GetTerritoryByIdAsync(int id, CancellationToken ct)
    {
        var sqlQuery = @"SELECT * FROM Territories WHERE id = @id";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;

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
            ContinentId: reader.GetInt32("continentId")
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
            ContinentId: continentId
        );
    }
}