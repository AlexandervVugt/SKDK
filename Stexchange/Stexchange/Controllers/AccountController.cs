using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stexchange.Controllers.Exceptions;
using Stexchange.Data;
using Stexchange.Data.Models;
using Stexchange.Data.Validation;
using Stexchange.Models;
using Stexchange.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stexchange.Data.Helpers;
using static Stexchange.Controllers.StexChangeController;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Stexchange.Controllers
{
    public class AccountController : StexChangeController
    {

        private readonly Database _db;

        public AccountController(Database db, EmailService emailService)
        {
            _db = db;
            EmailService = emailService;
        }

        private EmailService EmailService { get; }

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
                return RedirectToAction("Login", "Login");
            }
            var accModel = new AccountViewModel();
            //load users data
            accModel.User = (from user in _db.Users
                             where user.Id == userId
                             select user).First();
            accModel.User.Password = null; //remove sensitive data
            accModel.User.Rating = new User.RatingAggregation(
                from rating in _db.Ratings where rating.RevieweeId == userId select rating);
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
                                       select rr).Include("Reviewee").ToList();
            TempData.Keep("AccountControllerError");
            return View(model: accModel);
        }

        /// <summary>
        /// Deletes the specified listing and returns the MyAccount view.
        /// If the user is not logged in, redirects to the Login view.
        /// If the Listing is not found, or the User does not own it, an error message is set.
        /// </summary>
        /// <seealso cref="RemoveListing(int)"/>
        /// <param name="id">Id of the listing</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteListing(int listingId)
        {
            try
            {
                _db.Remove((from listing in _db.Listings
                            where listing.Id == listingId && listing.UserId == GetUserId()
                            select listing).First());
                await _db.SaveChangesAsync();
                TempData.Remove("AccountControllerError");
            }
            catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            }
            catch (InvalidOperationException)
            {
                //TODO: set error message (listing not found or unauthorised)
                TempData["AccountControllerError"] = "Aanbieding verwijderen mislukt.\n" +
                    "De aanbieding werd niet gevonden, of u bent niet gemachtigd om deze te verwijderen.";
            }
            return RedirectToAction("MyAccount");
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
        public async Task<IActionResult> RegisterTrade(int listingId, uint quantity, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["AccountControllerError"] = "U moet aangeven met wie u geruild heeft om een ruil te registreren.";
                return RedirectToAction("MyAccount");
            }else if (quantity == 0)
            {
                TempData["AccountControllerError"] = "Ongeldige quantiteit";
                return RedirectToAction("MyAccount");
            }
            Listing listing = null;
            try
            {
                listing = (from l in _db.Listings
                                   where l.Id == listingId
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
                    RevieweeId = otherUserId ?? throw new InvalidOperationException(),
                    PlantName = listing.NameNl,
                    RequestQuality = false
                });
                await _db.AddAsync(new RatingRequest
                {
                    ReviewerId = otherUserId ?? throw new InvalidOperationException(),
                    RevieweeId = GetUserId(),
                    PlantName = listing.NameNl,
                    RequestQuality = true
                });
                if (listing.Quantity == 0)
                {
                    return RedirectToAction("DeleteListing", new { listingId });
                }
                _db.Update(listing);
                await _db.SaveChangesAsync();
                TempData.Remove("AccountControllerError");
            }
            return RedirectToAction("MyAccount");
        }

        /// <summary>
        /// Returns a string array of users that interacted with the specified listing.
        /// </summary>
        /// <param name="listingId"></param>
        /// <returns></returns>
        public string[] GetInteractingUsers(int listingId)
        {
            return (from chat in _db.Chats
                    join user in _db.Users on chat.ResponderId equals user.Id
                    where chat.AdId == listingId
                    select user.Username).ToArray();
        }

        /// <summary>
        /// Returns the quantity of the specified listing
        /// </summary>
        /// <param name="listingId"></param>
        /// <returns></returns>
        public uint GetQuantity(int listingId)
        {
            return (from listing in _db.Listings
                    where listing.Id == listingId
                    select listing.Quantity).FirstOrDefault();
        }

        /// <summary>
        /// Sets the visibility of the specified listing to the specified value.
        /// </summary>
        /// <param name="listingId"></param>
        /// <param name="value"></param>
        /// <returns>A json string that specifies status and a message.</returns>
        public string SetVisible(int listingId, bool value)
        {
            string message = "";
            try
            {
                Listing listing = (from li in _db.Listings
                                   where li.Id == listingId && li.UserId == GetUserId()
                                   select li).First();
                listing.Visible = value;
                _db.Update(listing);
                _db.SaveChanges();
                message = $"Gelukt!\nUw advertentie \"{listing.Title}\" is nu {(listing.Visible ? "actief" : "inactief")}";
            }catch (InvalidOperationException)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                message = "De aanbieding bestaat niet, of u bent niet gemachtigd om deze aan te passen.";
            }catch (InvalidSessionException)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                message = "Sessie bestaat niet of is verlopen.";
            }
            return message;
        }


        public async Task<object> ChangeAccountSettings(string username, string postalcode, string email, string password, string confirm_password)
        {
            try {
                var dbUser = (from user in _db.Users
                              where user.Id == GetUserId()
                              select user).First();

                ChangeAccountSettingsValidator registerValidator = new ChangeAccountSettingsValidator();
                ValidationResult resultsValidator = registerValidator.Validate(new AccountSettingsModel()
                {
                    Username = username ?? "",
                    Postalcode = postalcode ?? "",
                    Email = email ?? "",
                    Password = password ?? "",
                    Confirm_Password = confirm_password ?? ""
                });

                List<string> errormessages = new List<string>();

                if (!resultsValidator.IsValid)
                {
                    foreach (ValidationFailure error in resultsValidator.Errors)
                    {
                        errormessages.Add(error.ErrorMessage);
                    }
                };

                // Checks if email already exists in database
                if (_db.Users.Any(u => u.Email == email && u.Id != dbUser.Id))
                {
                    errormessages.Add("E-mail is al gebruikt");
                }

                // Checks if username already exists in database
                if (_db.Users.Any(u => u.Username == username && u.Id != dbUser.Id))
                {
                    errormessages.Add("Gebruikersnaam is al bezet");
                }

                if (errormessages.Count > 0)
                {
                    return errormessages;
                }

                if (!(dbUser is null))
                {
                    if (!(string.IsNullOrWhiteSpace(username)) && dbUser.Username != username)
                    {
                        dbUser.Username = username;
                    }
                    if (!(string.IsNullOrWhiteSpace(email)) && dbUser.Email != email)
                    {
                        var userverification = (from code in _db.UserVerifications
                                                where code.Id == dbUser.Id
                                                select code).FirstOrDefault();

                        var verification = userverification ?? new UserVerification() { Guid = Guid.NewGuid() };

                        dbUser.Email = email;
                        dbUser.IsVerified = false;
                        dbUser.Verification = verification;

                        string body = $@"STEXCHANGE
Verifieer je e-mailadres door op de onderstaande link te klikken
https://{ControllerContext.HttpContext.Request.Host}/login/Verification/{verification.Guid}";
                        SendEmail(email, body);
                    }
                    if (!(string.IsNullOrWhiteSpace(postalcode)) && dbUser.Postal_Code != postalcode)
                    {
                        dbUser.Postal_Code = postalcode;
                    }
                    if (!(string.IsNullOrWhiteSpace(password)) && dbUser.Password != CreatePasswordHash(password, dbUser.Id.ToString()))
                    {
                        dbUser.Password = CreatePasswordHash(password, dbUser.Id.ToString());
                    }
                }
                await _db.SaveChangesAsync();
                return "Wijzigingen succesvol opgeslagen";
            } 
            catch (InvalidSessionException) {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Sessie bestaat niet of is verlopen.";
            }
        }

        /// <summary>
		/// Adds message to queue
		/// </summary>
		/// <param name="address">The mail address of the user</param>
		/// <param name="body">The mail message</param>
        private void SendEmail(string address, string body) => EmailService.QueueMessage(address, body);

        /// <summary>
		/// Given a password and salt, returns a salted SHA512 hash.
		/// </summary>
		/// <param name="password">The password</param>
		/// <param name="salt">The salt to use (username)</param>
		/// <returns>The new password hash</returns>
        private byte[] CreatePasswordHash(string password, string salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException($"'{nameof(password)}' cannot be null or empty");
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentException($"'{nameof(salt)}' cannot be null or empty");

            using var sha512Hash = SHA512.Create();
            return sha512Hash.ComputeHash(Encoding.UTF8.GetBytes($"{salt}#:#{password}"));
        }

        [HttpPost]
        public IActionResult PostReview(int revieweeId, int communication, int? quality)
        {
            if(communication <= 0 || communication > 5 || (quality is object && (quality <= 0 || quality > 5)))
            {
                SetTempDataMessage(false, "De ingevoerde waarden voldoen niet aan de eisen.");
                return View("ReviewAdvertisement");
            }
            byte communicationGrade = (byte)communication;
            byte? qualityGrade = (byte?)quality;

            int userId;
            try
            {
                userId = GetUserId();
            }catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            }
            if (revieweeId == userId)
            {
                SetTempDataMessage(false, "U kunt uzelf niet beoordelen.");
                return RedirectToAction("MyAccount");
            }
            _db.Ratings.Add(new Rating()
            {
                Communication = communicationGrade,
                Quality = qualityGrade,
                ReviewerId = userId,
                RevieweeId = revieweeId
            });
            _db.SaveChanges();
            SetTempDataMessage(true, "Uw beoordeling is opgeslagen.");
            return RedirectToAction("MyAccount");
        }

        public string GetModifyFormData(int listingId)
        {
            Listing listing = (from li in _db.Listings where li.Id == listingId select li)
                .Include("Pictures")
                .FirstOrDefault();
            var temp = listing.Pictures;
            listing.Filters = (from fl in _db.FilterListings where fl.ListingId == listingId select fl.Value).ToList();
            listing.Pictures = null;
            List<string> imgs = new List<string>();
            temp.ForEach(delegate (ImageData img)
            {
                imgs.Add(img.GetImage());
            });
            JObject jsonListing = JObject.Parse(JsonConvert.SerializeObject(listing));
            jsonListing.Add("images", JToken.FromObject(imgs));
            return JsonConvert.SerializeObject(jsonListing);
        }

        private void SetTempDataMessage(bool success, string message)
        {
            TempData.Remove(success ? "AccountControllerError" : "AccountControllerMsg");
            TempData[success ? "AccountControllerMsg" : "AccountControllerError"] = message;
        }
    }
}

