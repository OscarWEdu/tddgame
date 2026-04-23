namespace WebApp;

using MySqlConnector;
using Dyndata;
using static Dyndata.Factory;

public static class DbQuery
{
    // Setup the database connection from config
    private static string connectionString;

    // JSON columns for _CONTAINS_ validation
    public static Arr JsonColumns = Arr(new[] { "categories" });

    public static bool IsJsonColumn(string column) => JsonColumns.Includes(column);

    static DbQuery()
    {
        var configPath = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "db-config.json"
        );
        var configJson = File.ReadAllText(configPath);
        var config = JSON.Parse(configJson);

        connectionString =
            $"Server={config.host};Port={config.port};Database={config.database};" +
            $"User={config.username};Password={config.password};";

        var db = new MySqlConnection(connectionString);
        db.Open();

        // Create tables if they don't exist
        if (config.createTablesIfNotExist == true)
        {
            CreateTablesIfNotExist(db);
        }

        // Seed data if tables are empty
        if (config.seedDataIfEmpty == true)
        {
            SeedDataIfEmpty(db);
        }

        db.Close();
    }

    private static void CreateTablesIfNotExist(MySqlConnection db)
    {
        // Create tables in the database (MySQL)
        var createTablesSql = @"
            CREATE TABLE IF NOT EXISTS GameSessions (
                id VARCHAR(255) PRIMARY KEY NOT NULL,
                name VARCHAR(255) NOT NULL,
                status ENUM('lobby', 'started', 'completed') NOT NULL DEFAULT 'lobby',
                maxPlayers INT NOT NULL DEFAULT 6
            );

            CREATE TABLE IF NOT EXISTS Missions (
                id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
                name VARCHAR(255) NOT NULL,
                description TEXT NOT NULL,
                winCondition ENUM('1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12', '13') NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Players (
                id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(255) NOT NULL,
                colour ENUM('Black', 'Blue', 'Green', 'Pink', 'Red', 'Yellow') NOT NULL,
                turnOrder INT NOT NULL,
                numGold INT NOT NULL DEFAULT 0,
                isDead BOOLEAN NOT NULL DEFAULT FALSE,
                isHost BOOLEAN NOT NULL DEFAULT FALSE,
                gameSessions_id VARCHAR(255) NOT NULL,
                missions_id INT NOT NULL,
                FOREIGN KEY (gameSessions_id) REFERENCES GameSessions(id),
                FOREIGN KEY (missions_id) REFERENCES Missions(id)
            );

            CREATE TABLE IF NOT EXISTS Continents (
                id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
                name VARCHAR(255) NOT NULL,
                bonusConst INT NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Territories (
                id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
                name VARCHAR(255) NOT NULL,
                NorthAdjacentId INT NOT NULL,
                SouthAdjacentId INT NOT NULL,
                EastAdjacentId INT NOT NULL,
                WestAdjacentId INT NOT NULL,
                continentId INT NOT NULL,
                FOREIGN KEY (continentId) REFERENCES Continents(id)
            );

            CREATE TABLE IF NOT EXISTS PlayerTerritories (
                id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
                troopNum INT NOT NULL DEFAULT 0,
                hasCity BOOLEAN DEFAULT FALSE,
                playerId INT NOT NULL,
                territoryId INT NOT NULL,
                FOREIGN KEY (playerId) REFERENCES Players(id),
                FOREIGN KEY (territoryId) REFERENCES Territories(id)
            );

            CREATE TABLE IF NOT EXISTS Turns (
                id INT PRIMARY KEY NOT NULL,
                round INT NOT NULL DEFAULT 0,
                phase ENUM('build', 'assigned', 'attack', 'reinforce') NOT NULL DEFAULT 'build',
                status ENUM('active', 'inactive') NOT NULL DEFAULT 'active',
                createAt DATE DEFAULT (CURDATE()) NOT NULL,
                gameSessions_id VARCHAR(255) NOT NULL,
                players_id INT NOT NULL,
                FOREIGN KEY (gameSessions_id) REFERENCES GameSessions(id),
                FOREIGN KEY (players_id) REFERENCES Players(id)
            );

            CREATE TABLE IF NOT EXISTS Battles (
                id INT PRIMARY KEY NOT NULL,
                attackingTroops INT NOT NULL DEFAULT 0,
                attackerTerritoryId INT NOT NULL,
                defenderTerritoryId INT NOT NULL,
                FOREIGN KEY (attackerTerritoryId) REFERENCES PlayerTerritories(id),
                FOREIGN KEY (defenderTerritoryId) REFERENCES PlayerTerritories(id)
            );

            CREATE TABLE IF NOT EXISTS TypingChallenges (
                id INT PRIMARY KEY NOT NULL,
                speed INT NOT NULL DEFAULT 0,
                mistakes INT NOT NULL DEFAULT 0,
                promptText TEXT NOT NULL,
                battles_id INT NOT NULL UNIQUE,
                FOREIGN KEY (battles_id) REFERENCES Battles(id)
            );

            CREATE TABLE IF NOT EXISTS Results (
                id INT PRIMARY KEY NOT NULL,
                battles_id INT NOT NULL UNIQUE,
                winner ENUM('attacker', 'defender') NOT NULL,
                attackerScore INT NOT NULL DEFAULT 0,
                defenderScore INT NOT NULL DEFAULT 0,
                attackerMistakes INT NOT NULL DEFAULT 0,
                defenderMistakes INT NOT NULL DEFAULT 0,
                attackerCompleted BOOLEAN NOT NULL DEFAULT FALSE,
                defenderCompleted BOOLEAN NOT NULL DEFAULT FALSE,
                attackerTroopLoss INT NOT NULL DEFAULT 0,
                defenderTroopLoss INT NOT NULL DEFAULT 0,
                FOREIGN KEY (battles_id) REFERENCES Battles(id)
            );
        ";

        // Execute each statement separately
        foreach (var sql in createTablesSql.Split(';'))
        {
            var trimmed = sql.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                var command = db.CreateCommand();
                command.CommandText = trimmed;
                command.ExecuteNonQuery();
            }
        }
        var createViews = @"
        ";
        // Execute each statement separately
        foreach (var sql in createViews.Split(';'))
        {
            var trimmed = sql.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                var command = db.CreateCommand();
                command.CommandText = trimmed;
                command.ExecuteNonQuery();
            }
        }
    }

    private static void SeedDataIfEmpty(MySqlConnection db)
    {
        // Check if tables are empty and seed if needed
        var command = db.CreateCommand();

        // Seed GameSessions rules
        command.CommandText = "SELECT COUNT(*) FROM GameSessions";
        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            var GameSessionData = @"
                INSERT INTO GameSessions (id, name, status) VALUES
                ('550e8400-e29b-41d4-a716-446655440000', 'new', 'lobby'),
                ('6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'newTest', 'completed');
            ";
            command.CommandText = GameSessionData;
            command.ExecuteNonQuery();
        }

        // Seed missions
        command.CommandText = "SELECT COUNT(*) FROM Missions";
        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            var MissionData = @"
                INSERT INTO Missions (id, name, description, winCondition) VALUES
                (1, 'Capture North America and Australia', 'Own North America and Australia continent at the end of your turn', '2'),
                (2, 'Capture Asia and South America', 'Own Asia and South America continent at the end of your turn', '2'),
                (3, 'Capture 24 territories', 'Own 24 territories at the end of your turn', '1'),
                (4, 'Capture 18 territories with 2 troops', 'Own 18 territories and each must have at least 2 troops', '2');
            ";
            command.CommandText = MissionData;
            command.ExecuteNonQuery();
        }

        // Seed players
        command.CommandText = "SELECT COUNT(*) FROM Players";
        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            var PlayerData = @"
                INSERT INTO Players (id, name, colour, turnOrder, numGold, isDead, gameSessions_id, missions_id) VALUES
                (1, 'Max', 'Blue', 1, 10, 0, '550e8400-e29b-41d4-a716-446655440000', 2),
                (2, 'Linus', 'Red', 2, 5, 0, '550e8400-e29b-41d4-a716-446655440000', 1);
            ";
            command.CommandText = PlayerData;
            command.ExecuteNonQuery();
        }
        // Seed Continents
        command.CommandText = "SELECT COUNT(*) FROM Continents";
        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            var ContinentData = @"
                INSERT INTO Continents (name, bonusConst) VALUES
                ('Eurtarctica', 3),
                ('Ameristralia', 2);
            ";
            command.CommandText = ContinentData;
            command.ExecuteNonQuery();
        }

        // Seed the rest of the tables/views here. 
    }

    public static void Initialize()
    {
    }


    // // Helper to create an object from the DataReader
    // private static dynamic ObjFromReader(MySqlDataReader reader)
    // {
    //     var obj = Obj();
    //     for (var i = 0; i < reader.FieldCount; i++)
    //     {
    //         var key = reader.GetName(i);
    //         var value = reader.GetValue(i);

    //         // Handle NULL values
    //         if (value == DBNull.Value)
    //         {
    //             obj[key] = null;
    //         }
    //         // Handle DateTime - convert to ISO string
    //         else if (value is DateTime dt)
    //         {
    //             obj[key] = dt.ToString("yyyy-MM-ddTHH:mm:ss");
    //         }
    //         // Handle boolean (MySQL returns sbyte for TINYINT(1))
    //         else if (value is sbyte sb)
    //         {
    //             obj[key] = sb != 0;
    //         }
    //         else if (value is bool b)
    //         {
    //             obj[key] = b;
    //         }
    //         // Handle JSON columns (MySQL returns JSON as string starting with [ or {)
    //         else if (value is string strValue && (strValue.StartsWith("[") || strValue.StartsWith("{")))
    //         {
    //             // Special case: Don't parse 'data' column from sessions - keep as string
    //             if (key == "data")
    //             {
    //                 obj[key] = strValue;
    //             }
    //             else
    //             {
    //                 try
    //                 {
    //                     obj[key] = JSON.Parse(strValue);
    //                 }
    //                 catch
    //                 {
    //                     // If parsing fails, keep the original value and try to convert to number
    //                     obj[key] = strValue.TryToNum();
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             // Normal handling - convert to string and try to parse as number
    //             obj[key] = value.ToString().TryToNum();
    //         }
    //     }
    //     return obj;
    // }

    // // Run a query - rows are returned as an array of objects
    // public static Arr SQLQuery(
    //     string sql, object parameters = null, HttpContext context = null
    // )
    // {
    //     var paras = parameters == null ? Obj() : Obj(parameters);
    //     using var db = new MySqlConnection(connectionString);
    //     db.Open();
    //     var command = db.CreateCommand();
    //     command.CommandText = @sql;
    //     var entries = (Arr)paras.GetEntries();
    //     entries.ForEach(x => command.Parameters.AddWithValue("@" + x[0], x[1]));
    //     if (context != null)
    //     {
    //         DebugLog.Add(context, new
    //         {
    //             sqlQuery = sql.Regplace(@"\s+", " "),
    //             sqlParams = paras
    //         });
    //     }
    //     var rows = Arr();
    //     try
    //     {
    //         if (sql.StartsWith("SELECT ", true, null))
    //         {
    //             var reader = command.ExecuteReader();
    //             while (reader.Read())
    //             {
    //                 rows.Push(ObjFromReader(reader));
    //             }
    //             reader.Close();
    //         }
    //         else
    //         {
    //             rows.Push(new
    //             {
    //                 command = sql.Split(" ")[0].ToUpper(),
    //                 rowsAffected = command.ExecuteNonQuery()
    //             });
    //         }
    //     }
    //     catch (Exception err)
    //     {
    //         rows.Push(new { error = err.Message });
    //     }
    //     return rows;
    // }

    // Run a query - only return the first row, as an object
    // public static dynamic SQLQueryOne(
    //     string sql, object parameters = null, HttpContext context = null
    // )
    // {
    //     return SQLQuery(sql, parameters, context)[0];
    // }
}
