namespace TddGame;

using MySqlConnector;

public class GameSessionRepository(MySqlDataSource db) : IGameSessionsRepository
{
    public async Task<IEnumerable<GameSessionDto>> GetGameSessionsAsync(CancellationToken ct)
    {
        var sqlQuery = @"
            SELECT gs.id, gs.name, gs.status, gs.maxPlayers, COUNT(p.id) AS playerCount
            FROM GameSessions gs
            LEFT JOIN Players p ON p.gameSessions_id = gs.id
            GROUP BY gs.id, gs.name, gs.status, gs.maxPlayers";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;

        var gameSessions = new List<GameSessionDto>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            gameSessions.Add(
                new GameSessionDto(
                    Id: reader.GetString("id"),
                    Name: reader.GetString("name"),
                    Status: Enum.Parse<GameSessionStatus>(reader.GetString("status")),
                    MaxPlayers: reader.GetInt32("maxPlayers"),
                    PlayerCount: Convert.ToInt32(reader["playerCount"])
                )
            );
        }
        return gameSessions;
    }

    public async Task<GameSessionDto?> GetGameSessionByIdAsync(Guid id, CancellationToken ct)
    {
        var sqlQuery = @"
            SELECT gs.id, gs.name, gs.status, gs.maxPlayers, COUNT(p.id) AS playerCount
            FROM GameSessions gs
            LEFT JOIN Players p ON p.gameSessions_id = gs.id
            WHERE gs.id = @id
            GROUP BY gs.id, gs.name, gs.status, gs.maxPlayers";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@id", id.ToString());

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return new GameSessionDto(
            Id: reader.GetString("id"),
            Name: reader.GetString("name"),
            Status: Enum.Parse<GameSessionStatus>(reader.GetString("status")),
            MaxPlayers: reader.GetInt32("maxPlayers"),
            PlayerCount: Convert.ToInt32(reader["playerCount"])
        );
    }

    public async Task<GameSessionDto> CreateGameSessionAsync(string name, int maxPlayers, CancellationToken ct)
    {
        var sessionId = Guid.NewGuid();

        var sqlQuery = @"INSERT INTO GameSessions (id, name, status, maxPlayers) VALUES (@id, @name, 'lobby', @maxPlayers)";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@id", sessionId.ToString());
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@maxPlayers", maxPlayers);

        await command.ExecuteNonQueryAsync(ct);

        return new GameSessionDto(
            Id: sessionId.ToString(),
            Name: name,
            Status: GameSessionStatus.lobby,
            MaxPlayers: maxPlayers,
            PlayerCount: 0
        );
    }

    public async Task<bool> UpdateGameSessionStatusAsync(Guid id, string status, CancellationToken ct)
    {
        var sqlQuery = @"UPDATE GameSessions SET status = @status WHERE id = @id";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@status", status);

        var rowsAffected = await command.ExecuteNonQueryAsync(ct);

        if (rowsAffected == 0)
            return false;

        return true;
    }

    public async Task<bool> DeleteGameSessionAsync(Guid id, CancellationToken ct)
    {
        var sqlQuery = @"DELETE FROM GameSessions WHERE id = @id";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@id", id.ToString());

        var rowsAffected = await command.ExecuteNonQueryAsync(ct);

        if (rowsAffected == 0)
            return false;

        return true;

    }
}