using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stexchange.Data;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Stexchange.Services
{
    public class DatabaseCleanupService : IHostedService
    {
        public DatabaseCleanupService(IServiceScopeFactory scopefactory, ILogger<DatabaseCleanupService> logger)
        {
            scopeFactory = scopefactory;
            Log = logger;
        }

        private readonly IServiceScopeFactory scopeFactory;
        private ILogger Log { get; }

        // Will be called during application start
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Time to wait until first deployment.
            var startTimeSpan = DateTime.Now.Date.AddDays(1) - DateTime.Now;

            // Calls method every day
            Timer timer = new Timer(async e => await RemoveAdvertisements(), null, startTimeSpan, TimeSpan.FromDays(1));

            // Returns Task.CompletedTask because it's not async
            return Task.CompletedTask;
        }
        
        public async Task RemoveAdvertisements()
        {
            // Creates a new IServiceScope
            // Allows the use of services/db contexts etc.
            using (var scope = scopeFactory.CreateScope())
            {
                Database database = scope.ServiceProvider.GetRequiredService<Database>();
                foreach (var advertisement in database.Listings)
                {
                    // Checks if advertisement is older than 8 weeks
                    int weeks = (int)((DateTime.Now - advertisement.CreatedAt).TotalDays / 7);
                    if (weeks > 8)
                    {
                        database.Remove(advertisement);
                        Log.LogTrace($"Removed advertisement ID {advertisement.Id} from user {advertisement.UserId}");
                    }
                }
                await database.SaveChangesAsync();
                Log.LogTrace($"Saved database changes");
            }
        }

        // Will be called during application stop
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Returns Task.CompletedTask because it's not async
            return Task.CompletedTask;
        }
    }
}

