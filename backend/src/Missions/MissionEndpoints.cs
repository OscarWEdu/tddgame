using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

public static class MissionEndpoints
{
  public static IEndpointRouteBuilder MapMissionEndpoints(this IEndpointRouteBuilder app)
  {
    var missionGroup = app.MapGroup("/api/missions").WithTags("Missions");

    missionGroup.MapGet(
        "/",
        async Task<Results<Ok<IEnumerable<MissionDto>>, NotFound<string>>> (
            IMissionsRepository repo,
            CancellationToken ct) =>
        {
          var missions = await repo.GetMissionsAsync(ct);

          if (missions is null)
          {
            return TypedResults.NotFound("No missions found.");
          }

          return TypedResults.Ok(missions);
        })
        .WithSummary("Get all missions")
        .WithDescription("Fetch all missions.");

    missionGroup.MapPost(
        "/",
        async Task<Results<Ok<MissionDto>, BadRequest<string>>> (
            CreateMissionDto mission,
            IMissionsRepository repo,
            CancellationToken ct) =>
        {
          var createdMission = await repo.CreateMissionAsync(mission, ct);

          if (createdMission is null)
          {
            return TypedResults.BadRequest("Could not create mission.");
          }

          return TypedResults.Ok(createdMission);
        })
        .WithSummary("Create mission")
        .WithDescription("Create a new mission.");

    missionGroup.MapGet(
        "/{playerId}",
        async Task<Results<Ok<MissionDto>, NotFound<string>>> (
            int playerId,

            IMissionsRepository repo,
            CancellationToken ct) =>
        {
          var nextMission = await repo.GetMissionByPlayerIdAsync(playerId, ct);

          if (nextMission is null)
          {
            return TypedResults.NotFound("Mission not found.");
          }

          return TypedResults.Ok(nextMission);
        })
        .WithSummary("Get mission by player id")
        .WithDescription("Fetch mission using mission id and current mission.");

    return app;
  }
}