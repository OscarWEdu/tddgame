// - submit attacker and defender typing data
// - calculate attacker and defender score
// - defender wins ties
// - calculate troop loss (1 or 2)
// - save the final resolved result row

namespace TddGame;

using System.Text.Json.Serialization;

// Enum used for the final winner of a resolved battle result.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BattleWinner
{
    attacker,
    defender
}

// DTO returned from repository/API for one resolved battle result.
public record ResultDto(
    int Id,
    int BattleId,
    BattleWinner Winner,
    int AttackerScore,
    int DefenderScore,
    int AttackerMistakes,
    int DefenderMistakes,
    bool AttackerCompleted,
    bool DefenderCompleted,
    int AttackerTroopLoss,
    int DefenderTroopLoss
);

// Request body used when both players submit their typing data for one battle.
public record CreateResultRequest(
    int AttackerWpm,
    int AttackerMistakes,
    bool AttackerCompleted,
    int DefenderWpm,
    int DefenderMistakes,
    bool DefenderCompleted
);

// DTO used by the repository when creating a resolved result row.
public record CreateResultDto(
    int BattleId,
    BattleWinner Winner,
    int AttackerScore,
    int DefenderScore,
    int AttackerMistakes,
    int DefenderMistakes,
    bool AttackerCompleted,
    bool DefenderCompleted,
    int AttackerTroopLoss,
    int DefenderTroopLoss
);

// DTO used to validate that a battle exists before saving a result.
public record ResultBattleValidationDto(
    int Id,
    int AttackingTroops,
    int DefenderTroops
);