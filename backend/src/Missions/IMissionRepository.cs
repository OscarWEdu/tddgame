// mission repository implementation
// Implements the repository interface using MySQL
// this class contains mission related database operations such as fetching the current mission, creating a new mission and fetching the next mission
// gives a mission dto as a result of the database operations
namespace TddGame;
//defines the contract for all mission based database operations
public interface IMissionsRepository
{
  Task<IEnumerable<MissionDto>?> GetMissionsAsync(string MissionId, CancellationToken ct);
  Task<MissionDto?> CreateMissionAsync(CreateMissionDto turn, CancellationToken ct);
  Task<int?> GetMissionsIdByGameSessionIdAsync(string MissionId, CancellationToken ct);
  Task<int?> GetMissionByPlayerIDAsync(string MissionID, int currentMission, CancellationToken ct);
  Task<int?> CreateMissionAsync(string MissionID, int NextMission, CancellationToken ct);

}
