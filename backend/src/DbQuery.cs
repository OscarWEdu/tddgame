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
                status ENUM('lobby', 'started', 'completed') NOT NULL DEFAULT 'lobby'
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

            
            CREATE TABLE IF NOT EXISTS TerritoryAdjacencies (
                territoryId INT NOT NULL,
                adjacentTerritoryId INT NOT NULL,
                PRIMARY KEY (territoryId, adjacentTerritoryId),
                FOREIGN KEY (territoryId) REFERENCES Territories(id),
                FOREIGN KEY (adjacentTerritoryId) REFERENCES Territories(id)
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
         // Seed Continents (6 classic Risk continents, ids match Territories.continentId below)
        command.CommandText = "SELECT COUNT(*) FROM Continents";
        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            var ContinentData = @"
                INSERT INTO Continents (id, name, bonusConst) VALUES
                (1, 'North America', 5),
                (2, 'South America', 2),
                (3, 'Europe', 5),
                (4, 'Africa', 3),
                (5, 'Asia', 7),
                (6, 'Australia', 2);
            ";
            command.CommandText = ContinentData;
            command.ExecuteNonQuery();
        }

        // Seed Territories (42 classic Risk territories; names match the SVG path ids
        // when slugified, so the frontend RiskMap can match by name. Adjacency fields
        // are placeholders (0) until the schema gains a real adjacency list.)
        command.CommandText = "SELECT COUNT(*) FROM Territories";
        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            var TerritoryData = @"
                INSERT INTO Territories (name, NorthAdjacentId, SouthAdjacentId, EastAdjacentId, WestAdjacentId, continentId) VALUES
                ('Alaska', 0, 0, 0, 0, 1),
                ('Alberta', 0, 0, 0, 0, 1),
                ('Central America', 0, 0, 0, 0, 1),
                ('Eastern United States', 0, 0, 0, 0, 1),
                ('Greenland', 0, 0, 0, 0, 1),
                ('Northwest Territory', 0, 0, 0, 0, 1),
                ('Ontario', 0, 0, 0, 0, 1),
                ('Quebec', 0, 0, 0, 0, 1),
                ('Western United States', 0, 0, 0, 0, 1),
                ('Argentina', 0, 0, 0, 0, 2),
                ('Brazil', 0, 0, 0, 0, 2),
                ('Venezuela', 0, 0, 0, 0, 2),
                ('Peru', 0, 0, 0, 0, 2),
                ('Great Britain', 0, 0, 0, 0, 3),
                ('Iceland', 0, 0, 0, 0, 3),
                ('Northern Europe', 0, 0, 0, 0, 3),
                ('Scandinavia', 0, 0, 0, 0, 3),
                ('Southern Europe', 0, 0, 0, 0, 3),
                ('Ukraine', 0, 0, 0, 0, 3),
                ('Western Europe', 0, 0, 0, 0, 3),
                ('Congo', 0, 0, 0, 0, 4),
                ('East Africa', 0, 0, 0, 0, 4),
                ('Egypt', 0, 0, 0, 0, 4),
                ('Madagascar', 0, 0, 0, 0, 4),
                ('North Africa', 0, 0, 0, 0, 4),
                ('South Africa', 0, 0, 0, 0, 4),
                ('Afghanistan', 0, 0, 0, 0, 5),
                ('China', 0, 0, 0, 0, 5),
                ('India', 0, 0, 0, 0, 5),
                ('Irkutsk', 0, 0, 0, 0, 5),
                ('Japan', 0, 0, 0, 0, 5),
                ('Kamchatka', 0, 0, 0, 0, 5),
                ('Middle East', 0, 0, 0, 0, 5),
                ('Mongolia', 0, 0, 0, 0, 5),
                ('Siam', 0, 0, 0, 0, 5),
                ('Siberia', 0, 0, 0, 0, 5),
                ('Ural', 0, 0, 0, 0, 5),
                ('Yakutsk', 0, 0, 0, 0, 5),
                ('Eastern Australia', 0, 0, 0, 0, 6),
                ('New Guinea', 0, 0, 0, 0, 6),
                ('Indonesia', 0, 0, 0, 0, 6),
                ('Western Australia', 0, 0, 0, 0, 6);
            ";
            command.CommandText = TerritoryData;
            command.ExecuteNonQuery();
        }

        // Seed Territory Adjacencies (classic Risk neighbor pairs, stored
        // symmetrically — each edge becomes two rows so a single lookup by
        // territoryId returns all neighbors. Ids match the insertion order of
        // the Territories seed above.)
        command.CommandText = "SELECT COUNT(*) FROM TerritoryAdjacencies";
        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            var edges = new (int A, int B)[]
            {
                // North America (1=Alaska, 2=Alberta, 3=Central America, 4=EUS,
                // 5=Greenland, 6=NWT, 7=Ontario, 8=Quebec, 9=WUS)
                (1, 2), (1, 6), (1, 32),
                (2, 6), (2, 7), (2, 9),
                (3, 4), (3, 9), (3, 12),
                (4, 7), (4, 8), (4, 9),
                (5, 6), (5, 7), (5, 8), (5, 15),
                (6, 7),
                (7, 8), (7, 9),
                // South America (10=Argentina, 11=Brazil, 12=Venezuela, 13=Peru)
                (10, 11), (10, 13),
                (11, 12), (11, 13), (11, 25),
                (12, 13),
                // Europe (14=GB, 15=Iceland, 16=NEU, 17=Scandinavia, 18=SEU,
                // 19=Ukraine, 20=WEU)
                (14, 15), (14, 16), (14, 17), (14, 20),
                (15, 17),
                (16, 17), (16, 18), (16, 19), (16, 20),
                (17, 19),
                (18, 19), (18, 20), (18, 23), (18, 25), (18, 33),
                (19, 27), (19, 33), (19, 37),
                (20, 25),
                // Africa (21=Congo, 22=East Africa, 23=Egypt, 24=Madagascar,
                // 25=North Africa, 26=South Africa)
                (21, 22), (21, 25), (21, 26),
                (22, 23), (22, 24), (22, 25), (22, 26), (22, 33),
                (23, 25), (23, 33),
                (24, 26),
                // Asia (27=Afghanistan, 28=China, 29=India, 30=Irkutsk, 31=Japan,
                // 32=Kamchatka, 33=Middle East, 34=Mongolia, 35=Siam, 36=Siberia,
                // 37=Ural, 38=Yakutsk)
                (27, 28), (27, 29), (27, 33), (27, 37),
                (28, 29), (28, 34), (28, 35), (28, 36), (28, 37),
                (29, 33), (29, 35),
                (30, 32), (30, 34), (30, 36), (30, 38),
                (31, 32), (31, 34),
                (32, 34), (32, 38),
                (34, 36),
                (35, 41),
                (36, 37), (36, 38),
                // Australia (39=Eastern Australia, 40=New Guinea, 41=Indonesia,
                // 42=Western Australia)
                (39, 40), (39, 42),
                (40, 41), (40, 42),
                (41, 42),
            };

            var values = new List<string>(edges.Length * 2);
            foreach (var (a, b) in edges)
            {
                values.Add($"({a},{b})");
                values.Add($"({b},{a})");
            }
            command.CommandText =
                "INSERT INTO TerritoryAdjacencies (territoryId, adjacentTerritoryId) VALUES "
                + string.Join(",", values) + ";";
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
