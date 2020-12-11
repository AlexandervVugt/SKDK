using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stexchange.Controllers.Exceptions;
using Stexchange.Data;
using Stexchange.Data.Models;
using Stexchange.Models;

namespace Stexchange.Controllers
{
    public class TradeController : StexChangeController
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
                RenewListingCache(ref _listingCache);
                RenewUserCache(ref _userCache);
                _cacheBirth = DateTime.Now;
                _readable = true;
                _blocked = false;
            }
        }
        public IActionResult Trade()
        {
            BlockedPoller(); //Wait for our turn to read the resource

            //Shallow copy, this was accounted for in the design of this method.
            var listings = _listingCache.Values.ToList();
            listings.ForEach(listing => PrepareListing(ref listing));
            listings = (from listing in listings orderby listing.CreatedAt descending select listing).ToList();
            var tradeModel = new TradeViewModel(listings);

            //TODO: move releasing the resource to this class' Dispose method
            _blocked = false; //Release the resource
            return View(model: tradeModel);
        }

        public IActionResult Detail(int listingId)
        {
            BlockedPoller(); //Wait for our turn to read the resource

            var listing = _listingCache[listingId];
            PrepareListing(ref listing);

            //TODO: move releasing the resource to this class' Dispose method
            _blocked = false; //Release the resource
            //TODO: put the listing in a model for the detail page.
            
            
            return View("DetailAdvertisement", model: new DetailAdvertisementModel(listing, FormatFilters(listing.Filters)));
        }

        private Dictionary<string, string> FormatFilters(List<string> filters)
        {
            List<string> filteroptions = new List<string> { "light_", "water_", "plant_type_", "nutrients_", "ph_", "indigenous_", "with_pot_", "give_away_" };
            Dictionary<string, string> Filters = new Dictionary<string, string>();

            // Loops through advertisementfilters and compares each filter to each filteroptions
            for (int i = 0; i < filteroptions.Count; i++)
            {
                for (int j = 0; j < filteroptions.Count; j++)
                {
                    if (filters[i].Length >= filteroptions[j].Length &&
                        filteroptions[j] == filters[i].Substring(0, filteroptions[j].Length))
                    {
                        // Replaces filteroption name and underscores in filter value with empty strings or white spaces
                        var filterValue = filters[i].Replace(filteroptions[j], "").Replace("_", " ");
                        // Replaces all underscores in filteroption names with empty strings
                        var filterKey = filteroptions[j].Replace("_", "");
                        Filters.Add(filterKey, filterValue);
                    }
                }
            }
            return Filters;
        }

        /// <summary>
        /// Updates the state of Listings in the database that is stored in private field of this object.
        /// </summary>
        /// <param name="cache">Reference to the private field.</param>
        private void RenewListingCache(ref ConcurrentDictionary<int, Listing> cache)
        {
            var newOrModified = (from listing in _db.Listings
                                 where (!_readable || listing.LastModified >= _cacheBirth) && listing.Visible
                                 select new EntityBuilder<Listing>(listing)
                                    .SetProperty("Pictures",
                                        (from img in _db.Images
                                         where img.ListingId == listing.Id
                                         select img).ToList())
                                    .SetProperty("Filters",
                                        (from filter in _db.FilterListings
                                         where filter.ListingId == listing.Id
                                         select filter.Value).ToList())
                                    .Complete()
                        ).GetEnumerator();
            while (newOrModified.MoveNext())
            {
                cache.AddOrUpdate(newOrModified.Current.Id, newOrModified.Current,
                    (key, oldvalue) => newOrModified.Current);
            }
            newOrModified.Dispose();
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
        private void PrepareListing(ref Listing listing)
        {
            listing.Owner = _userCache[listing.UserId];
            try
            {
                listing.Distance = CalculateDistance(listing.Owner.Postal_Code);
            } catch (InvalidSessionException)
            {
                listing.Distance = -1;
            } catch (NotImplementedException)
            {
                listing.Distance = -1;
                //TODO: remove catch block
            }
            listing.OwningUserName = listing.Owner.Username;
            listing.Owner = null;
        }

        /// <summary>
        /// Calculates the distance between the logged in user
        /// and the owner of the listing.
        /// </summary>
        /// <param name="ownerPostalCode"></param>
        /// <exception cref="InvalidSessionException">If the user is not logged in.</exception>
        /// <returns>distance in km as double</returns>
        private double CalculateDistance(string ownerPostalCode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// returns the postal code of a user, based on the session
        /// </summary>
        /// <returns></returns>
        private string GetCurrentUserPostalCode()
        {
            long token = Convert.ToInt64(Request.Cookies["SessionToken"]);

            if (GetSessionData((long)token, out Tuple<int, string> data))
            {
                string user_postal = data.Item2;
                return user_postal;
            }

            return "1111AA";
        }


        /// <summary>
        /// returns the distance between two users 
        /// </summary>
        /// <returns></returns>
        private async Task<Tuple<string, string>> GetLocationAsync(string postalCode)
        {
            string uri = $"https://www.geonames.org/postalcode-search.html?q={postalCode}&country=NL";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                string readSite = await reader.ReadToEndAsync();
                int indx = readSite.IndexOf("&nbsp;&nbsp;&nbsp");
                string contains_lat_long_string = readSite.Substring(indx);
                //<small> 52.341/4.955 </small>
                int firstsmall_index = contains_lat_long_string.IndexOf("small>");
                int secondsmall_index = contains_lat_long_string.IndexOf("</small");
                string lat_long_unf = contains_lat_long_string.Substring(firstsmall_index + 6, secondsmall_index);

                int slash_index = lat_long_unf.IndexOf("/");

                string lat = lat_long_unf.Substring(0, slash_index);
                //the lat values for the netherlands range from about 6000-7000, never 10.000
                string lon = lat_long_unf.Substring(slash_index + 1, lat.Length - 1);

                Tuple<string, string> lat_long = new Tuple<string, string>(lat, lon);
                //Console.WriteLine($"lattetude: {lat_long.Item1}");
                //Console.WriteLine($"longtitude: {lat_long.Item2}");
                return lat_long;
            }
        }

        /// <summary>
        /// returns the distance between two users 
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetDistance(string postalCode_current_user, string postalCode_listing_user)
        {
            Tuple<string, string> lat_long_current_user = await GetLocationAsync(postalCode_current_user);
            Tuple<string, string> lat_long_listing_user = await GetLocationAsync(postalCode_listing_user);
            int lat_current_us;
            int lon_current_us;
            int.TryParse(lat_long_current_user.Item1, out lat_current_us);
            int.TryParse(lat_long_current_user.Item2, out lon_current_us);
            int lat_listing_us;
            int lon_listing_us;
            int.TryParse(lat_long_listing_user.Item1, out lat_listing_us);
            int.TryParse(lat_long_listing_user.Item2, out lon_listing_us);

            var cCoord = new GeoCoordinate(lat_current_us, lon_current_us);
            var lCoord = new GeoCoordinate(lat_listing_us, lon_listing_us);

            return cCoord.GetDistanceTo(lCoord) / 1000; //to km
        }
    }
}
