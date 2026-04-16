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

        return app;
    }
}