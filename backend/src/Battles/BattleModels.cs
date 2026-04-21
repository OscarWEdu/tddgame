// Contains models related to battles.
// Example includes:
// BattleDto
// CreateBattleRequestDto
// BattleStateDto
// Represents battle data such as id, attackerTerritoryId, defenderTerritoryId, attackingTroops.

namespace TddGame;

// DTO returned from the API/repository when a battle row is read.
public record BattleDto(
    int Id,
    int AttackingTroops,
    int AttackerTerritoryId,
    int DefenderTerritoryId
);

// Request body used when starting a new battle.
public record CreateBattleRequest(
    int AttackerTerritoryId,
    int DefenderTerritoryId,
    int AttackingTroops
);

// Data object used internally by the repository when creating a battle row.
public record CreateBattleDto(
    int AttackingTroops,
    int AttackerTerritoryId,
    int DefenderTerritoryId
);

// DTO used internally for validating territories before a battle starts.
public record BattleTerritoryValidationDto(
    int PlayerTerritoryId,
    int PlayerId,
    int TroopNum,
    int TerritoryId
);