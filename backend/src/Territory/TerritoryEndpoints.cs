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

        //Get Continent by id
        territoryEndpointsGroup.MapGet(
            "/{id}",
            async Task<Results<Ok<TerritoryDto>, NotFound>> (ITerritoryRepository repo, int id, CancellationToken ct) =>
            {
                var territory = await repo.GetTerritoryByIdAsync(id, ct);
                if (territory is null){
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(territory);
            }

        ).WithSummary("Get territory by id").WithDescription("Return territory by id, or not found.");

        territoryEndpointsGroup.MapPost(
            "/",
            async Task<Created<TerritoryDto>> (ITerritoryRepository repo, CreateTerritoryRequest request, CancellationToken ct) =>
            {
                var territory = await repo.CreateTerritoryAsync(request.Name, request.NorthAdjacentId, request.SouthAdjacentId, request.SouthAdjacentId, request.EastAdjacentId, request.ContinentId, ct);

                return TypedResults.Created($"/api/territories/{territory.Id}", territory);
            }
        ).WithSummary("Create new territory").WithDescription("Returns new game session data.");

        return app;
    }
}