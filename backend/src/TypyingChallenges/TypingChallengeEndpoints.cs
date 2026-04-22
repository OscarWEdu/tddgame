using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

public static class TypingChallengesEndpoints
{
  public static IEndpointRouteBuilder MapTypingChallengesEndpoints(this IEndpointRouteBuilder app)
  {
    var typingChallengesGroup = app.MapGroup("/api/typing-challenges").WithTags("TypingChallenges");

    // GET /api/typing-challenges/{id}
    // Get one specific typing challenge by id
    typingChallengesGroup.MapGet(
        "/{id:int}",
        async Task<Results<Ok<TypingChallengeDto>, NotFound<string>>> (
            int id,
            ITypingChallengesRepository repo,
            CancellationToken ct) =>
        {
          var typingChallenge = await repo.GetTypingChallengeByIdAsync(id, ct);

          return typingChallenge is not null
                  ? TypedResults.Ok(typingChallenge)
                  : TypedResults.NotFound("Typing challenge not found.");
        }
    ).WithSummary("Get one typing challenge").WithDescription("Get one specific typing challenge by id");

    // GET /api/typing-challenges/battle/{battleId}
    // Get typing challenge belonging to a particular battle by id
    typingChallengesGroup.MapGet(
        "/battle/{battleId:int}",
        async Task<Results<Ok<TypingChallengeDto>, NotFound<string>>> (
            int battleId,
            ITypingChallengesRepository repo,
            CancellationToken ct) =>
        {
          var typingChallenge = await repo.GetTypingChallengeByBattleIdAsync(battleId, ct);

          return typingChallenge is not null
                  ? TypedResults.Ok(typingChallenge)
                  : TypedResults.NotFound("Typing challenge for this battle was not found.");
        }
    ).WithSummary("Get typing challenge for a battle").WithDescription("Get typing challenge belonging to a particular battle by id");

    // POST /api/typing-challenges
    // Create new typing challenge for a battle
    typingChallengesGroup.MapPost(
        "/",
        async Task<Results<Created<TypingChallengeDto>, BadRequest<string>, NotFound<string>, Conflict<string>>> (
            CreateTypingChallengeDto request,
            ITypingChallengesRepository typingChallengesRepo,
            IBattlesRepository battlesRepo,
            CancellationToken ct) =>
        {
          if (request.BattleId <= 0)
            return TypedResults.BadRequest("BattleId must be greater than 0.");

          if (string.IsNullOrWhiteSpace(request.PromptText))
            return TypedResults.BadRequest("PromptText can not be empty.");

          if (request.PromptText.Trim().Length > 500)
            return TypedResults.BadRequest("PromptText can not be longer than 500 characters.");

          // Make sure the battle exists before attaching a typing challenge to it.
          var battle = await battlesRepo.GetBattleByIdAsync(request.BattleId, ct);
          if (battle is null)
            return TypedResults.NotFound("Battle not found.");

          // Only one typing challenge should exist for one battle in this first slice.
          var existingTypingChallenge = await typingChallengesRepo.GetTypingChallengeByBattleIdAsync(request.BattleId, ct);
          if (existingTypingChallenge is not null)
            return TypedResults.Conflict("A typing challenge already exists for this battle.");

          var typingChallenge = await typingChallengesRepo.CreateTypingChallengeAsync(
                  new CreateTypingChallengeDto(
                      BattleId: request.BattleId,
                      PromptText: request.PromptText.Trim()
                  ),
                  ct
              );

          return TypedResults.Created($"/api/typing-challenges/{typingChallenge.Id}", typingChallenge);
        }
    ).WithSummary("Creates typing challenge").WithDescription("Create new typing challenge for a battle");

    return app;
  }
}