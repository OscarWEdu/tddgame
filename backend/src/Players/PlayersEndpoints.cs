// Defines the HTTP API routes related to players in a game session.
// Example:
// POST /api/game-session/{gameSession_id}/players
// GET /api/game-session/{gameSession_id}/players
// GET /api/players/{id}
// Handles incoming requests and sends responses.
// Calls repository or service methods to perform player-related actions.
using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

public static class PlayersEndPoints
{
    public static void MapPlayersEndpoints(this IEndpointRouteBuilder app)
    {
        var playersEndpointsGroup = app.MapGroup("/api/game-session/{gameSessionId}/players").WithTags("Players");
        var playerGroup = app.MapGroup("/api/players").WithTags("Players");

        //GET /api/game-session/{gameSession_id}/players
        playersEndpointsGroup.MapGet(
           "/",
           async Task<Ok<IEnumerable<PlayerDto>>> (int gameSessionId, IPlayersRepository repo, CancellationToken ct)
              =>
           {
               var player = await repo.GetPlayersByGameSessionAsync(gameSessionId, ct);
               return TypedResults.Ok(player);
           }
        ).WithSummary("Get players in a game session").WithDescription("Return all players in a specific game session.");
    }
}

