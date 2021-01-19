using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stexchange.Data;
using Stexchange.Data.Models;
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
            Timer ratingTimer = new Timer(async e => await RemoveRatingRequests(), null, startTimeSpan, TimeSpan.FromDays(1));

            // Returns Task.CompletedTask because it's not async
            return Task.CompletedTask;
        }

        /// <summary>
        /// Databasecleanupservice has a singleton lifetime, which is a longer lifetime than scoped.
        /// Database contexts are scoped and shouldn't be kept alive indefinitely.
        /// With scopeFactory you can create a new scope and use the db contexts whenever you need it.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveRatingRequests()
        {
            // Creates a new IServiceScope
            // Allows the use of services/db contexts etc.
            using (var scope = scopeFactory.CreateScope())
            {
                Database database = scope.ServiceProvider.GetRequiredService<Database>();
                foreach (RatingRequest rr in database.RatingRequests)
                {
                    if((DateTime.Now - rr.CreatedAt) >= TimeSpan.FromDays(3))
                    {
                        database.Remove(rr);
                    }
                }
                await database.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Databasecleanupservice has a singleton lifetime, which is a longer lifetime than scoped.
        /// Database contexts are scoped and shouldn't be kept alive indefinitely.
        /// With scopeFactory you can create a new scope and use the db contexts whenever you need it.
        /// </summary>
        /// <returns></returns>
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
                    }else if (weeks > 2 && !advertisement.Renewed && advertisement.Visible)
                    {
                        //advertisement.Visible = false;
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

