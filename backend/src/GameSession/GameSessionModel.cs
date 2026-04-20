namespace TddGame;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameSessionStatus
{
    lobby,
    started,
    completed
}

public record GameSessionDto(
    string Id,
    string Name,
    GameSessionStatus Status
);

public record CreateGameSessionRequest(string Name);
public record UpdateGameSessionStatusRequest(GameSessionStatus Status);