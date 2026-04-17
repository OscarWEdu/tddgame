// Contains models related to turns
// includes; TurnDto, EndTurnRequestDto, ChangePhaseRequestDto (DTO records)
// Represents turn data such as id, round, phase, player_id, gameSession_id, createdAt.

namespace TddGame;

// DTO used when returning turn data from API or repository
public record TurnDto(
  int Id,
  int Round,
  string Phase,
  DateTime CreateAt,
  string GameSessionId,
  int PlayerId
);

// Data needed when creating a new turn
public record CreateTurnDto(
  int Round,
  string Phase,
  string GameSessionId,
  int PlayerId
);