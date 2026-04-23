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

    command.CommandText = "SELECT id, name, description, WinConditions FROM Missions";

    var reader = await command.ExecuteReaderAsync(ct);

    while (await reader.ReadAsync(ct))
    {
      var mission = new MissionDto(
          reader.GetInt32("id"),
          reader.GetString("name"),
          reader.GetString("description"),
          reader.GetString("WinConditions")
      );

      missions.Add(mission);
    }

    return missions;
  }

  public async Task<MissionDto?> CreateMissionAsync(CreateMissionDto mission, CancellationToken ct)
  {
    var connection = await _db.OpenConnectionAsync(ct);
    var command = connection.CreateCommand();

    command.CommandText = "INSERT INTO Missions (name, description, winConditions) VALUES (@name, @description, @winconditions); SELECT LAST_INSERT_ID();";

    command.Parameters.AddWithValue("@name", mission.Name);
    command.Parameters.AddWithValue("@description", mission.Description);
    command.Parameters.AddWithValue("@winconditions", mission.WinCondition);

    var result = await command.ExecuteScalarAsync(ct);

    if (result == null)
    {
      return null;
    }

    int id = Convert.ToInt32(result);

    return new MissionDto(id, mission.Name, mission.Description, mission.WinCondition);
  }

  public async Task<MissionDto?> GetMissionByPlayerIdAsync(int playerId, CancellationToken ct)
  {
    var sqlQuery = @"
        SELECT m.id, m.name, m.description, m.winCondition
        FROM Missions m
        INNER JOIN Players p ON p.missions_id = m.id
        WHERE p.id = @playerId";

    await using var connection = await _db.OpenConnectionAsync(ct);
    await using var command = connection.CreateCommand();

    command.CommandText = sqlQuery;
    command.Parameters.AddWithValue("@playerId", playerId);

    await using var reader = await command.ExecuteReaderAsync(ct);

    if (!await reader.ReadAsync(ct))
      return null;

    return new MissionDto(
        Id: reader.GetInt32("id"),
        Name: reader.GetString("name"),
        Description: reader.GetString("description"),
        WinCondition: reader.GetString("winCondition")
    );
  }
}