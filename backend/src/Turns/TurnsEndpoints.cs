using Microsoft.AspNetCore.Http.HttpResults; // Imports typed HTTP result helpers.

namespace TddGame;

public static class TurnsEndpoints
{
  public static IEndpointRouteBuilder MapTurnsEndpoints(this IEndpointRouteBuilder app)
  {
    // Create a route group for turn-related endpoints.
    var turnsEndpointsGroup = app.MapGroup("/api/turn").WithTags("Turns");

    // Route for getting the current active turn by game session id.
    turnsEndpointsGroup.MapGet(
      "/{gameSessionId:guid}/turn",
      async Task<Results<Ok<TurnDto>, NotFound<string>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Get the active turn for the given game session from the repository.
        var turn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

        if (turn is null)
        {
          return TypedResults.NotFound("No active turn found for this game session.");
        }

        // Otherwise return 200 with the active turn.
        return TypedResults.Ok(turn);
      }
    ).WithDescription("// Route for getting the current active turn by game session id.")
     .WithSummary("Get turn for a game session");

    // Route for getting all turns by game session id.
    turnsEndpointsGroup.MapGet(
      "/{gameSessionId:guid}/turns",
      async Task<Ok<IEnumerable<TurnDto>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Get all turns for the game session.
        var turns = await repo.GetTurnsByGameSessionIdAsync(gameSessionId, ct);

        return TypedResults.Ok(turns);    // Return all turns
      }
    ).WithDescription("Route for getting all turns in a game session.")
     .WithSummary("Get all turns");

    // Route for creating the first turn.
    turnsEndpointsGroup.MapPost(
      "/{gameSessionId:guid}/turn/start",
      async Task<Results<Ok<TurnDto>, BadRequest<string>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Check whether an active turn already exists.
        var existingTurn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

        // Prevent duplicate active turns.
        if (existingTurn is not null)
        {
          return TypedResults.BadRequest("An active turn already exists for this game session.");
        }

        // Get the first player in turn order.
        var firstPlayerId = await repo.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, ct);

        // Return an error if no player exists in the session.
        if (firstPlayerId is null)
        {
          return TypedResults.BadRequest("Cannot create first turn because no players were found in this game session.");
        }

        // Create the first turn in round 1, build phase, and active status.
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

        // Return the created turn.
        return TypedResults.Ok(createdTurn);
      }
    ).WithDescription("Route for creating the very first turn when a game starts")
     .WithSummary("Create first turn at game start");

    // Route for changing active turn phase.
    turnsEndpointsGroup.MapPatch(
      "/{gameSessionId:guid}/turn/phase",
      async Task<Results<Ok<string>, BadRequest<string>>> (Guid gameSessionId, ChangeTurnPhaseRequest request, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Update the phase of the active turn.
        var updated = await repo.ChangeCurrentTurnPhaseAsync(gameSessionId, request.Phase, ct);

        // Return an error if no active turn was updated.
        if (!updated)
        {
          return TypedResults.BadRequest("Could not change phase because no active turn was found.");
        }

        // Return success message if phase update succeeded
        return TypedResults.Ok($"Turn phase changed to {request.Phase}.");
      }
    ).WithDescription("Route for changing active turn phase")
     .WithSummary("Update active turn phase");

    // Route for ending the current turn and creating the next one
    turnsEndpointsGroup.MapPost(
      "/{gameSessionId:guid}/turn/end",
      async Task<Results<Ok<TurnDto>, BadRequest<string>, NotFound<string>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Get the current active turn.
        var currentTurn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

        if (currentTurn is null)
        {
          return TypedResults.NotFound("No current turn found for this game session.");
        }

        // Find the next player based on turn order.
        var nextPlayerId = await repo.GetNextPlayerIdAsync(gameSessionId, currentTurn.PlayerId, ct);

        // Return an error if no next player could be found.
        if (nextPlayerId is null)
        {
          return TypedResults.BadRequest("Could not determine the next player.");
        }

        // Get the current round number. Default to 1 if none exists.
        var currentRound = await repo.GetCurrentRoundAsync(gameSessionId, ct) ?? 1;

        // Get the first player in turn order.
        var firstPlayerId = await repo.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, ct);

        // Return an error if no players exist.
        if (firstPlayerId is null)
        {
          return TypedResults.BadRequest("No players found in this game session.");
        }

        // Start with the current round number.
        var nextRound = currentRound;

        // If the turn wraps back to the first player, increase the round number.
        if (nextPlayerId.Value == firstPlayerId.Value)
        {
          nextRound += 1;
        }

        // Mark the previous turn as inactive.
        var oldTurnUpdated = await repo.SetTurnStatusAsync(currentTurn.Id, TurnStatus.inactive, ct);

        // Return an error if the current turn could not be deactivated.
        if (!oldTurnUpdated)
        {
          return TypedResults.BadRequest("Could not deactivate the current turn.");
        }

        // Create the next turn in build phase with active status.
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

        // Return the newly created turn.
        return TypedResults.Ok(createdTurn);
      }
    ).WithDescription("Route for ending the current active turn and creating the next one")
     .WithSummary("End current active turn and create next one");

    // Return the route builder so more endpoint groups can be chained.
    return app;
  }
}