using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;

namespace ST10335260_PROG6212_POE_Part2.Seeding;

public static class DatabaseInitialiser
{
    public static async Task InitialiseAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        const int maxAttempts = 20;
        var includeDemoData = configuration.GetValue("Seed:DemoData", false);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
                await db.Database.MigrateAsync();
                await DbSeeder.SeedAsync(db, includeDemoData);
                logger.LogInformation("Database is ready.");
                return;
            }
            catch (SqlException ex) when (attempt < maxAttempts)
            {
                logger.LogWarning("Database not ready yet (attempt {Attempt}/{Max}): {Message}", attempt, maxAttempts, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialisation failed.");
                return;
            }
        }
    }
}
