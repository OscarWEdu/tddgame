// Implements IPlayersRepository.
// Contains SQL for inserting, updating, and selecting player records.
// Maps database results to PlayerDto or related models.

namespace TddGame;
using MySqlConnector;



public class PlayersRepository(MySqlDataSource db) : IPlayersRepository
{
    public async Task<IEnumerable<PlayerDto>> GetPlayersByGameSessionAsync(int gameSessionId, CancellationToken ct)
    {
        var sqlQuery = @"SELECT * FROM Players WHERE gameSessions_id = @gameSessionId";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@gameSessionId", gameSessionId);

        var players = new List<PlayerDto>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct));
        {
            players.Add(
                new PlayerDto(
                    id: reader.GetInt32("id"),
                    Name: reader.GetString("name"),
                    Colour: reader.GetString("colour"),
                    TurnOrder: reader.GetInt32("turnOrder"),
                    NumGold: reader.GetInt32("numGold"),
                    IsDead: reader.GetBoolean("isDead"),
                    GameSessionId: reader.GetInt32("gameSessions_id"),
                    MissionId: reader.GetInt32("mission_id")
                )
            );
        }
    return players;
    }

    public async Task<PlayerDto> GetPlayerByIdAsync(int playerId, CancellationToken ct)
    {
        var sqlQuery = @"SELECT * FROM Players WHERE id = @playerId";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@playerId", playerId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return new PlayerDto(
                id: reader.GetInt32("id"),
                Name: reader.GetString("name"),
                Colour: reader.GetString("colour"),
                TurnOrder: reader.GetInt32("turnOrder"),
                NumGold: reader.GetInt32("numGold"),
                IsDead: reader.GetBoolean("isDead"),
                GameSessionId: reader.GetInt32("gameSessions_id"),
                MissionId: reader.GetInt32("mission_id")
            );
        }
        else
        {
            throw new Exception($"Player with id {playerId} not found.");
        }
    }
}