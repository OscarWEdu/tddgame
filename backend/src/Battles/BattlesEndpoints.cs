// Defines the HTTP API routes related to attacks and battles.
// Example routes:
// POST /api/game-session/{gameSession_id}/attack
// GET /api/battles/{id}
// Accepts attack requests from the frontend.
// Validates request shape and calls battle logic/service/repository.

using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

public static class BattlesEndpoints
{
  // Extension method to register all battle endpoints.
  public static IEndpointRouteBuilder MapBattlesEndpoints(this IEndpointRouteBuilder app)
  {
    var battlesEndpointsGroup = app.MapGroup("/api/battles").WithTags("Battles");

    // Get one battle by id.
    battlesEndpointsGroup.MapGet(
        "/{battleId:int}",
        async Task<Results<Ok<BattleDto>, NotFound<string>>> (int battleId, IBattlesRepository repo, CancellationToken ct) =>
        {
          var battle = await repo.GetBattleByIdAsync(battleId, ct);

          if (battle is null)
          {
            return TypedResults.NotFound("Battle not found.");
          }

          return TypedResults.Ok(battle);
        }
    ).WithDescription("Fetch one battle by id").WithSummary("Get battle by id");

    // Get all battles for one game session.
    battlesEndpointsGroup.MapGet(
        "/game-session/{gameSessionId:guid}",
        async Task<Ok<IEnumerable<BattleDto>>> (Guid gameSessionId, IBattlesRepository repo, CancellationToken ct) =>
        {
          var battles = await repo.GetBattlesByGameSessionIdAsync(gameSessionId, ct);
          return TypedResults.Ok(battles);
        }
    ).WithDescription("Get all battles for one game session").WithSummary("Get all battles");

    // Start a new battle after validating turn, ownership, troop count, and adjacency.
    battlesEndpointsGroup.MapPost(
        "/game-session/{gameSessionId:guid}",
        async Task<Results<Ok<BattleDto>, BadRequest<string>, NotFound<string>>>
        (Guid gameSessionId, CreateBattleRequest request, IBattlesRepository repo, CancellationToken ct) =>
        {
          // Validate that there is an active turn.
          var currentTurn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);
          if (currentTurn is null)
          {
            return TypedResults.NotFound("No active turn found for this game session.");
          }

          // Battles can only be started during the attack phase.
          if (currentTurn.Phase != "attack")
          {
            return TypedResults.BadRequest("Battle can only start during the attack phase.");
          }

          // Read attacker and defender player-territory rows.
          var attackerTerritory = await repo.GetBattleTerritoryValidationAsync(request.AttackerTerritoryId, ct);
          var defenderTerritory = await repo.GetBattleTerritoryValidationAsync(request.DefenderTerritoryId, ct);

          if (attackerTerritory is null || defenderTerritory is null)
          {
            return TypedResults.NotFound("One or both territories were not found.");
          }

          // The active player must own the attacking territory.
          if (attackerTerritory.PlayerId != currentTurn.PlayerId)
          {
            return TypedResults.BadRequest("You do not own the attacking territory.");
          }

          // The defender must belong to another player.
          if (defenderTerritory.PlayerId == currentTurn.PlayerId)
          {
            return TypedResults.BadRequest("You cannot attack your own territory.");
          }

          // Minimum troop validation for attack.
          if (attackerTerritory.TroopNum < 2)
          {
            return TypedResults.BadRequest("Not enough troops to start an attack.");
          }

          if (request.AttackingTroops < 1)
          {
            return TypedResults.BadRequest("Attacking troops must be at least 1.");
          }

          // The attacker must always leave at least one troop behind.
          if (request.AttackingTroops >= attackerTerritory.TroopNum)
          {
            return TypedResults.BadRequest("You cannot attack with the last troop.");
          }

          // The base territories must be adjacent.
          var areAdjacent = await repo.AreTerritoriesAdjacentAsync(
                  attackerTerritory.TerritoryId,
                  defenderTerritory.TerritoryId,
                  ct
              );

          if (!areAdjacent)
          {
            return TypedResults.BadRequest("The selected territories are not adjacent.");
          }

          var createdBattle = await repo.CreateBattleAsync(
                  new CreateBattleDto(
                      AttackingTroops: request.AttackingTroops,
                      AttackerTerritoryId: request.AttackerTerritoryId,
                      DefenderTerritoryId: request.DefenderTerritoryId
                  ),
                  ct
              );

          return TypedResults.Ok(createdBattle);
        }
    ).WithDescription("Start a new battle during the current attack phase").WithSummary("Create new battle");

    return app;
  }
}