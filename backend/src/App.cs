using MySqlConnector;
using TddGame;
using WebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("db-config.json", optional: false);

var c = builder.Configuration;
var connStr = $"Server={c["host"]};Port={c["port"]};Database={c["database"]};User={c["username"]};Password={c["password"]};";

builder.Services.AddSingleton(new MySqlDataSource(connStr));

builder.Services.AddScoped<IGameSessionsRepository, GameSessionRepository>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();
app.MapGameSessionEndpoints();

try
{
    DbQuery.Initialize();
}
catch (Exception ex)
{
    Console.WriteLine("DbQuery error: " + ex.Message);
}

app.Run();


