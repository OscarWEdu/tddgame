namespace TddGame;

public class GameSessionRepository(MySqlDataSource db): IGameSessionsRepository
{
    public async Task<IEnumerable<GameSessionDto>> GetGameSessionsAsync(CancellationToken ct)
    {
        var sqlQuery = @"SELECT * FROM GameSessions";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;

        var gameSessions = new List<GameSessionDto>();
        await using var reader = await command.ExecuteReaderAsync();
        while(await reader.ReadAsync(ct))
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
}