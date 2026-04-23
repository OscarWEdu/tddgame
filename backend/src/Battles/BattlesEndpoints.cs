using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

// Defines the HTTP API routes related to battles.
public static class BattlesEndpoints
{
  public static IEndpointRouteBuilder MapBattlesEndpoints(this IEndpointRouteBuilder app)
  {
    var battleGroup = app.MapGroup("/api/battles").WithTags("Battles");

    // GET /api/battles/{battleId}
    battleGroup.MapGet(
        "/{battleId:int}",
        async Task<Results<Ok<BattleDto>, NotFound>> (int id, IBattlesRepository repo, CancellationToken ct)
        =>
        {
          var battle = await repo.GetBattleByIdAsync(id, ct);
          return battle is not null ? TypedResults.Ok(battle) : TypedResults.NotFound();
        }
    ).WithSummary("Get battle by id")
     .WithDescription("Return one specific battle by id.");

    // GET /api/battles?gameSessionId={gameSessionId}
    battleGroup.MapGet(
        "/",
        async Task<Ok<IEnumerable<BattleDto>>> (Guid gameSessionId, IBattlesRepository repo, CancellationToken ct)
        =>
        {
          var battles = await repo.GetBattlesByGameSessionIdAsync(gameSessionId, ct);
          return TypedResults.Ok(battles);
        }
    ).WithSummary("Get all battles in a game session")
     .WithDescription("Return all battles that belong to a specific game session.");

    // POST /api/battles?gameSessionId={gameSessionId}
    battleGroup.MapPost(
        "/",
        async Task<Results<Ok<BattleDto>, BadRequest<string>>> (
            Guid gameSessionId,
            CreateBattleRequest request,
            IBattlesRepository repo,
            CancellationToken ct)
        =>
        {
          var currentTurn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

          if (currentTurn is null)
          {
            return TypedResults.BadRequest("No active turn found for this game session.");
          }

          if (currentTurn.Phase != TurnPhase.attack)
          {
            return TypedResults.BadRequest("Battle can only start during the attack phase.");
          }

          var attackerTerritory = await repo.GetBattleTerritoryValidationAsync(request.AttackerTerritoryId, ct);
          var defenderTerritory = await repo.GetBattleTerritoryValidationAsync(request.DefenderTerritoryId, ct);

          if (attackerTerritory is null || defenderTerritory is null)
          {
            return TypedResults.BadRequest("One or both selected territories were not found.");
          }

          if (attackerTerritory.PlayerId != currentTurn.PlayerId)
          {
            return TypedResults.BadRequest("You do not own the attacking territory.");
          }

          if (defenderTerritory.PlayerId == currentTurn.PlayerId)
          {
            return TypedResults.BadRequest("You cannot attack your own territory.");
          }

          if (attackerTerritory.TroopNum < 2)
          {
            return TypedResults.BadRequest("Not enough troops to attack.");
          }

          if (request.AttackingTroops < 1)
          {
            return TypedResults.BadRequest("Attacking troops must be at least 1.");
          }

          if (request.AttackingTroops >= attackerTerritory.TroopNum)
          {
            return TypedResults.BadRequest("You cannot attack with the last troop.");
          }

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
    ).WithSummary("Create a battle")
     .WithDescription("Start a new battle during the current attack phase in a specific game session.");

    return app;
  }
}