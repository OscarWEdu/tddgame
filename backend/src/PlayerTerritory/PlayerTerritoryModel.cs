namespace TddGame;

public record PlayerTerritoryDto(
    int Id,
    int TroopNum,
    bool HasCity,
    int PlayerId,
    int TerritoryId
);