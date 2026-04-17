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

    turnsEndpointsGroup.MapGet(       // Maps an HTTP GET endpoint.
      "/{gameSessionId}/turn",        // Route for fetching the current turn in a game session.

      // Define the endpoint handler and its possible response types.
      async Task<Results<Ok<TurnDto>, NotFound<string>>> (string gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Fetch the current turn for the given game session.
        var turn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

        if (turn is null)
        {
          // Returns 404 if no turn exists.
          return TypedResults.NotFound("No turn found for this game session.");
        }

        // Otherwise return the current turn as 200 OK.
        return TypedResults.Ok(turn);
      }
    ).WithDescription("Fetch current turn for a game session").WithSummary("Get turn for a game session");


    turnsEndpointsGroup.MapPost(
      "/{gameSessionId}/turn/start",      // Route for creating the very first turn when a game starts.
      async Task<Results<Ok<TurnDto>, BadRequest<string>>> (string gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
      {
        // Check whether a turn already exists.
        var existingTurn = await repo.GetCurrentTurnByGameSessionIdAsync(gameSessionId, ct);

        if (existingTurn is not null)
        {
          return TypedResults.BadRequest("A turn already exists for this game session.");
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
                GameSessionId: gameSessionId,
                PlayerId: firstPlayerId.Value
            ),
            ct
        );
        // Return the created first turn as 200 OK.
        return TypedResults.Ok(createdTurn);
      }
    ).WithDescription("Route for creating the very first turn when a game starts").WithSummary("Create first turn at game start");


    turnsEndpointsGroup.MapPost(
      "/{gameSessionId}/turn/end",    // Route for ending the current turn and creating the next one.
      async Task<Results<Ok<TurnDto>, BadRequest<string>, NotFound<string>>> (string gameSessionId, ITurnsRepository repo, CancellationToken ct) =>
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

        var nextRound = currentRound;   // Next round becomes the current round, where no round already exists

        // Check if the turn order wrapped back to the first player.
        if (nextPlayerId.Value == firstPlayerId.Value)
        {
          nextRound += 1;     // Increase round number when a full cycle is completed.
        }

        // Create the next turn.
        var createdTurn = await repo.CreateTurnAsync(
            new CreateTurnDto(
                Round: nextRound,                         // Use the calculated next round.
                Phase: "build",                           // Reset the next turn to build phase.
                GameSessionId: gameSessionId,             // Use the provided game session id.
                PlayerId: nextPlayerId.Value              // Use the resolved next player's id.
            ),
            ct
        );

        return TypedResults.Ok(createdTurn);
      }
    ).WithDescription("Route for ending the current turn and creating the next one").WithSummary("End current turn and Create next one");
    // Return the route builder so more endpoint groups can be chained.
    return app;

  }
}