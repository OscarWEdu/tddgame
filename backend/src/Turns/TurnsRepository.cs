// Implements ITurnsRepository.
// Contains SQL queries for creating, retrieving, and updating turn data (SQL logic).
// Responsible for storing current round/phase and active player turn.

namespace TddGame;

// Imports MySQL database access classes
using MySqlConnector;

// Implements the repository interface using MySQL
public class TurnsRepository(MySqlDataSource db) : ITurnsRepository
{
  // Get the active turn for a specific game session.
  public async Task<TurnDto?> GetCurrentTurnByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct)
  {
    // SQL query to fetch the latest turn row for a game session
    var sqlQuery = @" 
        SELECT id, round, phase, status, createAt, gameSessions_id, players_id
        FROM Turns
        WHERE gameSessions_id = @gameSessionId
          AND status = 'active'   -- only returns the currently active turn
        ORDER BY id DESC
        LIMIT 1
    ";
    // Open a database connection asynchronously
    await using var connection = await db.OpenConnectionAsync(ct);
    // Create a SQL command object
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery; // Set the SQL query text
    command.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString()); // Adds the game session and convert to string because DB column is VARCHAR

    // Execute the query and return a reader
    await using var reader = await command.ExecuteReaderAsync(ct);

    // Check whether a row was returned
    if (await reader.ReadAsync(ct))
    {
      // Create and return a TurnDto from the row
      return new TurnDto(
            Id: reader.GetInt32("id"),
            Round: reader.GetInt32("round"),
            Phase: reader.GetString("phase"),
            Status: reader.GetString("status"),
            CreateAt: reader.GetDateTime("createAt"),
            GameSessionId: reader.GetString("gameSessions_id"),
            PlayerId: reader.GetInt32("players_id")
          );
    }

    return null;  // Return null if no active turn exists yet for the game session 
  }

  // Get all turns for one game session.
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

    // Create a list to store all returned Turns.
    var turns = new List<TurnDto>();

    await using var reader = await command.ExecuteReaderAsync(ct);
    while (await reader.ReadAsync(ct))
    {
      // Add a new TurnDto to the list.
      turns.Add(
          new TurnDto(
              Id: reader.GetInt32("id"),
              Round: reader.GetInt32("round"),
              Phase: reader.GetString("phase"),
              Status: reader.GetString("status"),
              CreateAt: reader.GetDateTime("createAt"),
              GameSessionId: reader.GetString("gameSessions_id"),
              PlayerId: reader.GetInt32("players_id")
          )
      );
    }

    return turns;   // Returns the full list of Turns.
  }

  // Insert a new turn row into the database
  public async Task<TurnDto> CreateTurnAsync(CreateTurnDto turn, CancellationToken ct)
  {
    var nextId = await GetNextTurnIdAsync(ct);

    // SQL insert for a new turn
    var sqlQuery = @"
          INSERT INTO Turns (id, round, phase, status, gameSessions_id, players_id)
          VALUES (@id, @round, @phase, @status, @gameSessionId, @playerId)
        ";

    await using var connection = await db.OpenConnectionAsync(ct);  // Open database connection asynchronously
    await using var command = connection.CreateCommand();           // Create a SQL command

    // Set the SQL query text and add each parament
    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@id", nextId);
    command.Parameters.AddWithValue("@round", turn.Round);
    command.Parameters.AddWithValue("@phase", turn.Phase);
    command.Parameters.AddWithValue("@status", turn.Status);
    command.Parameters.AddWithValue("@gameSessionId", turn.GameSessionId);
    command.Parameters.AddWithValue("@playerId", turn.PlayerId);

    await command.ExecuteNonQueryAsync(ct);   // Execute the insert

    // Return a DTO for the newly created turn
    return new TurnDto(
      Id: nextId,
      Round: turn.Round,
      Phase: turn.Phase,
      Status: turn.Status,
      CreateAt: DateTime.Today,
      GameSessionId: turn.GameSessionId,
      PlayerId: turn.PlayerId
    );
  }

  // Change the phase of the active turn.
  public async Task<bool> ChangeCurrentTurnPhaseAsync(Guid gameSessionId, string newPhase, CancellationToken ct)
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
    command.Parameters.AddWithValue("@phase", newPhase);
    command.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

    var rowsAffected = await command.ExecuteNonQueryAsync(ct); // Execute the update and stores affected rows.

    return rowsAffected > 0; // Return true if at least one row was updated.
  }

  // Update the status of one specific turn row.
  public async Task<bool> SetTurnStatusAsync(int turnId, string status, CancellationToken ct)
  {
    var sqlQuery = @"
            UPDATE Turns
            SET status = @status
            WHERE id = @turnId
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@status", status); // Add the status parameter.
    command.Parameters.AddWithValue("@turnId", turnId); // Add the turn id parameter.

    var rowsAffected = await command.ExecuteNonQueryAsync(ct);

    return rowsAffected > 0;
  }

  // Get the first player by turn order
  public async Task<int?> GetFirstPlayerIdByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct)
  {
    // SQL query to get the player with the lowest turnOrder in one game session
    var sqlQuery = @"
            SELECT id
            FROM Players
            WHERE gameSessions_id = @gameSessionId
              AND isDead = FALSE
            ORDER BY turnOrder ASC
            LIMIT 1
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

    // Execute the query and get a single scalar result 
    var result = await command.ExecuteScalarAsync(ct);

    // Check whether the query returned no result
    if (result == null)
    {
      return null;      // Return null if no player exists
    }

    return Convert.ToInt32(result);       // Convert the scalar value (result) to an integer player id
  }

  // Get the next player's id based on current turn order
  public async Task<int?> GetNextPlayerIdAsync(Guid gameSessionId, int currentPlayerId, CancellationToken ct)
  {
    // Open a database connection asynchronously
    await using var connection = await db.OpenConnectionAsync(ct);

    int currentTurnOrder;   // Store the current player's turnOrder.

    // Create a command to find the current player's turn order.
    await using (var currentPlayerCommand = connection.CreateCommand())
    {
      // SQL query to get the current player's turnOrder.
      currentPlayerCommand.CommandText = @"
                SELECT turnOrder
                FROM Players
                WHERE id = @currentPlayerId
                  AND gameSessions_id = @gameSessionId
                LIMIT 1
            ";

      currentPlayerCommand.Parameters.AddWithValue("@currentPlayerId", currentPlayerId);
      currentPlayerCommand.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

      var currentTurnOrderResult = await currentPlayerCommand.ExecuteScalarAsync(ct);   // Execute the query

      // Check whether the current player was not found
      if (currentTurnOrderResult == null)
      {
        return null;    // Return null if the current player does not exist in the session
      }

      currentTurnOrder = Convert.ToInt32(currentTurnOrderResult);   // Convert the found turn order to int
    }

    // Create a command to find the next player in order
    await using (var nextPlayerCommand = connection.CreateCommand())
    {
      // SQL query to find the next alive player after the current turnOrder
      nextPlayerCommand.CommandText = @"
                SELECT id
                FROM Players
                WHERE gameSessions_id = @gameSessionId
                  AND turnOrder > @currentTurnOrder
                  AND isDead = FALSE
                ORDER BY turnOrder ASC
                LIMIT 1
            ";

      nextPlayerCommand.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());
      nextPlayerCommand.Parameters.AddWithValue("@currentTurnOrder", currentTurnOrder);

      var nextPlayerResult = await nextPlayerCommand.ExecuteScalarAsync(ct);

      // Check whether a next player after current order exists
      if (nextPlayerResult != null)
      {
        return Convert.ToInt32(nextPlayerResult);   // Return the next player's id
      }
    }

    // Creates a command to wrap around to the first alive player.
    await using (var wrapAroundCommand = connection.CreateCommand())
    {
      // SQL query to get the first alive player if current player was last in turn order.
      wrapAroundCommand.CommandText = @"
                SELECT id
                FROM Players
                WHERE gameSessions_id = @gameSessionId
                  AND isDead = FALSE
                ORDER BY turnOrder ASC
                LIMIT 1
            ";

      wrapAroundCommand.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

      var wrapAroundResult = await wrapAroundCommand.ExecuteScalarAsync(ct);

      if (wrapAroundResult == null)
      {
        return null;
      }

      return Convert.ToInt32(wrapAroundResult);
    }
  }

  // Gets the latest round number for a game session.
  public async Task<int?> GetCurrentRoundAsync(Guid gameSessionId, CancellationToken ct)
  {
    // SQL query to get the maximum round number for a game session.
    var sqlQuery = @"
            SELECT MAX(round)
            FROM Turns
            WHERE gameSessions_id = @gameSessionId
        ";

    await using var connection = await db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@gameSessionId", gameSessionId.ToString());

    var result = await command.ExecuteScalarAsync(ct);

    if (result == null || result == DBNull.Value)   // Checks whether no rounds exist yet.
    {
      return null;
    }

    return Convert.ToInt32(result);
  }

  private async Task<int> GetNextTurnIdAsync(CancellationToken ct)
  {
    // Read the next available numeric id
    var sqlQuery = @"SELECT COALESCE(MAX(id), 0) + 1 FROM Turns";

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
// 2. Create new turn   → GetNextTurnIdAsync (inside CreateTurnAsync)