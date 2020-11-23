﻿using Microsoft.Extensions.Logging;
using Stexchange.Controllers;
using Stexchange.Data;
using Stexchange.Data.Builders;
using Stexchange.Data.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stexchange.Models
{
    public sealed class TradeViewModel : IDisposable
    {
        private Database db;
        private ILogger log;
        private Thread cacheWorker;

        private bool blocked = true;
        private ConcurrentDictionary<int, Listing> listingCache;
        private ConcurrentDictionary<int, User> userCache;

        public TradeViewModel(Database db, ILogger<TradeViewModel> logger)
        {
            this.db = db;
            log = logger;
            listingCache = new ConcurrentDictionary<int, Listing>();
            cacheWorker = new Thread(() =>
            {
                var task = Run();
                task.Wait();
            });
            cacheWorker.Start();
        }

        /// <summary>
        /// Updates the state of Listings in the database that is stored in private field of this object.
        /// </summary>
        /// <param name="cache">Reference to the private field.</param>
        private void renewListingCache(ref ConcurrentDictionary<int, Listing> cache)
        {
            var start = DateTime.Now;
            var newOrModified = (from listing in db.Listings
                     where (blocked || listing.LastModified >= start)
                     select new ListingBuilder(listing)
                        .SetProperty("Pictures",
                            (from img in db.Images
                             where img.ListingId == listing.Id
                             select img).ToList())
                        .SetProperty("Categories",
                            (from filter in db.FilterListings
                             where filter.ListingId == listing.Id
                             select filter.Value).ToList())
                        .SetProperty("Owner", userCache[listing.UserId])
                        .Complete()
                        ).GetEnumerator();
            while (newOrModified.MoveNext())
            {
                cache.AddOrUpdate(newOrModified.Current.Id, newOrModified.Current,
                    (key, oldvalue) => newOrModified.Current);
            }
            var elapsed = DateTime.Now - start;
            log.LogTrace($"Finished renewing Listing cache.\nTime elapsed: {elapsed}");
        }

        /// <summary>
        /// Updates the state of Users in the database that is stored in private field of this object.
        /// </summary>
        /// <param name="cache">Reference to the private field.</param>
        private void renewUserCache(ref ConcurrentDictionary<int, User> cache)
        {
            var start = DateTime.Now;
            var queryResult = (from user in db.Users
                    join listing in db.Listings on user.Id equals listing.UserId
                    select user).ToArray();
            var buffer = new ConcurrentDictionary<int, User>();
            Array.ForEach(queryResult, (user) =>
            {
                buffer.TryAdd(user.Id, user);
            });
            cache = buffer;
            var elapsed = DateTime.Now - start;
            log.LogTrace($"Finished renewing Listing cache.\nTime elapsed: {elapsed}");
        }

        /// <summary>
        /// Retrieves all listings from the cache.
        /// </summary>
        /// <param name="token">Users session token. Used to calculate distance.
        /// If null is passed, the distance will be a default value.</param>
        /// <returns></returns>
        public List<Listing> RetrieveListings(long? token)
        {
            /*When the first request to retrieve the listings is made,
             *a TradeViewModel is initialized and starts retrieving the data
             *from the database in a separate thread. To prevent the user
             *from receiving incomplete data, this method will wait
             *for the cacheWorker thread to un-block the instance,
             *by polling every 100 ms (10 times per second).
             */
            while (blocked)
            {
                Thread.Sleep(100);
            }

            //Shallow copy, this was accounted for in the design of this method.
            var listings = listingCache.Values.ToList();
            //Each listing is temporarily assigned it's owner.
            listings.ForEach(listing => listing.Owner = userCache[listing.UserId]);
            if(token is object && ServerController.GetSessionData((long) token, out Tuple<int, string> sessionData)) {
                listings.ForEach(listing => listing.Distance = calculateDistance(listing.Owner.Postal_Code, sessionData.Item2));
            } else
            {
                listings.ForEach(listing => listing.Distance = -1);
            }
            listings.ForEach((listing) =>
            {
                listing.OwningUserName = listing.Owner.Username;
                listing.Owner = null;
            });
            return listings;
        }

        /// <summary>
        /// Calculates the distance between two postal codes
        /// </summary>
        /// <param name="ownerPostalCode"></param>
        /// <param name="myPostalCode"></param>
        /// <returns>distance in km as double</returns>
        private double calculateDistance(string ownerPostalCode, string myPostalCode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Task that renews the cache every minute.
        /// </summary>
        /// <returns>Task</returns>
        private async Task Run()
        {
            do
            {
                await Task.Run(() =>
                {
                    renewUserCache(ref userCache);
                    renewListingCache(ref listingCache);
                    blocked = false;
                });
                await Task.Delay(60000);
            } while (true);
        }

        public void Dispose()
        {
            cacheWorker = null;
        }
    }
}
