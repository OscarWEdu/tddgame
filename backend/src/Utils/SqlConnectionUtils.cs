namespace TddGame;

using MySqlConnector;

public static class SqlUtils
{

    //Gets the last Autoincrement Id added in this connection
    public static async Task<int> GetAutoIncrementID(MySqlConnection connection, CancellationToken ct)
    {
        string sqlQuery = @"SELECT LAST_INSERT_ID()";

        await using var command = connection.CreateCommand();

        command.CommandText = sqlQuery;

        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }
}