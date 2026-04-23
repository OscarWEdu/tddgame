// - submit attacker and defender typing data
// - calculate attacker and defender score
// - defender wins ties
// - calculate troop loss (1 or 2)
// - save the final resolved result row

namespace TddGame;

// Define the contract for result-related database operations.
public interface IResultsRepository
{
  Task<ResultDto?> GetResultByIdAsync(int id, CancellationToken ct);    // Get one resolved result by id.
  Task<ResultDto?> GetResultByBattleIdAsync(int battleId, CancellationToken ct);     // Get the resolved result connected to one battle.
  Task<ResultDto> CreateResultAsync(CreateResultDto result, CancellationToken ct);    // Create and saves a new resolved result row.
  Task<ResultBattleValidationDto?> GetBattleForResultAsync(int battleId, CancellationToken ct);   // Get the battle data needed to validate and calculate a result.
  Task<bool> TypingChallengeExistsForBattleAsync(int battleId, CancellationToken ct);   // Checks that a typing challenge exists for the battle before result submission.
}