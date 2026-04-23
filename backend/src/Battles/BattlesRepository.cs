// Implements IBattlesRepository.
// Contains SQL queries for storing and retrieving battle records.
// Maps battle rows from MySQL into BattleDto or model objects.


namespace TddGame;

using MySqlConnector;

// Implements the repository interface using MySQL.
public class BattlesRepository(MySqlDataSource db) : IBattlesRepository
{
  // Get a single battle row by id.
  public async Task<BattleDto?> GetBattleByIdAsync(int battleId, CancellationToken ct)
  {
    var sqlQuery = @"
        SELECT id, attackingTroops, attackerTerritoryId, defenderTerritoryId
        FROM Battles
        WHERE id = @battleId
        LIMIT 1
    ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@battleId", battleId);

    await using var reader = await command.ExecuteReaderAsync(ct);

    if (await reader.ReadAsync(ct))
    {
      return new BattleDto(
        Id: reader.GetInt32("id"),
        AttackingTroops: reader.GetInt32("attackingTroops"),
        AttackerTerritoryId: reader.GetInt32("attackerTerritoryId"),
        DefenderTerritoryId: reader.GetInt32("defenderTerritoryId")
      );
    }

    return null;
  }


  // Get all battles for one game session.
  public async Task<IEnumerable<BattleDto>> GetBattlesByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct)
  {
    // Display unique rows, no duplication where it exists
    var sqlQuery = @"
            SELECT DISTINCT b.id, b.attackingTroops, b.attackerTerritoryId, b.defenderTerritoryId
            FROM Battles b
            INNER JOIN PlayerTerritories pt ON pt.id = b.attackerTerritoryId
            INNER JOIN Players p ON p.id = pt.playerId
            WHERE p.gameSessions_id = @gameSessionId
            ORDER BY b.id ASC
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

    var battles = new List<BattleDto>();

    await using var reader = await command.ExecuteReaderAsync(ct);
    while (await reader.ReadAsync(ct))
    {
      battles.Add(
          new BattleDto(
              Id: reader.GetInt32("id"),
              AttackingTroops: reader.GetInt32("attackingTroops"),
              AttackerTerritoryId: reader.GetInt32("attackerTerritoryId"),
              DefenderTerritoryId: reader.GetInt32("defenderTerritoryId")
          )
      );
    }

    return battles;
  }


  // Insert a new battle row.
  public async Task<BattleDto> CreateBattleAsync(CreateBattleDto battle, CancellationToken ct)
  {
    var nextId = await GetNextBattleIdAsync(ct);

    var sqlQuery = @"
            INSERT INTO Battles (id, attackingTroops, attackerTerritoryId, defenderTerritoryId)
            VALUES (@id, @attackingTroops, @attackerTerritoryId, @defenderTerritoryId)
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@id", nextId);
    command.Parameters.AddWithValue("@attackingTroops", battle.AttackingTroops);
    command.Parameters.AddWithValue("@attackerTerritoryId", battle.AttackerTerritoryId);
    command.Parameters.AddWithValue("@defenderTerritoryId", battle.DefenderTerritoryId);

    await command.ExecuteNonQueryAsync(ct);

    return new BattleDto(
        Id: nextId,
        AttackingTroops: battle.AttackingTroops,
        AttackerTerritoryId: battle.AttackerTerritoryId,
        DefenderTerritoryId: battle.DefenderTerritoryId
    );
  }


  // Read the current active turn so the battle endpoint can validate player and phase.
  public async Task<TurnDto?> GetCurrentTurnByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct)
  {
    var sqlQuery = @"
            SELECT id, round, phase, status, createAt, gameSessions_id, players_id
            FROM Turns
            WHERE gameSessions_id = @gameSessionId
              AND status = 'active'
            ORDER BY id DESC
            LIMIT 1
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

    await using var reader = await command.ExecuteReaderAsync(ct);

    if (await reader.ReadAsync(ct))
    {
      return new TurnDto(
          Id: reader.GetInt32("id"),
          Round: reader.GetInt32("round"),
          Phase: Enum.Parse<TurnPhase>(reader.GetString("phase")),
          Status: Enum.Parse<TurnStatus>(reader.GetString("status")),
          CreateAt: reader.GetDateTime("createAt"),
          GameSessionId: reader.GetString("gameSessions_id"),
          PlayerId: reader.GetInt32("players_id")
      );
    }

    return null;
  }

  // Reads the player territory row together with the owner player id (territory owner) and contending territory id.
  public async Task<BattleTerritoryValidationDto?> GetBattleTerritoryValidationAsync(int playerTerritoryId, CancellationToken ct)
  {
    var sqlQuery = @"
            SELECT id, playerId, troopNum, territoryId
            FROM PlayerTerritories
            WHERE id = @playerTerritoryId
            LIMIT 1
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@playerTerritoryId", playerTerritoryId);

    await using var reader = await command.ExecuteReaderAsync(ct);

    if (await reader.ReadAsync(ct))
    {
      return new BattleTerritoryValidationDto(
          PlayerTerritoryId: reader.GetInt32("id"),
          PlayerId: reader.GetInt32("playerId"),
          TroopNum: reader.GetInt32("troopNum"),
          TerritoryId: reader.GetInt32("territoryId")
      );
    }

    return null;
  }

  // Check whether the underlying Territory rows are adjacent in any direction.
  public async Task<bool> AreTerritoriesAdjacentAsync(int attackerTerritoryId, int defenderTerritoryId, CancellationToken ct)
  {
    var sqlQuery = @"
            SELECT COUNT(*)
            FROM TerritoryAdjacencies
            WHERE territoryId = @attackerTerritoryId
              AND adjacentTerritoryId = @defenderTerritoryId
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@attackerTerritoryId", attackerTerritoryId);
    command.Parameters.AddWithValue("@defenderTerritoryId", defenderTerritoryId);

    var result = await command.ExecuteScalarAsync(ct);
    return Convert.ToInt32(result) > 0;
  }

  // Get the next available numeric id.
  private async Task<int> GetNextBattleIdAsync(CancellationToken ct)
  {
    var sqlQuery = @"SELECT COALESCE(MAX(id), 0) + 1 FROM Battles";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;

    var result = await command.ExecuteScalarAsync(ct);
    return Convert.ToInt32(result);
  }
}