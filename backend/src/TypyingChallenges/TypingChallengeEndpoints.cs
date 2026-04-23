using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

// Defines the HTTP API routes related to typing challenges.
public static class TypingChallengesEndpoints
{
  public static IEndpointRouteBuilder MapTypingChallengesEndpoints(this IEndpointRouteBuilder app)
  {
    var typingChallengeGroup = app.MapGroup("/api/typing-challenges").WithTags("TypingChallenges");

    // GET /api/typing-challenges/{typingChallengeId}
    typingChallengeGroup.MapGet(
        "/{typingChallengeId:int}",
        async Task<Results<Ok<TypingChallengeDto>, NotFound>> (int id, ITypingChallengesRepository repo, CancellationToken ct)
        =>
        {
          var typingChallenge = await repo.GetTypingChallengeByIdAsync(id, ct);
          return typingChallenge is not null ? TypedResults.Ok(typingChallenge) : TypedResults.NotFound();
        }
    ).WithSummary("Get typing challenge by id")
     .WithDescription("Return one specific typing challenge by id.");

    // GET /api/typing-challenges?battleId={battleId}
    typingChallengeGroup.MapGet(
        "/",
        async Task<Results<Ok<TypingChallengeDto>, NotFound>> (int battleId, ITypingChallengesRepository repo, CancellationToken ct)
        =>
        {
          var typingChallenge = await repo.GetTypingChallengeByBattleIdAsync(battleId, ct);
          return typingChallenge is not null ? TypedResults.Ok(typingChallenge) : TypedResults.NotFound();
        }
    ).WithSummary("Get typing challenge for a battle")
     .WithDescription("Return the typing challenge connected to a specific battle.");

    // POST /api/typing-challenges
    typingChallengeGroup.MapPost(
        "/",
        async Task<Results<Created<TypingChallengeDto>, BadRequest<string>, NotFound<string>, Conflict<string>>> (
            CreateTypingChallengeDto request,
            ITypingChallengesRepository typingChallengesRepo,
            IBattlesRepository battlesRepo,
            CancellationToken ct)
        =>
        {
          if (request.BattleId <= 0)
          {
            return TypedResults.BadRequest("BattleId must be greater than 0.");
          }

          if (string.IsNullOrWhiteSpace(request.PromptText))
          {
            return TypedResults.BadRequest("PromptText can not be empty.");
          }

          if (request.PromptText.Trim().Length > 500)
          {
            return TypedResults.BadRequest("PromptText can not be longer than 500 characters.");
          }

          var battle = await battlesRepo.GetBattleByIdAsync(request.BattleId, ct);

          if (battle is null)
          {
            return TypedResults.NotFound("Battle not found.");
          }

          var existingTypingChallenge = await typingChallengesRepo.GetTypingChallengeByBattleIdAsync(request.BattleId, ct);

          if (existingTypingChallenge is not null)
          {
            return TypedResults.Conflict("A typing challenge already exists for this battle.");
          }

          var createdTypingChallenge = await typingChallengesRepo.CreateTypingChallengeAsync(
                  new CreateTypingChallengeDto(
                      BattleId: request.BattleId,
                      PromptText: request.PromptText.Trim()
                  ),
                  ct
              );

          return TypedResults.Created($"/api/typing-challenges/{createdTypingChallenge.Id}", createdTypingChallenge);
        }
    ).WithSummary("Create typing challenge")
     .WithDescription("Create a new typing challenge for a specific battle.");

    return app;
  }
}