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
            TempData.Keep("AccountControllerError");
            return View(model: accModel);
        }

        /// <summary>
        /// Deletes the specified listing and returns the MyAccount view.
        /// If the user is not logged in, redirects to the Login view.
        /// </summary>
        /// <seealso cref="RemoveListing(int)"/>
        /// <param name="id">Id of the listing</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteListing(int id)
        {
            return await RemoveListing(id);
        }

        /// <summary>
        /// Creates RatingRequests for the trade and reduces the remaining quantity.
        /// If the remaining quantity is 0, the listing is removed.
        /// </summary>
        /// <seealso cref="RemoveListing(int)"/>
        /// <param name="id">The id of the listing</param>
        /// <param name="quantity">The quantity of the trade</param>
        /// <param name="username">The user that was traded with</param>
        /// <returns></returns>
        public async Task<IActionResult> RegisterTrade(int id, uint quantity, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["AccountControllerError"] = "U moet aangeven met wie u geruild heeft om een ruil te registreren.";
                return RedirectToAction("MyAccount");
            }
            Listing listing = null;
            try
            {
                listing = (from l in _db.Listings
                                   where l.Id == id
                                   select l).First();
            }
            catch (InvalidOperationException)
            {
                TempData["AccountControllerError"] = "De listing is niet gevonden.";
                return RedirectToAction("MyAccount");
            }
            bool authorized = false;
            try
            {
                authorized = listing.UserId == GetUserId();
            } catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            }
            if (!authorized)
            {
                TempData["AccountControllerError"] = "U bent niet gemachtigd om deze actie uit te voeren.";
            }
            else if(listing.Quantity < quantity)
            {
                TempData["AccountControllerError"] = "Er kunnen niet meer planten geruild worden dan aangeboden zijn.";
            }else
            {
                listing.Quantity -= quantity;
                int? otherUserId = null;
                try
                {
                    otherUserId = (from user in _db.Users where user.Username == username select user.Id).First();
                } catch (InvalidOperationException)
                {
                    TempData["AccountControllerError"] = "De gebruiker is niet gevonden.";
                    return RedirectToAction("MyAccount");
                }
                await _db.AddAsync(new RatingRequest
                {
                    ReviewerId = GetUserId(),
                    RevieweeId = otherUserId ?? throw new InvalidOperationException()
                });
                await _db.AddAsync(new RatingRequest
                {
                    ReviewerId = otherUserId ?? throw new InvalidOperationException(),
                    RevieweeId = GetUserId()
                });
                if (listing.Quantity == quantity)
                {
                    return await RemoveListing(id);
                }
                _db.Update(listing);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("MyAccount");
        }

        /// <summary>
        /// Removes the listing with the specified id.
        /// If the User is not logged in, they will be redirected to the login view.
        /// If the Listing is not found, or the User does not own it, an error message is set.
        /// </summary>
        /// <param name="id"></param>
        private async Task<IActionResult> RemoveListing(int id)
        {
            try
            {
                _db.Remove((from listing in _db.Listings
                            where listing.Id == id && listing.UserId == GetUserId()
                            select listing).First());
            } catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            } catch (InvalidOperationException)
            {
                //TODO: set error message (listing not found or unauthorised)
                TempData["AccountControllerError"] = "Aanbieding verwijderen mislukt.\n" +
                    "De aanbieding werd niet gevonden, of u bent niet gemachtigd om deze te verwijderen.";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("MyAccount");
        }
    }
}
