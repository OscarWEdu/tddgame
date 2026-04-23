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
    public static IEndpointRouteBuilder MapPlayersEndpoints(this IEndpointRouteBuilder app)
    {

        var playerGroup = app.MapGroup("/api/players").WithTags("Players");

        //GET /api/players?gameSessionId={gameSessionId}
        playerGroup.MapGet(
           "/",
           async Task<Ok<IEnumerable<PlayerDto>>> (string gameSessionId, IPlayersRepository repo, CancellationToken ct)
              =>
           {
               var player = await repo.GetPlayersByGameSessionAsync(gameSessionId, ct);
               return TypedResults.Ok(player);
           }
        ).WithSummary("Get players in a game session").WithDescription("Return all players in a specific game session.");

        //POST /api/players?gameSessionId={gameSessionId}
        playerGroup.MapPost(
            "/",
            async Task<Results<Created<PlayerDto>, NotFound<string>, BadRequest<string>, Conflict<string>>> (string gameSessionId, CreatePlayerDto player, IPlayersRepository playerRepo, IGameSessionsRepository sessionRepo, CancellationToken ct)
            =>
            {
                if (!Guid.TryParse(gameSessionId, out var guid))
                    return TypedResults.BadRequest("Invalid gameSessionId format. Must be a UUID.");

                var session = await sessionRepo.GetGameSessionByIdAsync(guid, ct);
                if (session is null)
                    return TypedResults.NotFound("Game session not found.");

                if (session.Status != GameSessionStatus.lobby)
                    return TypedResults.Conflict("Cannot join a game session that has already started or completed.");

                var existingPlayers = (await playerRepo.GetPlayersByGameSessionAsync(gameSessionId, ct)).ToList();
                if (existingPlayers.Count >= session.MaxPlayers)
                    return TypedResults.Conflict($"Lobby is full ({session.MaxPlayers}/{session.MaxPlayers} players).");

                var isHost = existingPlayers.Count == 0;
                var createdPlayer = await playerRepo.AddPlayerToGameAsync(gameSessionId, player, isHost, ct);
                return TypedResults.Created($"/api/players/{createdPlayer.id}", createdPlayer);
            }
            ).WithSummary("Add a player to a game session").WithDescription("Create a new player in a specific game session.");

            //GET /api/players/{id}
            playerGroup.MapGet(
            "/{id:int}",
            async Task<Results<Ok<PlayerDto>, NotFound>> (int id, IPlayersRepository repo, CancellationToken ct)
            =>
            {
                var player = await repo.GetPlayerByIdAsync(id, ct);
                return player is not null ? TypedResults.Ok(player) : TypedResults.NotFound();
            }
            ).WithSummary("Get player by id").WithDescription("Return a specific player by their id.");

        //PATCH /api/players/{id}
        playerGroup.MapPatch(
        "/{id:int}",
        async Task<Results<Ok<PlayerDto>, NotFound>> (int id, PlayerStateDto state, IPlayersRepository repo, CancellationToken ct)
        =>
        {
            var updatedPlayer = await repo.UpdatePlayerAsync(id, state, ct);

            return updatedPlayer is not null ? TypedResults.Ok(updatedPlayer) : TypedResults.NotFound();
        }

        ).WithSummary("Update player state").WithDescription("Update a players numGold and isDead by their id");

        //DELETE /api/players/{id}  
        playerGroup.MapDelete(
            "/{id:int}",
            async Task<Results<NoContent, NotFound>> (int id, IPlayersRepository repo, CancellationToken ct)
            =>
            {
                var deleted = await repo.DeletePlayerAsync(id, ct);
                return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
            }
            ).WithSummary("Delete player by id").WithDescription("Delete a specific player by their id.");

            return app;
    }
}

