using Microsoft.AspNetCore.Http.HttpResults;

namespace TddGame;

// Defines the HTTP API routes related to results.
public static class ResultEndpoints
{
    public static IEndpointRouteBuilder MapResultEndpoints(this IEndpointRouteBuilder app)
    {
        var resultGroup = app.MapGroup("/api/results").WithTags("Results");

        // GET /api/results/{id}
        resultGroup.MapGet(
            "/{id:int}",
            async Task<Results<Ok<ResultDto>, NotFound>> (int id, IResultsRepository repo, CancellationToken ct)
            =>
            {
                var result = await repo.GetResultByIdAsync(id, ct);
                return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
            }
        ).WithSummary("Get result by id")
         .WithDescription("Return one specific result by id.");

        // GET /api/results?battleId={battleId}
        resultGroup.MapGet(
            "/",
            async Task<Results<Ok<ResultDto>, NotFound>> (int battleId, IResultsRepository repo, CancellationToken ct)
            =>
            {
                var result = await repo.GetResultByBattleIdAsync(battleId, ct);
                return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
            }
        ).WithSummary("Get result for a battle")
         .WithDescription("Return the result connected to a specific battle.");

        // POST /api/results?battleId={battleId}
        resultGroup.MapPost(
            "/",
            async Task<Results<Created<ResultDto>, BadRequest<string>, NotFound<string>, Conflict<string>>> (
                int battleId,
                CreateResultRequest request,
                IResultsRepository repo,
                CancellationToken ct)
            =>
            {
                var existingResult = await repo.GetResultByBattleIdAsync(battleId, ct);

                if (existingResult is not null)
                {
                    return TypedResults.Conflict("Result already exists for this battle.");
                }

                var battle = await repo.GetBattleForResultAsync(battleId, ct);

                if (battle is null)
                {
                    return TypedResults.NotFound("Battle not found.");
                }

                var challengeExists = await repo.TypingChallengeExistsForBattleAsync(battleId, ct);

                if (!challengeExists)
                {
                    return TypedResults.BadRequest("Typing challenge must exist before creating a result.");
                }

                if (request.AttackerWpm < 0 || request.DefenderWpm < 0)
                {
                    return TypedResults.BadRequest("WPM cannot be negative.");
                }

                if (request.AttackerMistakes < 0 || request.DefenderMistakes < 0)
                {
                    return TypedResults.BadRequest("Mistakes cannot be negative.");
                }

                const int mistakeConst = 2;
                const int defenceConst = 5;

                var attackerScore = CalculateScore(
                    request.AttackerWpm,
                    request.AttackerMistakes,
                    request.AttackerCompleted,
                    mistakeConst,
                    0
                );

                var defenderScore = CalculateScore(
                    request.DefenderWpm,
                    request.DefenderMistakes,
                    request.DefenderCompleted,
                    mistakeConst,
                    defenceConst
                );

                var winner = attackerScore > defenderScore
                    ? BattleWinner.attacker
                    : BattleWinner.defender;

                var (attackerTroopLoss, defenderTroopLoss) = CalculateTroopLosses(attackerScore, defenderScore);

                var createdResult = await repo.CreateResultAsync(
                    new CreateResultDto(
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
                    ),
                    ct
                );

                return TypedResults.Created($"/api/results/{createdResult.Id}", createdResult);
            }
        ).WithSummary("Create result")
         .WithDescription("Create a resolved result for a specific battle using both players' typing data.");

        return app;
    }

    // Calculates the final score and prevents negative values.
    private static int CalculateScore(int wpm, int mistakes, bool completed, int mistakeConst, int bonus)
    {
        if (!completed)
        {
            return 0;
        }

        var score = wpm - (mistakes * mistakeConst) + bonus;
        return Math.Max(score, 0);
    }

    // Calculates troop losses based on score difference.
    private static (int attackerTroopLoss, int defenderTroopLoss) CalculateTroopLosses(int attackerScore, int defenderScore)
    {
        if (attackerScore == defenderScore)
        {
            return (1, 0);
        }

        if (attackerScore > defenderScore)
        {
            if (defenderScore == 0)
            {
                return (0, 2);
            }

            var attackerRatio = (double)attackerScore / defenderScore;
            return attackerRatio > 1.5 ? (0, 2) : (0, 1);
        }

        if (attackerScore == 0)
        {
            return (2, 0);
        }

        var defenderRatio = (double)defenderScore / attackerScore;
        return defenderRatio > 1.5 ? (2, 0) : (1, 0);
    }
}