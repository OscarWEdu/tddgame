
namespace TddGame;

// DTO returned from repository/API for one typing challenge.
public record TypingChallengeDto(
    int Id,
    int Speed,
    int Mistakes,
    string PromptText,
    int BattleId
);

// Data required when creating a new typing challenge.
public record CreateTypingChallengeDto(
    int BattleId,
    string PromptText
);
