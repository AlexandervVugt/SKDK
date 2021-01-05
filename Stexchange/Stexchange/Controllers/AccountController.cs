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
            return View(model: accModel);
        }
    }
}
