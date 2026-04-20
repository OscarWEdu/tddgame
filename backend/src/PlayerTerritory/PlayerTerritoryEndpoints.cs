namespace TddGame;

using Microsoft.AspNetCore.Http.HttpResults;

public static class PlayerTerritoryEndpoints
{
    public static IEndpointRouteBuilder MapPlayerTerritoryEndpoints(this IEndpointRouteBuilder app)
    {
        var playerTerritoryEndpointsGroup = app.MapGroup("/api/playerterritories").WithTags("PlayerTerritories");

        //Get all PlayerTerritories
        playerTerritoryEndpointsGroup.MapGet(
            "/",
            async Task<Ok<IEnumerable<PlayerTerritoryDto>>> (IPlayerTerritoryRepository repo, CancellationToken ct)
            =>
            {
                var playerTerritories = await repo.GetPlayerTerritoriesAsync(ct);
                return TypedResults.Ok(playerTerritories);
            }
        ).WithSummary("Get all playerTerritories").WithDescription("Return all playerTerritories.");

        //Get all owned PlayerTerritories by playerId
        playerTerritoryEndpointsGroup.MapGet(
            "/player/{playerId}",
            async Task<Ok<IEnumerable<PlayerTerritoryDto>>> (IPlayerTerritoryRepository repo, int playerId, CancellationToken ct)
            =>
            {
                var playerTerritories = await repo.GetPlayerPlayerTerritoriesAsync(playerId, ct);
                
                return TypedResults.Ok(playerTerritories);
            }
        ).WithSummary("Get all owned playerTerritories by player").WithDescription("Return all owned playerTerritories by player.");

        //Get PlayerTerritory by id
        playerTerritoryEndpointsGroup.MapGet(
            "/{id}",
            async Task<Results<Ok<PlayerTerritoryDto>, NotFound>> (IPlayerTerritoryRepository repo, int id, CancellationToken ct) =>
            {
                var playerTerritory = await repo.GetPlayerTerritoryByIdAsync(id, ct);
                if (playerTerritory is null){
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(playerTerritory);
            }

        ).WithSummary("Get playerTerritory by id").WithDescription("Return playerTerritory by id, or not found.");

        // Get PlayerTerritory by territoryid and gameSessionId
        playerTerritoryEndpointsGroup.MapGet(
            "/{gameSessionId}/{territoryId}",
            async Task<Results<Ok<PlayerTerritoryDto>, NotFound>> (IPlayerTerritoryRepository repo, IPlayersRepository playerRepo, string gameSessionId, int territoryId, CancellationToken ct) =>
            {
                var players = await playerRepo.GetPlayersByGameSessionAsync(gameSessionId, ct);

                foreach (var player in players)
                {
                    var playerTerritories = await repo.GetPlayerPlayerTerritoriesAsync(player.id, ct);
                    foreach (PlayerTerritoryDto? playerTerritory in playerTerritories)
                    {
                        if (playerTerritory is not null) { return TypedResults.Ok(playerTerritory); }
                    }
                }
                return TypedResults.NotFound();
            }

        ).WithSummary("Get playerterritory by territoryid and gameSessionId").WithDescription("Return playerterritory by territoryId by a gameSessionId, or not found.");

        //Add new playerTerritory
        playerTerritoryEndpointsGroup.MapPost(
            "/",
            async Task<Created<PlayerTerritoryDto>> (IPlayerTerritoryRepository repo, CreatePlayerTerritoryRequest request, CancellationToken ct) =>
            {
                var playerTerritory = await repo.CreatePlayerTerritoryAsync(request.PlayerId, request.TerritoryId, ct);

                return TypedResults.Created($"/api/playerterritories/{playerTerritory.Id}", playerTerritory);
            }
        ).WithSummary("Create new playerTerritory").WithDescription("Returns newly created playerTerritory.");

        playerTerritoryEndpointsGroup.MapDelete(
            "/{id}",
            async Task<Results<NoContent, NotFound>> (IPlayerTerritoryRepository repo, int id, CancellationToken ct) =>
            {
                var isDeleted = await repo.DeletePlayerTerritoryAsync(id, ct);

                return isDeleted ? TypedResults.NoContent() : TypedResults.NotFound();
            }
        ).WithSummary("Delete playerTerritory by id").WithDescription("Removes playerTerritory from the table");

        playerTerritoryEndpointsGroup.MapPatch(
            "/{id}/troops/{troopNum}",
            async Task<Results<Ok, NotFound<string>>> (IPlayerTerritoryRepository repo, int id, int troopNum, CancellationToken ct) =>
            {
                var isUpdated = await repo.UpdatePlayerTerritoryTroopsAsync(id, troopNum, ct);

                return isUpdated ? TypedResults.Ok() : TypedResults.NotFound("PlayerTerritory does not exists");
            }
        ).WithSummary("Update playerTerritory troops").WithDescription("Change the playerTerritory troop count");

        playerTerritoryEndpointsGroup.MapPatch(
            "/{id}/city/{hasCity}",
            async Task<Results<Ok, NotFound<string>>> (IPlayerTerritoryRepository repo, int id, bool hasCity, CancellationToken ct) =>
            {
                var isUpdated = await repo.UpdatePlayerTerritoryCityAsync(id, hasCity, ct);

                return isUpdated ? TypedResults.Ok() : TypedResults.NotFound("PlayerTerritory does not exists");
            }
        ).WithSummary("Update playerTerritory hasCity").WithDescription("Change the playerTerritory hasCity bool");

        return app;
    }
}