
namespace TddGame;

using MySqlConnector;

// Repository responsible for all database operations related to Turns
public class TurnsRepository(MySqlDataSource db) : ITurnsRepository
{
  // Fetch the latest active turn for a given game session
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

    // Map database row --> TurnDto
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

  // Fetch all turns for a game session
  public async Task<IEnumerable<TurnDto>> GetTurnsByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct)
  {
    var sqlQuery = @"
        SELECT id, round, phase, status, createAt, gameSessions_id, players_id
        FROM Turns
        WHERE gameSessions_id = @gameSessionId
        ORDER BY id ASC
    ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

    var turns = new List<TurnDto>();

    await using var reader = await command.ExecuteReaderAsync(ct);

    // Loop through all rows and map to DTO
    while (await reader.ReadAsync(ct))
    {
      turns.Add(new TurnDto(
        reader.GetInt32("id"),
        reader.GetInt32("round"),
        Enum.Parse<TurnPhase>(reader.GetString("phase")),
        Enum.Parse<TurnStatus>(reader.GetString("status")),
        reader.GetDateTime("createAt"),
        reader.GetString("gameSessions_id"),
        reader.GetInt32("players_id")
      ));
    }

    return turns;
  }

  // Create a new turn entry
  public async Task<TurnDto> CreateTurnAsync(CreateTurnDto turn, CancellationToken ct)
  {
    var nextId = await GetNextTurnIdAsync(ct); // Generate the new turn id manually

    var sqlQuery = @"
        INSERT INTO Turns (id, round, phase, status, gameSessions_id, players_id)
        VALUES (@id, @round, @phase, @status, @gameSessionId, @playerId)
    ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;

    // Convert enums --> string for DB storage
    command.Parameters.AddWithValue("@id", nextId);
    command.Parameters.AddWithValue("@round", turn.Round);
    command.Parameters.AddWithValue("@phase", turn.Phase.ToString());
    command.Parameters.AddWithValue("@status", turn.Status.ToString());
    command.Parameters.AddWithValue("@gameSessionId", turn.GameSessionId);
    command.Parameters.AddWithValue("@playerId", turn.PlayerId);

    await command.ExecuteNonQueryAsync(ct);

    return new TurnDto(
      nextId,
      turn.Round,
      turn.Phase,
      turn.Status,
      DateTime.Today,
      turn.GameSessionId,
      turn.PlayerId
    );
  }

  // Update phase of the current active turn
  public async Task<bool> ChangeCurrentTurnPhaseAsync(Guid gameSessionId, TurnPhase newPhase, CancellationToken ct)
  {
    var sqlQuery = @"
        UPDATE Turns
        SET phase = @phase
        WHERE gameSessions_id = @gameSessionId
          AND status = 'active'
    ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@phase", newPhase.ToString());

    var rows = await command.ExecuteNonQueryAsync(ct);

    return rows > 0; // true if update succeeded
  }

  // Marks a turn as active or inactive
  public async Task<bool> SetTurnStatusAsync(int turnId, TurnStatus status, CancellationToken ct)
  {
    var sqlQuery = @"UPDATE Turns SET status = @status WHERE id = @turnId";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@status", status.ToString());
    command.Parameters.AddWithValue("@turnId", turnId);

    return await command.ExecuteNonQueryAsync(ct) > 0;
  }

  // Get first alive player (used when starting or wrapping turns)
  public async Task<int?> GetFirstPlayerIdByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct)
  {
    var sqlQuery = @"
        SELECT id FROM Players
        WHERE gameSessions_id = @gameSessionId AND isDead = FALSE
        ORDER BY turnOrder ASC LIMIT 1
    ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

    var result = await command.ExecuteScalarAsync(ct);

    return result == null ? null : Convert.ToInt32(result);
  }

  // Get next player in turn order, wraps around if needed
  // Concept: turn order cycling (last player → first player)
  public async Task<int?> GetNextPlayerIdAsync(Guid gameSessionId, int currentPlayerId, CancellationToken ct)
  {
    return await GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, ct);
  }

  // Get current round number
  public async Task<int?> GetCurrentRoundAsync(Guid gameSessionId, CancellationToken ct)
  {
    var sqlQuery = @"SELECT round FROM Turns WHERE gameSessions_id = @gameSessionId ORDER BY id DESC LIMIT 1";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

    var result = await command.ExecuteScalarAsync(ct);

    return result == null ? null : Convert.ToInt32(result);
  }

  // Generate next turns id manually
  private async Task<int> GetNextTurnIdAsync(CancellationToken ct)
  {
    const string sqlQuery = @"SELECT COALESCE(MAX(id), 0) + 1 FROM Turns";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;

    var result = await command.ExecuteScalarAsync(ct);
    return Convert.ToInt32(result);
  }
}

// GetNextPlayerIdAsync → “Who plays next?”
// GetNextTurnIdAsync → “What id should this new Turns row have?”
// Even though both are used in ending a turn, they are used at different steps:
// 1. Find next player  → GetNextPlayerIdAsync
// 2. Create new turn   → GetNextTurnIdAsync