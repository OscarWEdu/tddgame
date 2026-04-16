namespace TddGame;

public record TerritoryDto(
    int Id,
    string Name,
    int NorthAdjacentId,
    int SouthAdjacentId,
    int WestAdjacentId,
    int EastAdjacentId,
    int continents_id
);