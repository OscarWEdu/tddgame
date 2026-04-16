namespace TddGame;

using Microsoft.AspNetCore.Http.HttpResults;

public static class TerritoryEndpoints
{
    public static IEndpointRouteBuilder MapTerritoryEndpoints(this IEndpointRouteBuilder app)
    {
        var territoryEndpointsGroup = app.MapGroup("/api/territories").WithTags("Territories");

        territoryEndpointsGroup.MapGet(
            "/",
            async Task<Ok<IEnumerable<TerritoryDto>>> (ITerritoryRepository repo, CancellationToken ct)
            =>
            {
                var territories = await repo.GetTerritoriesAsync(ct);
                return TypedResults.Ok(territories);
            }
        ).WithSummary("Get all territories").WithDescription("Return all territories.");

        return app;
    }
}