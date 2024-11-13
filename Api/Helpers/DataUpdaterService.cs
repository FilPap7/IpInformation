using Common.Cache;
using DataAccess.Data;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.Helpers
{

    public class ScheduledDatabaseUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ScheduledDatabaseUpdateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Create a new scope for each use of DbContext
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<IpInformationDbContext>();
                    var cacheContext = scope.ServiceProvider.GetRequiredService<ICacheService>();
                    // Call your update method here using dbContext
                    await HelperMethods.UpdateDatabaseAsync(dbContext, cacheContext);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating database: {ex.Message}");
            }
        }
    }
}
