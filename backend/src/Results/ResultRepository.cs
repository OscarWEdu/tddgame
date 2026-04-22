// - submit attacker and defender typing data
// - calculate attacker and defender score
// - defender wins ties
// - calculate troop loss (1 or 2)
// - save the final resolved result row

namespace TddGame;

using MySqlConnector;

public class ResultRepository(MySqlDataSource db) : IResultsRepository
{
  // Get one result by id.
  public async Task<ResultDto?> GetResultByIdAsync(int id, CancellationToken ct)
  {
    const string sqlQuery = @"
            SELECT id, battles_id, winner, attackerScore, defenderScore,
                   attackerMistakes, defenderMistakes,
                   attackerCompleted, defenderCompleted,
                   attackerTroopLoss, defenderTroopLoss
            FROM Results
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
      return MapResult(reader);
    }

    return null;
  }

  // Get the result connected to one battle.
  public async Task<ResultDto?> GetResultByBattleIdAsync(int battleId, CancellationToken ct)
  {
    const string sqlQuery = @"
            SELECT id, battles_id, winner, attackerScore, defenderScore,
                   attackerMistakes, defenderMistakes,
                   attackerCompleted, defenderCompleted,
                   attackerTroopLoss, defenderTroopLoss
            FROM Results
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
      return MapResult(reader);
    }

    return null;
  }

  // Create and save a new resolved result row.
  public async Task<ResultDto> CreateResultAsync(CreateResultDto result, CancellationToken ct)
  {
    var nextId = await GetNextResultIdAsync(ct);

    const string sqlQuery = @"
            INSERT INTO Results (
                id,
                battles_id,
                winner,
                attackerScore,
                defenderScore,
                attackerMistakes,
                defenderMistakes,
                attackerCompleted,
                defenderCompleted,
                attackerTroopLoss,
                defenderTroopLoss
            )
            VALUES (
                @id,
                @battleId,
                @winner,
                @attackerScore,
                @defenderScore,
                @attackerMistakes,
                @defenderMistakes,
                @attackerCompleted,
                @defenderCompleted,
                @attackerTroopLoss,
                @defenderTroopLoss
            )
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@id", nextId);
    command.Parameters.AddWithValue("@battleId", result.BattleId);
    command.Parameters.AddWithValue("@winner", result.Winner.ToString());
    command.Parameters.AddWithValue("@attackerScore", result.AttackerScore);
    command.Parameters.AddWithValue("@defenderScore", result.DefenderScore);
    command.Parameters.AddWithValue("@attackerMistakes", result.AttackerMistakes);
    command.Parameters.AddWithValue("@defenderMistakes", result.DefenderMistakes);
    command.Parameters.AddWithValue("@attackerCompleted", result.AttackerCompleted);
    command.Parameters.AddWithValue("@defenderCompleted", result.DefenderCompleted);
    command.Parameters.AddWithValue("@attackerTroopLoss", result.AttackerTroopLoss);
    command.Parameters.AddWithValue("@defenderTroopLoss", result.DefenderTroopLoss);

    await command.ExecuteNonQueryAsync(ct);

    return new ResultDto(
        Id: nextId,
        BattleId: result.BattleId,
        Winner: result.Winner,
        AttackerScore: result.AttackerScore,
        DefenderScore: result.DefenderScore,
        AttackerMistakes: result.AttackerMistakes,
        DefenderMistakes: result.DefenderMistakes,
        AttackerCompleted: result.AttackerCompleted,
        DefenderCompleted: result.DefenderCompleted,
        AttackerTroopLoss: result.AttackerTroopLoss,
        DefenderTroopLoss: result.DefenderTroopLoss
    );
  }

  // Get the battle data needed to validate and calculate a result.
  // Defender troop count is read from the defending PlayerTerritory row.
  public async Task<ResultBattleValidationDto?> GetBattleForResultAsync(int battleId, CancellationToken ct)
  {
    const string sqlQuery = @"
            SELECT b.id, b.attackingTroops, defenderPt.troopNum AS defenderTroops
            FROM Battles b
            INNER JOIN PlayerTerritories defenderPt ON defenderPt.id = b.defenderTerritoryId
            WHERE b.id = @battleId
            LIMIT 1
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@battleId", battleId);

    await using var reader = await command.ExecuteReaderAsync(ct);

    if (await reader.ReadAsync(ct))
    {
      return new ResultBattleValidationDto(
          Id: reader.GetInt32("id"),
          AttackingTroops: reader.GetInt32("attackingTroops"),
          DefenderTroops: reader.GetInt32("defenderTroops")
      );
    }

    return null;
  }

  // Check that a typing challenge already exists for the battle.
  public async Task<bool> TypingChallengeExistsForBattleAsync(int battleId, CancellationToken ct)
  {
    const string sqlQuery = @"
            SELECT COUNT(*)
            FROM TypingChallenges
            WHERE battles_id = @battleId
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@battleId", battleId);

    var result = await command.ExecuteScalarAsync(ct);
    return Convert.ToInt32(result) > 0;
  }

  // Helper method that generates the next integer id manually.
  private async Task<int> GetNextResultIdAsync(CancellationToken ct)
  {
    const string sqlQuery = @"
            SELECT COALESCE(MAX(id), 0) + 1
            FROM Results
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;

    var result = await command.ExecuteScalarAsync(ct);
    return Convert.ToInt32(result);
  }

  // Helper method that maps one database row into a ResultDto.
  private static ResultDto MapResult(MySqlDataReader reader)
  {
    return new ResultDto(
        Id: reader.GetInt32("id"),
        BattleId: reader.GetInt32("battles_id"),
        Winner: Enum.Parse<BattleWinner>(reader.GetString("winner")),
        AttackerScore: reader.GetInt32("attackerScore"),
        DefenderScore: reader.GetInt32("defenderScore"),
        AttackerMistakes: reader.GetInt32("attackerMistakes"),
        DefenderMistakes: reader.GetInt32("defenderMistakes"),
        AttackerCompleted: reader.GetBoolean("attackerCompleted"),
        DefenderCompleted: reader.GetBoolean("defenderCompleted"),
        AttackerTroopLoss: reader.GetInt32("attackerTroopLoss"),
        DefenderTroopLoss: reader.GetInt32("defenderTroopLoss")
    );
  }
}