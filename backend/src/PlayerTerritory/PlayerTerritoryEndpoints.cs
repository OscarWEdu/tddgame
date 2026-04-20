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

        return app;
    }
}