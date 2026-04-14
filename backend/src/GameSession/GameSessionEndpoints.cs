// Contains main logic of the game

using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

public static class GameSessionsEndpoint
{
    public static IEndpointRouteBuilder MapGameSessionEndpoints(this IEndpointRouteBuilder app)
    {
        // http://
        var gameSessionEndpointsGroup = app.MapGroup("/api/game-session");

        gameSessionEndpointsGroup.MapGet(
            "/",
            async Task<Ok<IEnumerable<GameSessionDto>>> (IGameSessionsRepository repo, CancellationToken ct)
            =>
            {
                var gameSessions = await repo.GetGameSessionsAsync(ct);
                return TypedResults.Ok(gameSessions);
            }
        );

        return app;
    }
}
