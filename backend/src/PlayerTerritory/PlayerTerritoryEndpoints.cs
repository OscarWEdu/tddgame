namespace TddGame;

using Microsoft.AspNetCore.Http.HttpResults;

public static class PlayerTerritoryEndpoints
{
    public static IEndpointRouteBuilder MapPlayerTerritoryEndpoints(this IEndpointRouteBuilder app)
    {
        var playerTerritoryEndpointsGroup = app.MapGroup("/api/playerterritories").WithTags("PlayerTerritories");

        playerTerritoryEndpointsGroup.MapGet(
            "/",
            async Task<Ok<IEnumerable<PlayerTerritoryDto>>> (IPlayerTerritoryRepository repo, CancellationToken ct)
            =>
            {
                var playerTerritories = await repo.GetPlayerTerritoriesAsync(ct);
                return TypedResults.Ok(playerTerritories);
            }
        ).WithSummary("Get all playerterritories").WithDescription("Return all playerterritories.");

        return app;
    }
}