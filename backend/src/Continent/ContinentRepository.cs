namespace TddGame;

using MySqlConnector;

public class ContinentRepository(MySqlDataSource db) : IContinentRepository
{
    public async Task<IEnumerable<ContinentDto>> GetContinentsAsync(CancellationToken ct)
    {
        string sqlQuery = @"SELECT * FROM Continents";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;

        var Continents = new List<ContinentDto>();

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            Continents.Add(
                new ContinentDto(
                    Id: reader.GetInt32("id"),
                    name: reader.GetString("name"),
                    bonusConst: reader.GetInt32("bonusConst")
                )
            );
        }
        return Continents;
    }
}