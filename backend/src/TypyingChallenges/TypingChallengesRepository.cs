

namespace TddGame;

using MySqlConnector;

// Implements the repository interface using MySQL.
public class TypingChallengesRepository(MySqlDataSource db) : ITypingChallengesRepository
{
  // Gets one typing challenge by its id.
  public async Task<TypingChallengeDto?> GetTypingChallengeByIdAsync(int id, CancellationToken ct)
  {
    var sqlQuery = @"
            SELECT id, speed, mistakes, promptText, battles_id
            FROM TypingChallenges
            WHERE id = @id
            LIMIT 1
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@id", id);

    await using var reader = await command.ExecuteReaderAsync(ct);

    if (await reader.ReadAsync(ct))
    {
      return new TypingChallengeDto(
          Id: reader.GetInt32("id"),
          Speed: reader.GetInt32("speed"),
          Mistakes: reader.GetInt32("mistakes"),
          PromptText: reader.GetString("promptText"),
          BattleId: reader.GetInt32("battles_id")
      );
    }

    return null;
  }

  // Gets the typing challenge connected to one battle.
  public async Task<TypingChallengeDto?> GetTypingChallengeByBattleIdAsync(int battleId, CancellationToken ct)
  {
    var sqlQuery = @"
            SELECT id, speed, mistakes, promptText, battles_id
            FROM TypingChallenges
            WHERE battles_id = @battleId
            LIMIT 1
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@battleId", battleId);

    await using var reader = await command.ExecuteReaderAsync(ct);

    if (await reader.ReadAsync(ct))
    {
      return new TypingChallengeDto(
          Id: reader.GetInt32("id"),
          Speed: reader.GetInt32("speed"),
          Mistakes: reader.GetInt32("mistakes"),
          PromptText: reader.GetString("promptText"),
          BattleId: reader.GetInt32("battles_id")
      );
    }

    return null;
  }

  // Creates a new typing challenge row.
  public async Task<TypingChallengeDto> CreateTypingChallengeAsync(CreateTypingChallengeDto challenge, CancellationToken ct)
  {
    var nextId = await GetNextTypingChallengeIdAsync(ct);

    var sqlQuery = @"
            INSERT INTO TypingChallenges (id, speed, mistakes, promptText, battles_id)
            VALUES (@id, @speed, @mistakes, @promptText, @battleId)
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@id", nextId);
    command.Parameters.AddWithValue("@speed", 0);        // Results are saved later.
    command.Parameters.AddWithValue("@mistakes", 0);     // Results are saved later.
    command.Parameters.AddWithValue("@promptText", challenge.PromptText);
    command.Parameters.AddWithValue("@battleId", challenge.BattleId);

    await command.ExecuteNonQueryAsync(ct);

    return new TypingChallengeDto(
        Id: nextId,
        Speed: 0,
        Mistakes: 0,
        PromptText: challenge.PromptText,
        BattleId: challenge.BattleId
    );
  }

  // Helper method that generates the next integer id manually.
  private async Task<int> GetNextTypingChallengeIdAsync(CancellationToken ct)
  {
    const string sqlQuery = @"
            SELECT COALESCE(MAX(id), 0) + 1
            FROM TypingChallenges
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;

    var result = await command.ExecuteScalarAsync(ct);
    return Convert.ToInt32(result);
  }
}