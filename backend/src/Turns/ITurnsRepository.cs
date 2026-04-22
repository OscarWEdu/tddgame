
namespace TddGame;

// Defines the contract for all turn-related database operations
public interface ITurnsRepository
{
  Task<TurnDto?> GetCurrentTurnByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct);  // Gets the latest turn for a specific game session
  Task<IEnumerable<TurnDto>> GetTurnsByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct);  // Gets all turns for a game session.
  Task<TurnDto> CreateTurnAsync(CreateTurnDto turn, CancellationToken ct);  // Creates and returns a new turn row.
  Task<bool> ChangeCurrentTurnPhaseAsync(Guid gameSessionId, TurnPhase newPhase, CancellationToken ct); // Changes the phase of the active turn.
  Task<bool> SetTurnStatusAsync(int turnId, TurnStatus status, CancellationToken ct); // Changes the status of a turn row.
  Task<int?> GetFirstPlayerIdByGameSessionIdAsync(Guid gameSessionId, CancellationToken ct);  // Gets the first player in turn order for a game session.
  Task<int?> GetCurrentRoundAsync(Guid gameSessionId, CancellationToken ct);   // Gets the current round number for a game session.
  Task<int?> GetNextPlayerIdAsync(Guid gameSessionId, int currentPlayerId, CancellationToken ct);  // Gets the next player based on turn order.
}
