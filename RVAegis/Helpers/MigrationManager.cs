using Microsoft.EntityFrameworkCore;
using RVAegis.Contexts;

namespace RVAegis.Helpers
{
    public class MigrationManager(ILogger<MigrationManager> logger)
    {
        public async Task<IHost> MigrateDatabaseAsync(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using var appContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                try
                {
                    await appContext.Database.MigrateAsync();
                    await ApplicationContextSeed.SeedAsync(appContext);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during database migration or seeding.");
                    logger.LogError(ex, "StackTrace: {StackTrace}", ex.StackTrace);
                    throw;
                }
            }

            return host;
        }
    }
}
