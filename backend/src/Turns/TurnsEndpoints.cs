// TurnsEndpoints
// Defines the HTTP Minimal API routes related to turn handling.
// Example:
// GET /api/game-session/{gameSession_id}/turn
// POST /api/game-session/{gameSession_id}/end-turn
// POST /api/game-session/{gameSession_id}/phase
// Receives turn-related requests from the frontend.
// Delegates logic to repository/service classes.


using Microsoft.AspNetCore.Http.HttpResults;    // Imports typed HTTP result helpers.

namespace TddGame;

// Defines a static class for mapping turn-related endpoints.
public static class TurnsEndpoints
{
  // Extension method to register all turn endpoints.
  public static IEndpointRouteBuilder MapTurnsEndpoints(this IEndpointRouteBuilder app)
  {
    // Create a route group for game-session related turn endpoints.
    var turnsEndpointsGroup = app.MapGroup("/api/turn").WithTags("Turns");

    turnsEndpointsGroup.MapGet(       // Register the endpoint for reading the current active turn
      "/{gameSessionId:guid}/turn",        // Route for getting the current active turn by game session id.

      // Define the endpoint handler and its possible response types.
      async Task<Results<Ok<TurnDto>, NotFound<string>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Get the active turn for the given game session from the repository.
        var turn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

        // Check if no active turn exist
        if (turn is null)
        {
          // Returns 404 if no active turn exists.
          return TypedResults.NotFound("No active turn found for this game session.");
        }

        // Otherwise return 200 with the active turn.
        return TypedResults.Ok(turn);
      }
    ).WithDescription("Fetch current turn for a game session").WithSummary("Get turn for a game session");

    // Register the endpoint for reading all turns in one game session.
    turnsEndpointsGroup.MapGet(
      "/{gameSessionId:guid}/turns", // Route for getting all turns by game session id.
      async Task<Ok<IEnumerable<TurnDto>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        var turns = await repo.GetTurnsByGameSessionIdAsync(gameSessionId, ct); // Gets all turns for the game session.
        return TypedResults.Ok(turns);
        }
      ).WithDescription("This route is for getting all turns in a game session.").WithSummary("Get all turns");

    turnsEndpointsGroup.MapPost(
      "/{gameSessionId}/turn/start",      // Route for creating/starting the very first turn when a game starts.
      async Task<Results<Ok<TurnDto>, BadRequest<string>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Check whether an active turn already exists.
        var existingTurn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

        if (existingTurn is not null)
        {
          return TypedResults.BadRequest("An active turn already exists for this game session.");
        }

        // Get the first player in turn order.
        var firstPlayerId = await repo.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, ct);

        if (firstPlayerId is null)
        {
          return TypedResults.BadRequest("Cannot create first turn because no players were found in this game session.");
        }

        // Creates the first turn in build phase and round 1, if there is no existing turn.
        var createdTurn = await repo.CreateTurnAsync(
            new CreateTurnDto(
                Round: 1,
                Phase: "build",
                Status: "active", // First turn starts as active.
                GameSessionId: gameSessionId.ToString(),
                PlayerId: firstPlayerId.Value
            ),
            ct
        );
        // Return the created turn .
        return TypedResults.Ok(createdTurn);
      }
    ).WithDescription("Route for creating the very first turn when a game starts").WithSummary("Create first turn at game start");

    // Register the endpoint for changing the phase of the active turn.
    turnsEndpointsGroup.MapPatch(
      "/{gameSessionId:guid}/turn/phase",
      async Task<Results<Ok<string>, BadRequest<string>>> (Guid gameSessionId, ChangeTurnPhaseRequest request, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Define the valid phases allowed
        var allowedPhases = new[] { "build", "assigned", "attack", "reinforce" };

        if (!allowedPhases.Contains(request.Phase)) // Check whether the requested phase is valid.
        {
          return TypedResults.BadRequest("Invalid phase value.");
        }

        // Update the phase of the active turn.
        var updated = await repo.ChangeCurrentTurnPhaseAsync(gameSessionId, request.Phase, ct);

        // Check whether no turn was updated.
        if (!updated)
        {
          return TypedResults.BadRequest("Could not change phase because no active turn was found.");
        }

        return TypedResults.Ok($"Turn phase changed to {request.Phase}.");
      }
    ).WithDescription("Route for changing active turn phase").WithSummary("Update active turn phase");
    
    // Route for ending the current active turn and creating the next one.
    turnsEndpointsGroup.MapPost(
      "/{gameSessionId}/turn/end",   // Route for ending the current turn
      async Task<Results<Ok<TurnDto>, BadRequest<string>, NotFound<string>>> (Guid gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        var currentTurn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);   // Get the current turn.

        // Check whether no turn exists.
        if (currentTurn is null)
        {
          return TypedResults.NotFound("No current turn found for this game session.");
        }

        var nextPlayerId = await repo.GetNextPlayerIdAsync(gameSessionId, currentTurn.PlayerId, ct);    // Find the next player based on turn order.

        // Check whether no next player could be found.
        if (nextPlayerId is null)
        {
          return TypedResults.BadRequest("Could not determine the next player.");
        }

        var currentRound = await repo.GetCurrentRoundAsync(gameSessionId, ct) ?? 1;   // Get or read the current round number, if none exist default to round 1 
        var firstPlayerId = await repo.GetFirstPlayerIdByGameSessionIdAsync(gameSessionId, ct);   // Get first player in turn order

        // Check whether no players exist.
        if (firstPlayerId is null)
        {
          return TypedResults.BadRequest("No players found in this game session.");
        }

        var nextRound = currentRound;   // start next round value as current round

        // if the turn wraps back to the first player...
        if (nextPlayerId.Value == firstPlayerId.Value)
        {
          nextRound += 1;     // Increase round number.
        }

        // Mark the previous turn as inactive.
        var oldTurnUpdated = await repo.SetTurnStatusAsync(currentTurn.Id, "inactive", ct);

        // If the previous turn status could not be updated...
        if (!oldTurnUpdated)
        {
          return TypedResults.BadRequest("Could not deactivate the current turn.");
        }

        // Create the next active turn.
        var createdTurn = await repo.CreateTurnAsync(
            new CreateTurnDto(
                Round: nextRound,                         // Use the calculated next round.
                Phase: "build",                           // Reset the next turn to build phase.
                Status: "active",                         // New turn starts as active
                GameSessionId: gameSessionId.ToString(),             // Use the provided game session id.
                PlayerId: nextPlayerId.Value              // Use the resolved next player's id.
            ),
            ct
        );

        return TypedResults.Ok(createdTurn);
      }
    ).WithDescription("Route for ending the current active turn and creating the next one").WithSummary("End current active turn and Create next one");
    // Return the route builder so more endpoint groups can be chained.
    return app;

  }
}