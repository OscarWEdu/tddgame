using MySqlConnector;
using TddGame;
using WebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("db-config.json", optional: false);

var c = builder.Configuration;
var connStr = $"Server={c["host"]};Port={c["port"]};Database={c["database"]};User={c["username"]};Password={c["password"]};";

builder.Services.AddSingleton(new MySqlDataSource(connStr));

builder.Services.AddScoped<IGameSessionsRepository, GameSessionRepository>();
builder.Services.AddScoped<IPlayersRepository, PlayersRepository>();
builder.Services.AddScoped<ITurnsRepository, TurnsRepository>();
builder.Services.AddScoped<IContinentRepository, ContinentRepository>();
builder.Services.AddScoped<ITerritoryRepository, TerritoryRepository>();
builder.Services.AddScoped<IBattlesRepository, BattlesRepository>();
builder.Services.AddScoped<IPlayerTerritoryRepository, PlayerTerritoryRepository>();
builder.Services.AddScoped<IBattlesRepository, BattlesRepository>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "TDD Game API", Version = "v1" });
});

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TDD Game API v1");
});
app.MapGameSessionEndpoints();
app.MapPlayersEndpoints();
app.MapTurnsEndpoints();
app.MapContinentEndpoints();
app.MapTerritoryEndpoints();
app.MapBattlesEndpoints();
app.MapPlayerTerritoryEndpoints();
app.MapBattlesEndpoints();

try
{
    DbQuery.Initialize();
}
catch (Exception ex)
{
    Console.WriteLine("DbQuery error: " + ex.Message);
}

await app.RunStartupUtils();

Console.WriteLine("Server started");
app.Run();


