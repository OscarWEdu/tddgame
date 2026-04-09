namespace WebApp;

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
        var createTablesSql = @"
            CREATE TABLE IF NOT EXISTS GameSessions (
            id VARCHAR(255) PRIMARY KEY NOT NULL,
            name VARCHAR(255) NOT NULL,
            status ENUM('lobby', 'started', 'completed') NOT NULL DEFAULT 'lobby',

            CREATE TABLE IF NOT EXISTS Missions (
            id INT PRIMARY KEY NOT NULL,
            name VARCHAR(255) NOT NULL,
            description TEXT NOT NULL,
            condition ENUM('available', 'occupied') NOT NULL DEFAULT 'available',

            CREATE TABLE IF NOT EXISTS Players (
            id INT PRIMARY KEY AUTO_INCREMENT,
            name VARCHAR(255) NOT NULL,
            colour ENUM('Black', 'Blue', 'Green', 'Pink', 'Red', 'Yellow') NOT NULL,
            turnOrder INT NOT NULL,
            numGold INT NOT NULL DEFAULT 0,
            isDead BOOLEAN NOT NULL DEFAULT FALSE,
            gameSessions_id VARCHAR NOT NULL,
            missions_id INT NOT NULL,
            FOREIGN KEY (gameSessions_id) REFERENCES GameSessions(id),
            FOREIGN KEY (missions_id) REFERENCES Missions(id),

            CREATE TABLE IF NOT EXISTS Continents (
            id INT PRIMARY KEY NOT NULL,
            name VARCHAR(255) NOT NULL,
            bonusConst INT NOT NULL DEFAULT 0,

            CREATE TABLE IF NOT EXISTS Territories (
            id INT PRIMARY KEY NOT NULL,
            name VARCHAR(255) NOT NULL,
            NorthAdjacentId INT NOT NULL,
            SouthAdjacentId INT NOT NULL,
            EastAdjacentId INT NOT NULL,
            WestAdjacentId INT NOT NULL,
            continents_id INT NOT NULL,
            FOREIGN KEY (continents_id) REFERENCES Continents(id),
            
            CREATE TABLE IF NOT EXISTS PlayerTerritories (
            troopNum INT NOT NULL DEFAULT 0,
            players_id INT NOT NULL,
            territories_id INT NOT NULL,
            FOREIGN KEY (players_id) REFERENCES Players(id),
            FOREIGN KEY (territories_id) REFERENCES Territories(id),

            CREATE TABLE IF NOT EXISTS Turns (
            round INT NOT NULL DEFAULT 0,
            phase ENUM('build', 'assigned', 'attack', 'renforce') NOT NULL DEFAULT build,
            createAt DATE DEFAULT (CURDATE()) NOT NULL,
            gameSessions_id INT NOT NULL,
            players_id INT NOT NULL,
            FOREIGN KEY (gameSessions_id) REFERENCES GameSessions(id),
            FOREIGN KEY (players_id) REFERENCES Players(id),

            CREATE TABLE IF NOT EXISTS Battles (
            id INT PRIMARY KEY NOT NULL,
            attackerTerritoryId INT NOT NULL,
            defenderTerritoryId INT NOT NULL,
            attackingTroops INT NOT NULL DEFAULT 0,

            CREATE TABLE IF NOT EXISTS TypingChallenges (
            id INT PRIMARY KEY NOT NULL,
            speed INT PRIMARY KEY NOT NULL,
            mistakes INT PRIMARY KEY NOT NULL,
            promptText TEXT NOT NULL,

            CREATE TABLE IF NOT EXISTS Results (
            id INT PRIMARY KEY NOT NULL,
            attackerScore INT NOT NULL DEFAULT 0,
            defenderScore INT NOT NULL DEFAULT 0,
            battles_id INT NOT NULL,
            FOREIGN KEY (battles_id) REFERENCES battles(id),
        
        )";

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

        // Seed ACL rules
        command.CommandText = "SELECT COUNT(*) FROM acl";
        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            var aclData = @"
                INSERT INTO acl (userRoles, method, allow, route, `match`, comment) VALUES
                ('visitor, user', 'GET', 'disallow', '/secret.html', 'true', 'No access to /secret.html for visitors and normal users'),
                ('visitor,user, admin', 'GET', 'allow', '/api', 'false', 'Allow access to all routes not starting with /api'),
                ('visitor', 'POST', 'allow', '/api/users', 'true', 'Allow registration as new user for visitors'),
                ('visitor, user,admin', '*', 'allow', '/api/login', 'true', 'Allow access to all login routes'),
                ('admin', '*', 'allow', '/api/users', 'true', 'Allow admins to see and edit users'),
                ('admin', '*', 'allow', '/api/sessions', 'true', 'Allow admins to see and edit sessions'),
                ('admin', '*', 'allow', '/api/acl', 'true', 'Allow admins to see and edit acl rules'),
                ('visitor,user,admin', 'GET', 'allow', '/api/products', 'true', 'Allow all user roles to read products'),
                ('visitor,user,admin', 'GET', 'allow', '/api/films', 'true', 'Allow all user roles to see films');
            ";
            command.CommandText = aclData;
            command.ExecuteNonQuery();
        }
        // Seed the rest of the tables/views here. 

        /* // Seed users
        command.CommandText = "SELECT COUNT(*) FROM users";
        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            var usersData = @"
                INSERT INTO users (created, email, firstName, lastName, role, password) VALUES
                ('2024-04-02', 'thomas@nodehill.com', 'Thomas', 'Frank', 'admin', '$2a$13$IahRVtN2pxc1Ne1NzJUPpOQO5JCtDZvXpSF.IF8uW85S6VoZKCwZq'),
                ('2024-04-02', 'olle@nodehill.com', 'Olle', 'Olofsson', 'user', '$2a$13$O2Gs3FME3oA1DAzwE0FkOuMAOOAgRyuvNQq937.cl7D.xq0IjgzN.'),
                ('2024-04-02', 'maria@nodehill.com', 'Maria', 'Mårtensson', 'user', '$2a$13$p4sqCN3V3C1wQXspq4eN0eYwK51ypw7NPL6b6O4lMAOyATJtKqjHS');
            ";
            command.CommandText = usersData;
            command.ExecuteNonQuery();
        } */
    }

    // Helper to create an object from the DataReader
    private static dynamic ObjFromReader(MySqlDataReader reader)
    {
        var obj = Obj();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var key = reader.GetName(i);
            var value = reader.GetValue(i);

            // Handle NULL values
            if (value == DBNull.Value)
            {
                obj[key] = null;
            }
            // Handle DateTime - convert to ISO string
            else if (value is DateTime dt)
            {
                obj[key] = dt.ToString("yyyy-MM-ddTHH:mm:ss");
            }
            // Handle boolean (MySQL returns sbyte for TINYINT(1))
            else if (value is sbyte sb)
            {
                obj[key] = sb != 0;
            }
            else if (value is bool b)
            {
                obj[key] = b;
            }
            // Handle JSON columns (MySQL returns JSON as string starting with [ or {)
            else if (value is string strValue && (strValue.StartsWith("[") || strValue.StartsWith("{")))
            {
                // Special case: Don't parse 'data' column from sessions - keep as string
                if (key == "data")
                {
                    obj[key] = strValue;
                }
                else
                {
                    try
                    {
                        obj[key] = JSON.Parse(strValue);
                    }
                    catch
                    {
                        // If parsing fails, keep the original value and try to convert to number
                        obj[key] = strValue.TryToNum();
                    }
                }
            }
            else
            {
                // Normal handling - convert to string and try to parse as number
                obj[key] = value.ToString().TryToNum();
            }
        }
        return obj;
    }

    // Run a query - rows are returned as an array of objects
    public static Arr SQLQuery(
        string sql, object parameters = null, HttpContext context = null
    )
    {
        var paras = parameters == null ? Obj() : Obj(parameters);
        using var db = new MySqlConnection(connectionString);
        db.Open();
        var command = db.CreateCommand();
        command.CommandText = @sql;
        var entries = (Arr)paras.GetEntries();
        entries.ForEach(x => command.Parameters.AddWithValue("@" + x[0], x[1]));
        if (context != null)
        {
            DebugLog.Add(context, new
            {
                sqlQuery = sql.Regplace(@"\s+", " "),
                sqlParams = paras
            });
        }
        var rows = Arr();
        try
        {
            if (sql.StartsWith("SELECT ", true, null))
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    rows.Push(ObjFromReader(reader));
                }
                reader.Close();
            }
            else
            {
                rows.Push(new
                {
                    command = sql.Split(" ")[0].ToUpper(),
                    rowsAffected = command.ExecuteNonQuery()
                });
            }
        }
        catch (Exception err)
        {
            rows.Push(new { error = err.Message });
        }
        return rows;
    }

    // Run a query - only return the first row, as an object
    public static dynamic SQLQueryOne(
        string sql, object parameters = null, HttpContext context = null
    )
    {
        return SQLQuery(sql, parameters, context)[0];
    }
}
