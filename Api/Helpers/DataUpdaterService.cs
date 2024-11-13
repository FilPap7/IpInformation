using Common.Cache;
using DataAccess.Data;

namespace IpInformation.Helpers
{

    public class ScheduledDatabaseUpdateService : BackgroundService
    {
        private readonly IpInformationDbContext _dbContext;
        private readonly ICacheService _cacheContext;

        public ScheduledDatabaseUpdateService(IpInformationDbContext context, ICacheService cache)
        {
            _dbContext = context;
            _cacheContext = cache;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initial delay if needed
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Call the update method
                    await HelperMethods.UpdateDatabaseAsync(_dbContext, _cacheContext);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating database: {ex.Message}");
                }

                // Wait for the next interval
                await Task.Delay(Constants.DatabaseUpdateIntervalTime, stoppingToken);
            }
        }
    }
}
