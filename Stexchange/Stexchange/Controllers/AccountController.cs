using Microsoft.AspNetCore.Mvc;
using Stexchange.Controllers.Exceptions;
using Stexchange.Data;
using Stexchange.Data.Models;
using Stexchange.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Controllers
{
    public class AccountController : StexChangeController
    {
        private readonly Database _db;

        public AccountController(Database db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns the MyAccount view, populated with data from the logged in user.
        /// </summary>
        /// <returns>MyAccount view iff logged in, else Login view</returns>
        public IActionResult MyAccount()
        {
            int userId = -1;
            try
            {
                userId = GetUserId();
            } catch (InvalidSessionException)
            {
                RedirectToAction("Login", "Login");
            }
            var accModel = new AccountViewModel();
            //load users data
            accModel.User = (from user in _db.Users
                             where user.Id == userId
                             select user).FirstOrDefault();
            accModel.User.Password = null; //remove sensitive data
            //load users listings
            accModel.Listings = (from listing in _db.Listings
                                 where listing.UserId == userId
                                 select new EntityBuilder<Listing>(listing)
                                    .SetProperty("Pictures", (from img in _db.Images
                                                              where img.ListingId == listing.Id
                                                              select img).FirstOrDefault())
                                    .Complete()
                                ).ToList();
            //load users rating requests
            accModel.RatingRequests = (from rr in _db.RatingRequests
                                       where rr.ReviewerId == accModel.User.Id
                                       select rr).ToList();
            TempData.Keep("RemoveListingError");
            return View(model: accModel);
        }

        /// <summary>
        /// Removes the listing with the specified id.
        /// If the User is not logged in, they will be redirected to the login view.
        /// If the Listing is not found, or the User does not own it, an error message is set.
        /// </summary>
        /// <param name="id"></param>
        private async void RemoveListing(int id)
        {
            try
            {
                _db.Remove((from listing in _db.Listings
                            where listing.Id == id && listing.UserId == GetUserId()
                            select listing).First());
            } catch (InvalidSessionException)
            {
                RedirectToAction("Login", "Login");
            } catch (InvalidOperationException)
            {
                //TODO: set error message (listing not found or unauthorised)
                TempData["RemoveListingError"] = "Aanbieding verwijderen mislukt.\n" +
                    "De aanbieding werd niet gevonden, of u bent niet gemachtigd om deze te verwijderen.";
            }
        }
    }
}
