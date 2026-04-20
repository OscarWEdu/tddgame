// Contains main logic of the game

using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

public static class GameSessionsEndpoint
{
    public static IEndpointRouteBuilder MapGameSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var gameSessionEndpointsGroup = app.MapGroup("/api/game-session").WithTags("GameSessions");

        gameSessionEndpointsGroup.MapGet(
            "/",
            async Task<Ok<IEnumerable<GameSessionDto>>> (IGameSessionsRepository repo, CancellationToken ct)
            =>
            {
                var gameSessions = await repo.GetGameSessionsAsync(ct);
                return TypedResults.Ok(gameSessions);
            }
        ).WithSummary("Get all game sessions").WithDescription("Return all game sessions.");

        gameSessionEndpointsGroup.MapGet(
            "/{id}",
            async Task<Results<Ok<GameSessionDto>, NotFound<string>, BadRequest<string>>> (IGameSessionsRepository repo, Guid id, CancellationToken ct)
            =>
            {
                if (!Guid.TryParse(id.ToString(), out var guid))
                    return TypedResults.BadRequest($"Invalid id format {guid}. Game session id should be UUID");
                var gameSession = await repo.GetGameSessionByIdAsync(id, ct);
                if (gameSession is null)
                    return TypedResults.NotFound("Game session cna't be found with this id.");

                return TypedResults.Ok(gameSession);
            }
        ).WithSummary("Get game session by id").WithDescription("Return game session by id or not found.");

        gameSessionEndpointsGroup.MapPost(
            "/",
            async Task<Results<Created<GameSessionDto>, BadRequest<string>>> (IGameSessionsRepository repo, CreateGameSessionRequest request, CancellationToken ct)
            =>
            {
                if (request.Name == null || request.Name.Trim() == "")
                    return TypedResults.BadRequest("Name can not be empty or null");

                if (request.Name.Trim().Length > 100)
                    return TypedResults.BadRequest("Name can not be longer than 100 symbols");

                var gameSession = await repo.CreateGameSessionAsync(request.Name, ct);

                return TypedResults.Created($"/api/game-session/{gameSession.Id}", gameSession);
            }
        ).WithSummary("Create new game session").WithDescription("Returns new game session data.");

        gameSessionEndpointsGroup.MapPatch(
            "/{id}/status",
            async Task<Results<Ok, NotFound<string>, BadRequest<string>>> (IGameSessionsRepository repo, Guid id, UpdateGameSessionStatusRequest request, CancellationToken ct)
            =>
            {
                if (request.Status.ToString().Trim() == "")
                    return TypedResults.BadRequest("Status must not be empty");

                if (!Enum.IsDefined(typeof(GameSessionStatus), request.Status))
                    return TypedResults.BadRequest("Status must be one of allowed values: lobby, started, finished");



                var isUpdated = await repo.UpdateGameSessionStatusAsync(id, request.Status.ToString(), ct);

                return isUpdated ? TypedResults.Ok() : TypedResults.NotFound("Game session does not exists");
            }
        ).WithSummary("Update game session status").WithDescription("Changing the game session status");

        gameSessionEndpointsGroup.MapDelete(
            "/{id}",
            async Task<Results<NoContent, NotFound>> (IGameSessionsRepository repo, Guid id, CancellationToken ct)
            =>
            {
                var isDeleted = await repo.DeleteGameSessionAsync(id, ct);

                return isDeleted ? TypedResults.NoContent() : TypedResults.NotFound();
            }
        ).WithSummary("Delete game session by id").WithDescription("Removes game session form the list");

        return app;
    }
}
