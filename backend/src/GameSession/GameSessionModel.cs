namespace TddGame;

public record GameSessionDto(
    string Id,
    string Name,
    string Status
);

public record CreateGameSessionRequest(string Name);
public record UpdateGameSessionStatusRequest(string Status);