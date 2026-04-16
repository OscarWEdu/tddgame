namespace TddGame;

using Microsoft.AspNetCore.Http.HttpResults;

public static class ContinentEndpoints
{
    public static IEndpointRouteBuilder MapContinentEndpoints(this IEndpointRouteBuilder app)
    {
        var ContinentEndpointsGroup = app.MapGroup("/api/continents").WithTags("Continents");

        ContinentEndpointsGroup.MapGet(
            "/",
            async Task<Ok<IEnumerable<ContinentDto>>> (IContinentRepository repo, CancellationToken ct) =>
            {
                var continents = await repo.GetContinentsAsync(ct);
                return TypedResults.Ok(continents);
            }

        ).WithSummary("Get all continents").WithDescription("Return all continents.");

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