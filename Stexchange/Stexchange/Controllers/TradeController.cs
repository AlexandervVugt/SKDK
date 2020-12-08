﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stexchange.Data;
using Stexchange.Data.Builders;
using Stexchange.Data.Models;
using Stexchange.Models;

namespace Stexchange.Controllers
{
    public class TradeController : Controller
    {
        private Database _db;

        private static bool _blocked = false;
        private static DateTime _cacheBirth;
        private static Func<TimeSpan> _cacheAge = () =>
        {
            return DateTime.Now - _cacheBirth;
        };
        private static bool _readable = false;
        private static ConcurrentDictionary<int, Listing> _listingCache = new ConcurrentDictionary<int, Listing>();
        private static ConcurrentDictionary<int, User> _userCache;
        public TradeController(Database db)
        {
            _db = db;
            //load data if necessary
            if(!_readable || (_cacheAge() > TimeSpan.FromSeconds(60) && !_blocked))
            {
                _blocked = true;
                _cacheBirth = DateTime.Now;
                RenewListingCache(ref _listingCache);
                RenewUserCache(ref _userCache);
                _readable = true;
                _blocked = false;
            }
        }
        public IActionResult Trade(long token)
        {
            BlockedPoller(); //Wait for our turn to read the resource

            //Shallow copy, this was accounted for in the design of this method.
            var listings = _listingCache.Values.ToList();
            listings.ForEach(listing => PrepareListing(token, ref listing));
            var tradeModel = new TradeViewModel(listings);

            //TODO: move releasing the resource to this class' Dispose method
            _blocked = false; //Release the resource
            return View(model: tradeModel);
        }

        public IActionResult Detail(long token, int listingId)
        {
            BlockedPoller(); //Wait for our turn to read the resource

            var listing = _listingCache[listingId];
            PrepareListing(token, ref listing);

            //TODO: move releasing the resource to this class' Dispose method
            _blocked = false; //Release the resource
            //TODO: put the listing in a model for the detail page.
            return View();
        }

        /// <summary>
        /// Updates the state of Listings in the database that is stored in private field of this object.
        /// </summary>
        /// <param name="cache">Reference to the private field.</param>
        private void RenewListingCache(ref ConcurrentDictionary<int, Listing> cache)
        {
            var newOrModified = (from listing in _db.Listings
                                 where (!_readable || listing.LastModified >= _cacheBirth)
                                 select new ListingBuilder(listing)
                                    /*.SetProperty("Pictures",
                                        (from img in _db.Images
                                         where img.ListingId == listing.Id
                                         select img).ToList())*/
                                    .SetProperty("Categories",
                                        (from filter in _db.FilterListings
                                         where filter.ListingId == listing.Id
                                         select filter.Value).ToList())
                                    .SetProperty("Owner", _userCache[listing.UserId])
                                    .Complete()
                        ).GetEnumerator();
            while (newOrModified.MoveNext())
            {
                cache.AddOrUpdate(newOrModified.Current.Id, newOrModified.Current,
                    (key, oldvalue) => newOrModified.Current);
            }
        }

        /// <summary>
        /// Updates the state of Users in the database that is stored in private field of this object.
        /// </summary>
        /// <param name="cache">Reference to the private field.</param>
        private void RenewUserCache(ref ConcurrentDictionary<int, User> cache)
        {
            var queryResult = (from user in _db.Users
                               join listing in _db.Listings on user.Id equals listing.UserId
                               select user).ToArray();
            var buffer = new ConcurrentDictionary<int, User>();
            Array.ForEach(queryResult, (user) =>
            {
                buffer.TryAdd(user.Id, user);
            });
            cache = buffer;
        }

        /// <summary>
        /// When the first request to the controller is made,
        /// the controller will store the data from the database in a static field.
        /// While this is happening, all instances of this class will have !_readable and _blocked.
        /// After that, if the stored data is older than 60 seconds, the next request to the controller
        /// will update the data. While this is happening, all instances of this class will have _blocked.
        /// While either of this is happening, it is not guaranteed that a data access will be valid.
        /// Therefore, all other instances of the controller will wait for completion,
        /// by polling every 100 ms(10 times per second).
        /// </summary>
        private void BlockedPoller()
        {
            while (_blocked || !_readable)
            {
                Thread.Sleep(100);
            }
            _blocked = true;
        }

        /// <summary>
        /// Combines the known data in the given listing object.
        /// </summary>
        /// <param name="token">The user whose data to use, if logged in.</param>
        /// <param name="listing">The given listing</param>
        private void PrepareListing(long? token, ref Listing listing)
        {
            listing.Owner = _userCache[listing.UserId];
            if (token is object && ServerController.GetSessionData((long)token, out Tuple<int, string> sessionData))
            {
                listing.Distance = calculateDistance(listing.Owner.Postal_Code, sessionData.Item2);
            }
            else
            {
                listing.Distance = -1;
            }
            listing.OwningUserName = listing.Owner.Username;
            listing.Owner = null;
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
    }
}
