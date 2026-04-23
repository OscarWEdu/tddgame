// contains data related to missions (MissionDto, CreateMissionDto, MissionStateDto)
// represents mission data such as id, name and decsription

namespace TddGame;

public record MissionDto(
    int Id,
    string Name,
    string Description
);

public record CreateMissionDto(
    string Name,
    string Description);

public record MissionsDto(
    string Name,
    string Description);