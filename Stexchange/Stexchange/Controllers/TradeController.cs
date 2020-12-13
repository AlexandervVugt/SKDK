using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Http;
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
                listing.Distance = GetDistance(listing.Owner.Postal_Code);
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
        /// returns the postal code of a user, based on the session
        /// </summary>
        /// <returns></returns>
        private string GetCurrentUserPostalCode()
        {
            long token = Convert.ToInt64(Request.Cookies["SessionToken"]);

            if (GetSessionData(token, out Tuple<int, string> data))
            {
                string user_postal = data.Item2;
                return user_postal;
            }

            return "-1";
        }


        /// <summary>
        /// returns the distance between two users 
        /// </summary>
        /// <returns></returns>
        private Tuple<string, string> GetLocationAsync(string postalCode)
        {
            string uri = $"https://www.geonames.org/postalcode-search.html?q={postalCode}&country=NL";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                string readSite = reader.ReadToEnd();

                int tableStart = readSite.IndexOf("<table class=\"restable\">");
                if (tableStart == -1)
                {
                    throw new Exception("Postal code not recognized");
                }
                int tableEnd = readSite.IndexOf("</table>", tableStart);
                string[] dataContent = readSite.Substring(tableStart, tableEnd - tableStart)
                    .Split("small")[3]
                    .Replace("<", null)
                    .Replace(">", null)
                    .Split('/');
                return new Tuple<string, string>(dataContent[0], dataContent[1]);
            }
        }

        /// <summary>
        /// Calculates the distance between the logged in user
        /// and the owner of the listing.
        /// </summary>
        /// <returns></returns>
        public double GetDistance( string postalCode_listing_user)
        {
            try
            {
                Tuple<string, string> lat_long_current_user = GetLocationAsync(GetCurrentUserPostalCode());
                Tuple<string, string> lat_long_listing_user = GetLocationAsync(postalCode_listing_user);
                double.TryParse(lat_long_current_user.Item1, out double lat_current_us);
                double.TryParse(lat_long_current_user.Item2, out double lon_current_us);

                double.TryParse(lat_long_listing_user.Item1, out double lat_listing_us);
                double.TryParse(lat_long_listing_user.Item2, out double lon_listing_us);


                var cCoord = new GeoCoordinate(lat_current_us/1000, lon_current_us/1000);
                var lCoord = new GeoCoordinate(lat_listing_us/1000, lon_listing_us/1000);

                var distance = cCoord.GetDistanceTo(lCoord);
                Console.WriteLine($"resulting distance : {distance}");
                return Math.Round(distance/1000, 2); //to km
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                if(e.Message == "Postal code not recognized")
                {
                    return -1;
                } else
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Checks if there are any advertisements that contains the searched value.
        /// If yes, the advertisement will be added to a new list.
        /// Returns to trade view with a new TradeViewModel with a new listing List to display the requested listings
        /// </summary>
        /// <param name="searchbar"> search value </param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Search(string searchbar, bool search_description) 
        {
            List<Listing> searchList = new List<Listing>();
            
            if (string.IsNullOrEmpty(searchbar) || string.IsNullOrWhiteSpace(searchbar))
            {
                return RedirectToAction("Trade");
            }
            searchbar = searchbar.ToLower();
            // adds each advertisement to new searchList is the title contains the search value
            foreach (Listing advertisement in _listingCache.Values)
            {
                if (search_description == true)
                {
                    if (advertisement.Title.ToLower().Contains(searchbar) || advertisement.Description.ToLower().Contains(searchbar) || advertisement.NameNl.ToLower().Contains(searchbar))
                    {
                        searchList.Add(advertisement);
                    }
                    else if (advertisement.NameLatin != null)
                    {
                        if (advertisement.NameLatin.ToLower().Contains(searchbar)) searchList.Add(advertisement);
                    }
                }
                else
                {
                    if (advertisement.Title.ToLower().Contains(searchbar) || advertisement.NameNl.ToLower().Contains(searchbar))
                    {
                        searchList.Add(advertisement);
                    }
                    else if (advertisement.NameLatin != null)
                    {
                        if (advertisement.NameLatin.ToLower().Contains(searchbar)) searchList.Add(advertisement);
                    }
                }
            }

            if(searchList.Count > 0) searchList.ForEach(listing => PrepareListing(ref listing)); 
            searchList = (from advertisement in searchList orderby advertisement.CreatedAt descending select advertisement).ToList();
            TempData["SearchResults"] = searchList.Count;

            return View("trade", new TradeViewModel(searchList));
        }
    }


}
