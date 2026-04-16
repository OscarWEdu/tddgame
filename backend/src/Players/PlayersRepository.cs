// Implements IPlayersRepository.
// Contains SQL for inserting, updating, and selecting player records.
// Maps database results to PlayerDto or related models.

namespace TddGame;

using Microsoft.AspNetCore.StaticFiles;
using MySqlConnector;



public class PlayersRepository(MySqlDataSource db) : IPlayersRepository
{
    public async Task<IEnumerable<PlayerDto>> GetPlayersByGameSessionAsync(string gameSessionId, CancellationToken ct)
    {
        var sqlQuery = @"SELECT * FROM Players WHERE gameSessions_id = @gameSessionId";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@gameSessionId", gameSessionId);

        var players = new List<PlayerDto>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            players.Add(
                new PlayerDto(
                    id: reader.GetInt32("id"),
                    Name: reader.GetString("name"),
                    Colour: reader.GetString("colour"),
                    TurnOrder: reader.GetInt32("turnOrder"),
                    NumGold: reader.GetInt32("numGold"),
                    IsDead: reader.GetBoolean("isDead"),
                    GameSessionId: reader.GetString("gameSessions_id"),
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
                GameSessionId: reader.GetString("gameSessions_id"),
                MissionId: reader.GetInt32("mission_id")
            );
        }
        else
        {
            throw new Exception($"Player with id {playerId} not found.");
        }
    }

    public async Task<PlayerDto> AddPlayerToGameAsync(string gameSessionId, CreatePlayerDto player, CancellationToken ct)
    {
        var sqlQuery = @"INSERT INTO Players (name, colour, turnOrder, numGold, isDead, gameSessions_id, missions_id)
                         VALUES (@name, @colour, @turnOrder, @numGold, @isDead, @gameSessionId, @missionId);
                         SELECT LAST_INSERT_ID();";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@name", player.Name);
        command.Parameters.AddWithValue("@colour", player.Colour);
        command.Parameters.AddWithValue("@turnOrder", player.TurnOrder);
        command.Parameters.AddWithValue("@gameSessionId", gameSessionId);
        command.Parameters.AddWithValue("@missionId", player.MissionId); 

        var insertedId = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
        return new PlayerDto(
            id: insertedId,
            Name: player.Name,
            Colour: player.Colour,
            TurnOrder: player.TurnOrder,
            NumGold: 0, // New players start with 0 gold
            IsDead: false, // New players start alive
            GameSessionId: gameSessionId,
            MissionId: player.MissionId
        );
    }

    public async Task<PlayerDto> UpdatePlayerAsync(int playerId, PlayerStateDto state, CancellationToken ct)
    {
        var sqlQuery = @"UPDATE Players SET numGold = @numGold, isDead = @isDead WHERE id = @playerId";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@numGold", state.NumGold);
        command.Parameters.AddWithValue("@isDead", state.IsDead);
        command.Parameters.AddWithValue("@playerId", playerId);

        await command.ExecuteNonQueryAsync(ct);
        return await GetPlayerByIdAsync(playerId, ct);
    }

    public async Task<bool> DeletePlayerAsync(int playerId, CancellationToken ct)
    {
        var sqlQuery = @"DELETE FROM Players WHERE id = @playerId";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@playerId", playerId);

        var rowsAffected = await command.ExecuteNonQueryAsync(ct);
        if(rowsAffected == 0)
        {
           return false;
        }
        return true;
    }

    public async Task<PlayerDto> SetPlayerMissionAsync(int playerid, int missionid, CancellationToken ct)
    {
        var sqlQuery = @"UPDATE Players SET missions_id = @missionId WHERE id = @playerId";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@missionId", missionid);
        command.Parameters.AddWithValue("@playerId", playerid);

        await command.ExecuteNonQueryAsync(ct);
        return await GetPlayerByIdAsync(playerid, ct);
    }

    public async Task<PlayerDto> SetPlayerColourAsync(int playerId, string colour, CancellationToken ct)
    {
        var sqlQuery =  @"UPDATE Players SET colour = @colour WHERE id = @playerId";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@colour", colour);
        command.Parameters.AddWithValue("@playerId", playerId);

        await command.ExecuteNonQueryAsync(ct);
        return await GetPlayerByIdAsync(playerId, ct);
    }
}