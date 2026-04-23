using MySqlConnector;

namespace TddGame;

public class MissionRepository : IMissionsRepository
{
  private readonly MySqlDataSource _db;

  public MissionRepository(MySqlDataSource db)
  {
    _db = db;
  }

  public async Task<IEnumerable<MissionDto>?> GetMissionsAsync(CancellationToken ct)
  {
    var missions = new List<MissionDto>();

    var connection = await _db.OpenConnectionAsync(ct);
    var command = connection.CreateCommand();

    command.CommandText = "SELECT id, name, description FROM Missions";

    var reader = await command.ExecuteReaderAsync(ct);

    while (await reader.ReadAsync(ct))
    {
      var mission = new MissionDto(
          reader.GetInt32("id"),
          reader.GetString("name"),
          reader.GetString("description")
      );

      missions.Add(mission);
    }

    return missions;
  }

  public async Task<MissionDto?> CreateMissionAsync(CreateMissionDto mission, CancellationToken ct)
  {
    var connection = await _db.OpenConnectionAsync(ct);
    var command = connection.CreateCommand();

    command.CommandText = "INSERT INTO Missions (name, description) VALUES (@name, @description); SELECT LAST_INSERT_ID();";

    command.Parameters.AddWithValue("@name", mission.Name);
    command.Parameters.AddWithValue("@description", mission.Description);

    var result = await command.ExecuteScalarAsync(ct);

    if (result == null)
    {
      return null;
    }

    int id = Convert.ToInt32(result);

    return new MissionDto(id, mission.Name, mission.Description);
  }

  public async Task<int?> GetMissionByPlayerIdAsync(string missionId, int currentMission, CancellationToken ct)
  {
    var connection = await _db.OpenConnectionAsync(ct);
    var command = connection.CreateCommand();

    command.CommandText = "SELECT id FROM Missions WHERE id > @currentMission LIMIT 1";

    command.Parameters.AddWithValue("@currentMission", currentMission);

    var result = await command.ExecuteScalarAsync(ct);

    if (result == null)
    {
      return null;
    }

    return Convert.ToInt32(result);
  }
}