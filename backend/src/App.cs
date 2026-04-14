using MySqlConnector;
using TddGame;
using WebApp;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("Default")
?? throw new InvalidOperationException("Connection string 'Default' is not found.");
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

app.Run();

try
{
    DbQuery.Initialize();
}
catch (Exception ex)
{
    Console.WriteLine("DbQuery error: " + ex.Message);
}

