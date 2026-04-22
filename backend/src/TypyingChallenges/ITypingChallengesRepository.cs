
namespace TddGame;

public interface ITypingChallengesRepository
{
    Task<TypingChallengeDto?> GetTypingChallengeByIdAsync(int id, CancellationToken ct);    // Get one typing challenge by id.
    Task<TypingChallengeDto?> GetTypingChallengeByBattleIdAsync(int battleId, CancellationToken ct);    // Get the typing challenge connected to one battle.
    Task<TypingChallengeDto> CreateTypingChallengeAsync(CreateTypingChallengeDto challenge, CancellationToken ct);    // Create a new typing challenge row.
}