namespace TddGame;

using Microsoft.AspNetCore.Http.HttpResults;

public static class ContinentEndpoints
{
    public static IEndpointRouteBuilder MapContinentEndpoints(this IEndpointRouteBuilder app)
    {
        var ContinentEndpointsGroup = app.MapGroup("/api/continents").WithTags("Continents");

        //Get all Continents
        ContinentEndpointsGroup.MapGet(
            "/",
            async Task<Ok<IEnumerable<ContinentDto>>> (IContinentRepository repo, CancellationToken ct) =>
            {
                var continents = await repo.GetContinentsAsync(ct);
                return TypedResults.Ok(continents);
            }

        ).WithSummary("Get all continents").WithDescription("Return all continents.");

        //Get Continent by id
        ContinentEndpointsGroup.MapGet(
            "/{id}",
            async Task<Results<Ok<ContinentDto>, NotFound>> (IContinentRepository repo, int id, CancellationToken ct) =>
            {
                var continent = await repo.GetContinentByIdAsync(id, ct);
                if (continent is null){
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(continent);
            }

        ).WithSummary("Get continent by id").WithDescription("Return continent by id or not found.");

        //Insert a new continent
        ContinentEndpointsGroup.MapPost(
            "/",
            async Task<Created<ContinentDto>> (IContinentRepository repo, CreateContinentRequest request, CancellationToken ct) =>
            {
                var continent = await repo.CreateContinentAsync(request.name, request.bonusConst, ct);

                return TypedResults.Created($"/api/continents/{continent.Id}", continent);
            }
        ).WithSummary("Create new coninent").WithDescription("Returns newly added continent data.");

        return app;
    }
}