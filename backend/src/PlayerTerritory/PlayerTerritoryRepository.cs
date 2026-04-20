namespace TddGame;

using MySqlConnector;

public class PlayerTerritoryRepository(MySqlDataSource db) : IPlayerTerritoryRepository
{
    //Get all playerterritories
    public async Task<IEnumerable<PlayerTerritoryDto>> GetPlayerTerritoriesAsync(CancellationToken ct)
    {
        string sqlQuery = @"SELECT * FROM PlayerTerritories";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;

        var playerTerritories = new List<PlayerTerritoryDto>();

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            playerTerritories.Add(
                new PlayerTerritoryDto(
                    Id: reader.GetInt32("id"),
                    TroopNum: reader.GetInt32("troopNum"),
                    HasCity: reader.GetBoolean("hasCity"),
                    PlayerId: reader.GetInt32("playerId"),
                    TerritoryId: reader.GetInt32("territoryId")
                )
            );
        }

        return playerTerritories;
    }
}