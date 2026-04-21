// - submit attacker and defender typing data
// - calculate attacker and defender score
// - defender wins ties
// - calculate troop loss (1 or 2)
// - save the final resolved result row

namespace TddGame;

public static class ResultEndpoints
{
  public static IEndpointRouteBuilder MapResultEndpoints(this IEndpointRouteBuilder app)
  {
    var resultEndpointGroup = app.MapGroup("/api/results").WithTags("Results");

    // Get one result by id.
    resultEndpointGroup.MapGet("/{id:int}", async (int id, IResultsRepository repository, CancellationToken ct) =>
    {
      var result = await repository.GetResultByIdAsync(id, ct);
      return result is null ? Results.NotFound() : Results.Ok(result);
    }).WithSummary("Get one result").WithDescription("Get one result by id");

    // Get the result connected to one battle.
    resultEndpointGroup.MapGet("/battle/{battleId:int}", async (int battleId, IResultsRepository repository, CancellationToken ct) =>
    {
      var result = await repository.GetResultByBattleIdAsync(battleId, ct);
      return result is null ? Results.NotFound() : Results.Ok(result);
    }).WithSummary("Get a battle's result").WithDescription("Get the result connected to one particular battle");

    // Create a resolved result for one battle using both players' typing data.
    resultEndpointGroup.MapPost("/battle/{battleId:int}", async (int battleId, CreateResultRequest request, IResultsRepository repository, CancellationToken ct) =>
    {
      // Do not allow duplicate resolved results for the same battle.
      var existingResult = await repository.GetResultByBattleIdAsync(battleId, ct);
      if (existingResult is not null)
      {
        return Results.Conflict(new { message = "Result already exists for this battle." });
      }

      // The battle must exist before a result can be saved.
      var battle = await repository.GetBattleForResultAsync(battleId, ct);
      if (battle is null)
      {
        return Results.NotFound(new { message = "Battle not found." });
      }

      // A typing challenge must already exist for the battle.
      var challengeExists = await repository.TypingChallengeExistsForBattleAsync(battleId, ct);
      if (!challengeExists)
      {
        return Results.BadRequest(new { message = "Typing challenge must exist before creating a result." });
      }

      // Input validation.
      if (request.AttackerWpm < 0 || request.DefenderWpm < 0)
      {
        return Results.BadRequest(new { message = "WPM cannot be negative." });
      }

      if (request.AttackerMistakes < 0 || request.DefenderMistakes < 0)
      {
        return Results.BadRequest(new { message = "Mistakes cannot be negative." });
      }

      // Calculate both scores.
      // The defender gets a small defense bonus.
      const int mistakeConst = 2;
      const int defenceConst = 5;

      var attackerScore = CalculateScore(request.AttackerWpm, request.AttackerMistakes, request.AttackerCompleted, mistakeConst, 0);
      var defenderScore = CalculateScore(request.DefenderWpm, request.DefenderMistakes, request.DefenderCompleted, mistakeConst, defenceConst);

      // Defender wins ties.
      var winner = attackerScore > defenderScore ? BattleWinner.attacker : BattleWinner.defender;

      // - difference 0 or 1-50% => 1 troop lost
      // - difference above 50% => 2 troops lost
      var (attackerTroopLoss, defenderTroopLoss) = CalculateTroopLosses(attackerScore, defenderScore);

      var dto = new CreateResultDto(
              BattleId: battleId,
              Winner: winner,
              AttackerScore: attackerScore,
              DefenderScore: defenderScore,
              AttackerMistakes: request.AttackerMistakes,
              DefenderMistakes: request.DefenderMistakes,
              AttackerCompleted: request.AttackerCompleted,
              DefenderCompleted: request.DefenderCompleted,
              AttackerTroopLoss: attackerTroopLoss,
              DefenderTroopLoss: defenderTroopLoss
          );

      var created = await repository.CreateResultAsync(dto, ct);
      return Results.Created($"/api/results/{created.Id}", created);
    }).WithSummary("Create a result").WithDescription("Create a resolved result for one battle using both players' typing data.");

    return app;
  }

  // Calculate the final score and clamp negative values to zero.
  private static int CalculateScore(int wpm, int mistakes, bool completed, int mistakeConst, int bonus)
  {
    if (!completed)
    {
      return 0;
    }

    var score = wpm - (mistakes * mistakeConst) + bonus;
    return Math.Max(score, 0);
  }

  // Calculates troop losses based on the score difference.
  // The loser loses either 1 or 2 troops (to keep the implementation simple).
  private static (int attackerTroopLoss, int defenderTroopLoss) CalculateTroopLosses(int attackerScore, int defenderScore)
  {
    // Defender wins ties, so attacker loses 1 on equal scores.
    if (attackerScore == defenderScore)
    {
      return (1, 0);
    }

    if (attackerScore > defenderScore)
    {
      // Avoid division by zero when the loser scored 0.
      if (defenderScore == 0)
      {
        return (0, 2);
      }

      var ratio = (double)attackerScore / defenderScore;
      return ratio > 1.5 ? (0, 2) : (0, 1);
    }

    // Defender won.
    if (attackerScore == 0)
    {
      return (2, 0);
    }

    var defenderRatio = (double)defenderScore / attackerScore;
    return defenderRatio > 1.5 ? (2, 0) : (1, 0);
  }
}