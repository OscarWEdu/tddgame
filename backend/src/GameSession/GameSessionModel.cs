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
    GameSessionStatus Status,
    int MaxPlayers,
    int PlayerCount
);

public record CreateGameSessionRequest(string Name, int MaxPlayers);
public record UpdateGameSessionStatusRequest(GameSessionStatus Status);