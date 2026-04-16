namespace TddGame;

public record TerritoryDto(
    int Id,
    string name,
    int NorthAdjacentId,
    int SouthAdjacentId,
    int WestAdjacentId,
    int EastAdjacentId,
    int continents_id
);