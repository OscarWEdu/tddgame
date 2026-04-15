// Contains data models related to players (PlayerDto, CreatePlayerDto, PlayerStateDto)
// Represents player-specific data such as id, name, colour, turnOrder, numGold, isDead, missionId, and gameSession_id.
namespace TddGame;

public record PlayerDto(
    int id,
    string Name,
    string Colour,
    int TurnOrder,
    int NumGold,
    bool IsDead,
    int GameSessionId,
    int MissionId
);

public record CreatePlayerDto(
    string Name,
    string Colour,
    int TurnOrder,
    int MissionId
);

public record PlayerStateDto(
    int NumGold,
    bool IsDead
);
