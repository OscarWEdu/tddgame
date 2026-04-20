namespace TddGame;


public static class StartupUtils
{
    //Sets up services to be able to call util methods outside endpoints
    public static async Task RunStartupUtils(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<ITerritoryRepository>();
        await TerritoryGeneration.AddTerritories(1, 1, repo, CancellationToken.None);
    }
}