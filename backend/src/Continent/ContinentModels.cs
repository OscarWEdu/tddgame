namespace TddGame;

public record ContinentDto(
    int Id,
    string name,
    int bonusConst
);

public record CreateContinentRequest(
    string name,
    int bonusConst
);