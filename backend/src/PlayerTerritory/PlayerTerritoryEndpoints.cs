namespace TddGame;

using Microsoft.AspNetCore.Http.HttpResults;

public static class PlayerTerritoryEndpoints
{
    public static IEndpointRouteBuilder MapPlayerTerritoryEndpoints(this IEndpointRouteBuilder app)
    {
        var playerTerritoryEndpointsGroup = app.MapGroup("/api/playerterritories").WithTags("PlayerTerritories");

        return app;
    }
}