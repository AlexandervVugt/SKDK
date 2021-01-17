using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            listings.ForEach(listing => listing.Owner.Rating = new User.RatingAggregation(
                                        from rating in _db.Ratings where rating.RevieweeId == listing.UserId select rating));
            listings = (from listing in listings orderby listing.CreatedAt descending select listing).ToList();
            try
            {
                int userId = GetUserId();
                listings = (from listing in listings
                            where !((from b in _db.Blocks
                                     where b.BlockerId == userId
                                     select b.BlockedId).ToList().Contains(listing.UserId)) && !((from b in _db.Blocks
                                                                                                  where b.BlockedId == userId
                                                                                                  select b.BlockerId).ToList().Contains(listing.UserId))
                            orderby listing.CreatedAt 
                            descending
                            select listing).ToList();
            }
            catch(InvalidSessionException)
            {
                listings = (from listing in listings
                            orderby listing.CreatedAt 
                            descending
                            select listing).ToList();
            }


            

            var tradeModel = new TradeViewModel(listings);
 
            _blocked = false; //Release the resource
            return View(model: tradeModel);
        }

        public IActionResult Detail(int listingId)
        {
            BlockedPoller(); //Wait for our turn to read the resource

            var listing = _listingCache[listingId];
            PrepareListing(ref listing);

            listing.Owner.Rating = new User.RatingAggregation(
                            from rating in _db.Ratings where rating.RevieweeId == listing.UserId select rating);

            _blocked = false; //Release the resource
            //TODO: put the listing in a model for the detail page.
            
            try
            {
                return View("DetailAdvertisement", model: new DetailAdvertisementModel(listing, FormatFilters(listing.Filters), GetUserId()));
            }
            catch (InvalidSessionException)
            {
                return View("DetailAdvertisement", model: new DetailAdvertisementModel(listing, FormatFilters(listing.Filters), -1));
            }
            
        }

        private Dictionary<string, string> FormatFilters(List<string> filters)
        {
            List<string> filteroptions = new List<string> { "light_", "water_", "plant_type_", "nutrients_", "ph_", "indigenous_", "with_pot_", "give_away_", "plant_order_" };
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
                return Math.Round(distance/1000, 2); //to km
            } catch(Exception e)
            {
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
        /// </summary>
        /// <param name="advertisements"></param>
        /// <param name="searchbar"></param>
        /// <param name="search_description"></param>
        /// <returns></returns>
        public List<Listing> Searchbar(ICollection<Listing> advertisements, string searchbar, bool search_description)
        {
            List<Listing> searchList = new List<Listing>();

            if (string.IsNullOrEmpty(searchbar) || string.IsNullOrWhiteSpace(searchbar))
            {
                foreach (Listing advertisement in advertisements)
                {
                    searchList.Add(advertisement);
                }
                return searchList;
            }
            searchbar = searchbar.ToLower();
            // adds each advertisement to new searchList is the title contains the search value
            foreach (Listing advertisement in advertisements)
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
            return searchList;
        }

        /// <summary>
        /// Checks if advertisement in list of advertisements contains at least one of the selected filters in every filtercatagory.
        /// </summary>
        /// <param name="light"></param>
        /// <param name="indigenous"></param>
        /// <param name="ph"></param>
        /// <param name="nutrients"></param>
        /// <param name="water"></param>
        /// <param name="plant_type"></param>
        /// <param name="give_away"></param>
        /// <param name="with_pot"></param>
        /// <param name="recent_toggle"></param>
        /// <param name="recent"></param>
        /// <param name="distance_toggle"></param>
        /// <param name="distance"></param>
        /// <returns> A new trademodel with filtered advertisements to view </returns>
        //[HttpGet]
        public List<Listing> FilterSearch(List<Listing> advertisements, string[] light, string[] indigenous, string[] ph, string[] nutrients, string[] water, string[] plant_type, string[] plant_order, string[] give_away, string[] with_pot, bool recent_toggle, int recent, bool distance_toggle, int distance, bool rating_toggle, int rating)
        {
            // List of advertisements that contains selected filter values and search input
            List<Listing> searchList = new List<Listing>();

            // All plant filters
            List<string[]> filters = new List<string[]> { light, indigenous, ph, nutrients, water, plant_type, plant_order, give_away, with_pot };

            // Adds selected plant filters with a length greater than 0 to a new list
            List<string[]> selectedFilters = new List<string[]>();
            foreach (var filter in filters)
            {
                if (filter.Length > 0)
                {
                    selectedFilters.Add(filter);
                }
            }

            // All toggle filters
            // Adds selected toggle filters which are true to a new list
            List<bool> extraFilters = new List<bool>() { recent_toggle, distance_toggle, rating_toggle };
            List<bool> selectedExtraFilters = new List<bool>();
            foreach (var filter in extraFilters)
            {
                if (filter == true)
                {
                    selectedExtraFilters.Add(filter);
                }
            }

            // Adds all advertisement which contains selected filters
            foreach (Listing advertisement in advertisements)
            {
                int check = 0;
                // Loops through all filters
                foreach (var filter in filters)
                {
                    for (int i = 0; i < filter.Length; i++)
                    {
                        if (advertisement.Filters.Contains(filter[i]))
                        {
                            check++;
                        }
                    }
                }

                if (recent_toggle == true && advertisement.CreatedAt >= DateTime.Now.AddDays(-recent))
                {
                    check++;
                }

                advertisement.Distance = GetDistance(_userCache[advertisement.UserId].Postal_Code);
                if (distance_toggle == true && (advertisement.Distance <= distance))
                {
                    check++;
                }

                // Checks if the average of communication or quantity is average. If yes it will only get the average which is not equal to 0
                // If they both contain an average > 0 it will divide the sum of both averages by 2
                int advertisementRating = advertisement.Owner.Rating.QualityAvg > 0 ? advertisement.Owner.Rating.CommunicationAvg > 0 ? (int)Math.Round(((advertisement.Owner.Rating.QualityAvg) + (advertisement.Owner.Rating.CommunicationAvg)) / 2, 0, MidpointRounding.AwayFromZero) : (int)Math.Round(advertisement.Owner.Rating.QualityAvg, 0, MidpointRounding.AwayFromZero) : (int)Math.Round(advertisement.Owner.Rating.CommunicationAvg, 0, MidpointRounding.AwayFromZero);
                if (rating_toggle == true && advertisementRating == rating)
                {
                    check++;
                }


                // Checks if check count equals selected filters count
                if ((check == selectedFilters.Count + selectedExtraFilters.Count))
                {
                    searchList.Add(advertisement);
                }
            }
            return searchList;
        }

        /// <summary>
        ///Sorts advertisements in list with selected filter value.
        /// If list is empty, it will create a new list of advertisements
        /// </summary>
        /// <param name="advertisements"></param>
        /// <param name="searchList"></param>
        /// <param name="sort_distance"></param>
        /// <param name="sort_time"></param>
        /// <returns></returns>
        public List<Listing> SortSearch(List<Listing> searchList, bool sort_distance, bool sort_time)
        {
            if (searchList.Count > 0) {
                searchList.ForEach(listing => PrepareListing(ref listing));
                searchList.ForEach(listing => listing.Owner.Rating = new User.RatingAggregation(
                        from rating in _db.Ratings where rating.RevieweeId == listing.UserId select rating));
            }

            if (sort_time == true) { searchList = (from advertisement in searchList orderby advertisement.CreatedAt descending select advertisement).ToList(); }
            else if (sort_distance == true) { searchList = (from advertisement in searchList orderby advertisement.Distance, advertisement.CreatedAt ascending select advertisement).ToList(); }

            return searchList;
        }


        /// <summary>
        /// Filters advertisements with searched value, selected filters or selected sort options.
        /// </summary>
        /// <param name="searchbar"></param>
        /// <param name="search_description"></param>
        /// <param name="light"></param>
        /// <param name="indigenous"></param>
        /// <param name="ph"></param>
        /// <param name="nutrients"></param>
        /// <param name="water"></param>
        /// <param name="plant_type"></param>
        /// <param name="give_away"></param>
        /// <param name="with_pot"></param>
        /// <param name="recent_toggle"></param>
        /// <param name="recent"></param>
        /// <param name="distance_toggle"></param>
        /// <param name="distance"></param>
        /// <param name="sort_distance"></param>
        /// <param name="sort_time"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Search(string searchbar, bool search_description, string[] light, string[] indigenous, string[] ph, string[] nutrients, string[] water, string[] plant_type, string[] plant_order, string[] give_away, string[] with_pot, bool recent_toggle, int recent, bool distance_toggle, int distance, bool rating_toggle, int rating, bool sort_distance, bool sort_time)
        {
            ICollection<Listing> advertisements = _listingCache.Values;
            List<Listing> searchList = FilterSearch(Searchbar(advertisements, searchbar, search_description), light, indigenous, ph, nutrients, water, plant_type, plant_order, give_away, with_pot, recent_toggle, recent, distance_toggle, distance, rating_toggle, rating);

            if (sort_distance == true || sort_time == true)
            {
                searchList = SortSearch(searchList, sort_distance, sort_time);
            }
            else
            {
                if (searchList.Count > 0) {
                    searchList.ForEach(listing => PrepareListing(ref listing));
                    searchList.ForEach(listing => listing.Owner.Rating = new User.RatingAggregation(
                                        from rating in _db.Ratings where rating.RevieweeId == listing.UserId select rating));
                } 
                searchList = (from advertisement in searchList orderby advertisement.CreatedAt descending select advertisement).ToList();
            }
            if (Request.Cookies.ContainsKey(StexChangeController.Cookies.SessionToken))
            {
                List<int> blockedUsers = (from b in _db.Blocks
                                          where b.BlockerId == GetUserId()
                                          select b.BlockedId).ToList();
                List<int> blockerUsers = (from b in _db.Blocks
                                          where b.BlockedId == GetUserId()
                                          select b.BlockerId).ToList();
                searchList.RemoveAll(l => blockedUsers.Contains(l.UserId) || blockerUsers.Contains(l.UserId));

            }

            TempData["SearchResults"] = searchList.Count;

            return View("trade", new TradeViewModel(searchList));
        }
        public IActionResult Block(int listingId)
        {
          try
           {
                int blockedUserId = (from l in _db.Listings
                                     where (l.Id == listingId)
                                     select l.UserId).FirstOrDefault();
                    var newBlock = new Block
                    {
                        BlockerId = GetUserId(),
                        BlockedId = blockedUserId
                    };
                     _db.Blocks.Add(newBlock);
                     _db.SaveChanges();
                    return RedirectToAction("Trade", "Trade");
                

            }
            catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            }

        }
    }
}
