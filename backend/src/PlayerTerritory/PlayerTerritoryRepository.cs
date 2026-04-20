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
    //Get all playerTerritories owned by a given player
    public async Task<IEnumerable<PlayerTerritoryDto>> GetPlayerPlayerTerritoriesAsync(int playerId, CancellationToken ct)
    {
        string sqlQuery = @"SELECT * FROM PlayerTerritories WHERE playerId = @playerId";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@playerId", playerId);

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

    //Get PlayerTerritory by id
    public async Task<PlayerTerritoryDto?> GetPlayerTerritoryByIdAsync(int id, CancellationToken ct)
    {
        var sqlQuery = @"SELECT * FROM PlayerTerritories WHERE id = @id";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;

        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new PlayerTerritoryDto(
            Id: reader.GetInt32("id"),
            TroopNum: reader.GetInt32("troopNum"),
            HasCity: reader.GetBoolean("hasCity"),
            PlayerId: reader.GetInt32("playerId"),
            TerritoryId: reader.GetInt32("territoryId")
        );
    }

    //Insert new playerTerritory
    public async Task<PlayerTerritoryDto> CreatePlayerTerritoryAsync(int playerId, int territoryId, CancellationToken ct)
    {
        string sqlQuery = @"INSERT INTO PlayerTerritories (playerId, territoryId) VALUES (@playerId, @territoryId)";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@playerId", playerId);
        command.Parameters.AddWithValue("@territoryId", territoryId);

        await command.ExecuteNonQueryAsync(ct);
        var playerTerritoryId = await SqlUtils.GetAutoIncrementID(connection, ct);

        return new PlayerTerritoryDto(
            Id: playerTerritoryId,
            TroopNum: 0,
            HasCity: false,
            PlayerId: playerId,
            TerritoryId: territoryId
        );
    }
}