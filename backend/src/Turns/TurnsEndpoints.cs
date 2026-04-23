using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

// Defines the HTTP API routes related to turns in a game session.
public static class TurnsEndpoints
{
  public static IEndpointRouteBuilder MapTurnsEndpoints(this IEndpointRouteBuilder app)
  {
    var turnGroup = app.MapGroup("/api/turns").WithTags("Turns");

    // GET /api/turns/current?gameSessionId={gameSessionId}
    turnGroup.MapGet(
        "/current",
        async Task<Results<Ok<TurnDto>, NotFound>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct)
        =>
        {
          var turn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);
          return turn is not null ? TypedResults.Ok(turn) : TypedResults.NotFound();
        }
    ).WithSummary("Get current turn in a game session")
     .WithDescription("Return the current active turn for a specific game session.");

    // GET /api/turns?gameSessionId={gameSessionId}
    turnGroup.MapGet(
        "/",
        async Task<Ok<IEnumerable<TurnDto>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct)
        =>
        {
          var turns = await repo.GetTurnsByGameSessionIdAsync(gameSessionId, ct);
          return TypedResults.Ok(turns);
        }
    ).WithSummary("Get all turns in a game session")
     .WithDescription("Return all turns in a specific game session.");

    // POST /api/turns/start?gameSessionId={gameSessionId}
    turnGroup.MapPost(
        "/start",
        async Task<Results<Ok<TurnDto>, BadRequest<string>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct)
        =>
        {
          var existingTurn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

          if (existingTurn is not null)
          {
            return TypedResults.BadRequest("An active turn already exists for this game session.");
          }

          var firstPlayerId = await repo.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, ct);

          if (firstPlayerId is null)
          {
            return TypedResults.BadRequest("Cannot create first turn because no players were found in this game session.");
          }

          var createdTurn = await repo.CreateTurnAsync(
                  new CreateTurnDto(
                      Round: 1,
                      Phase: TurnPhase.build,
                      Status: TurnStatus.active,
                      GameSessionId: gameSessionId.ToString(),
                      PlayerId: firstPlayerId.Value
                  ),
                  ct
              );

          return TypedResults.Ok(createdTurn);
        }
    ).WithSummary("Create first turn")
     .WithDescription("Create the first active turn for a specific game session.");

    // PATCH /api/turns/phase?gameSessionId={gameSessionId}
    turnGroup.MapPatch(
        "/phase",
        async Task<Results<Ok<string>, NotFound>> (Guid gameSessionId, ChangeTurnPhaseRequest request, ITurnsRepository repo, CancellationToken ct)
        =>
        {
          var updated = await repo.ChangeCurrentTurnPhaseAsync(gameSessionId, request.Phase, ct);

          return updated
                  ? TypedResults.Ok($"Turn phase changed to {request.Phase}.")
                  : TypedResults.NotFound();
        }
    ).WithSummary("Update active turn phase")
     .WithDescription("Update the phase of the current active turn in a specific game session.");

    // POST /api/turns/end?gameSessionId={gameSessionId}
    turnGroup.MapPost(
        "/end",
        async Task<Results<Ok<TurnDto>, BadRequest<string>, NotFound>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct)
        =>
        {
          var currentTurn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

          if (currentTurn is null)
          {
            return TypedResults.NotFound();
          }

          var nextPlayerId = await repo.GetNextPlayerIdAsync(gameSessionId, currentTurn.PlayerId, ct);

          if (nextPlayerId is null)
          {
            return TypedResults.BadRequest("Could not determine the next player.");
          }

          var currentRound = await repo.GetCurrentRoundAsync(gameSessionId, ct) ?? 1;
          var firstPlayerId = await repo.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, ct);

          if (firstPlayerId is null)
          {
            return TypedResults.BadRequest("No players found in this game session.");
          }

          var nextRound = currentRound;

          if (nextPlayerId.Value == firstPlayerId.Value)
          {
            nextRound += 1;
          }

          var oldTurnUpdated = await repo.SetTurnStatusAsync(currentTurn.Id, TurnStatus.inactive, ct);

          if (!oldTurnUpdated)
          {
            return TypedResults.BadRequest("Could not deactivate the current turn.");
          }

          var createdTurn = await repo.CreateTurnAsync(
                  new CreateTurnDto(
                      Round: nextRound,
                      Phase: TurnPhase.build,
                      Status: TurnStatus.active,
                      GameSessionId: gameSessionId.ToString(),
                      PlayerId: nextPlayerId.Value
                  ),
                  ct
              );

          return TypedResults.Ok(createdTurn);
        }
    ).WithSummary("End current turn and create next turn")
     .WithDescription("Deactivate the current turn and create the next active turn in a specific game session.");

    return app;
  }
}