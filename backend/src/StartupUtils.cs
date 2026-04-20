namespace TddGame;


public static class StartupUtils
{
    //Sets up services to be able to call util methods outside endpoints
    public static async Task RunStartupUtils(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITerritoryRepository>();
        //Checks of territories are already added before creating
        var territory0 = await repo.GetTerritoryByIdAsync(1, CancellationToken.None);
        if (territory0==null) {
            await TerritoryGeneration.AddTerritories(7, 7, repo, CancellationToken.None);
        }
    }
}