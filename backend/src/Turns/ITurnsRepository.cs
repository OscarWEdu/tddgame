// Defines database operations for turns.
// Example methods:
//  GetCurrentTurnAsync(gameSession_id)
//  CreateTurnAsync(...)
//  EndTurnAsync(...)
//  UpdateTurnPhaseAsync(...)
// Used to isolate turn storage logic behind an interface.

namespace TddGame;

// Defines the contract for all turn-related database operations
public interface ITurnsRepository
{
  Task<TurnDto?> GetCurrentTurnByGameSessionIdAsync(string gameSessionId, CancellationToken ct);  // Gets the latest turn for a specific game session
  Task<TurnDto> CreateTurnAsync(CreateTurnDto turn, CancellationToken ct);  // Creates and returns a new turn row.
  Task<int?> GetFirstPlayerIdByGameSessionIdAsync(string gameSessionId, CancellationToken ct);  // Gets the first player in turn order for a game session.
  Task<int?> GetCurrentRoundAsync(string gameSessionId, CancellationToken ct);   // Gets the current round number for a game session.
  Task<int?> GetNextPlayerIdAsync(string gameSessionId, int currentPlayerId, CancellationToken ct);  // Gets the next player based on turn order.
}