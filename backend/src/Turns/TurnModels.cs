namespace TddGame;

using System.Text.Json.Serialization;

// Enum used for the current phase of a turn.
// JsonStringEnumConverter makes the API serialize and deserialize enum values as strings.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TurnPhase
{
  build,
  assigned,
  attack,
  reinforce
}

// Enum used for whether a turn is currently active or already finished.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TurnStatus
{
  active,
  inactive
}

// DTO returned from the API and repository when reading a turn.
public record TurnDto(
  int Id,
  int Round,
  TurnPhase Phase,
  TurnStatus Status,
  DateTime CreateAt,
  string GameSessionId,
  int PlayerId
);

// DTO used when creating a new turn row.
public record CreateTurnDto(
  int Round,
  TurnPhase Phase,
  TurnStatus Status,
  string GameSessionId,
  int PlayerId
);

// Request model used when changing the phase of a turn
public record ChangeTurnPhaseRequest(TurnPhase Phase);