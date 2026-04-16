namespace TddGame;

using MySqlConnector;

public class ContinentRepository(MySqlDataSource db) : IContinentRepository
{
    //Get all continents
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

    //Insert new continent
    public async Task<ContinentDto> CreateContinentAsync(string continentName, int bonus, CancellationToken ct)
    {
        //Inserts the data and fetches the autoincrement id
        string sqlQuery = @"INSERT INTO Continents (name, bonusConst) VALUES (@name, @bonusConst)";

        await using var connection = await db.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@name", continentName);
        command.Parameters.AddWithValue("@bonusConst", bonus);

        await command.ExecuteNonQueryAsync(ct);
        var continentId = await SqlUtils.GetAutoIncrementID(connection, ct);

        return new ContinentDto(
            Id: continentId,
            name: continentName,
            bonusConst: bonus
        );
    }
}