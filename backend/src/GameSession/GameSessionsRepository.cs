namespace TddGame;

using MySqlConnector;

public class GameSessionRepository(MySqlDataSource db) : IGameSessionsRepository
{
    public async Task<IEnumerable<GameSessionDto>> GetGameSessionsAsync(CancellationToken ct)
    {
        var sqlQuery = @"SELECT * FROM GameSessions";

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
                    Status: reader.GetString("status")
                )
            );
        }
        return gameSessions;
    }

    public async Task<GameSessionDto?> GetGameSessionByIdAsync(Guid id, CancellationToken ct)
    {
        var sqlQuery = @"SELECT * FROM GameSessions WHERE id = @id";

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
            Status: reader.GetString("status")
        );
    }

    public async Task<GameSessionDto> CreateGameSessionAsync(string name, CancellationToken ct)
    {
        var sessionId = Guid.NewGuid();

        var sqlQuery = @"INSERT INTO GameSessions (id, name, status) VALUES (@id, @name, 'lobby')";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@id", sessionId.ToString());
        command.Parameters.AddWithValue("@name", name);

        await command.ExecuteNonQueryAsync(ct);

        return new GameSessionDto(
            Id: sessionId.ToString(),
            Name: name,
            Status: "lobby"
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