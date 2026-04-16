namespace TddGame;

using MySqlConnector;

public class TerritoryRepository(MySqlDataSource db) : ITerritoryRepository
{
    public async Task<IEnumerable<TerritoryDto>> GetTerritoriesAsync(CancellationToken ct)
    {
        var sqlQuery = @"SELECT * FROM Territories";

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
                    continents_id: reader.GetInt32("continents_id")
                )
            );
        }

        return Territories;
    }
}