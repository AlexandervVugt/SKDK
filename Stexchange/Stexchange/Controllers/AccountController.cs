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
using Stexchange.Data.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc.Routing;

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
        public string PostReview(int ratingRequestId, int communication, int? quality)
        {
            RatingRequest ratingRequest = (from rr in _db.RatingRequests
                                           where rr.Id == ratingRequestId
                                           select rr).FirstOrDefault();
            if(ratingRequest is null)
            {
                Response.StatusCode = 500;
                return "The server encountered an invalid data state while processing your request.";
            }
            if(communication <= 0 || communication > 5 || (!ratingRequest.RequestQuality && quality is object)
                || (ratingRequest.RequestQuality && (quality is null || quality <= 0 || quality > 5)))
            {
                Response.StatusCode = 400;
                return "De ingevoerde waarden voldoen niet aan de eisen.";
            }
            byte communicationGrade = (byte)communication;
            byte? qualityGrade = (byte?)quality;

            int userId;
            try
            {
                userId = GetUserId();
            }catch (InvalidSessionException)
            {
                Response.StatusCode = 302;
                UrlHelper redirect = new UrlHelper(ControllerContext);
                Response.Headers.Add("RedirectURL", redirect.Action("Login", "Login"));
                return "Not allowed: you are not logged in";
            }
            if (ratingRequest.RevieweeId == userId)
            {
                Response.StatusCode = 400;
                return "U kunt uzelf niet beoordelen.";
            }
            if(ratingRequest.ReviewerId != userId)
            {
                Response.StatusCode = 401;
                return "U bent niet gemachtigd voor deze beoordeling.";
            }
            _db.Ratings.Add(new Rating()
            {
                Communication = communicationGrade,
                Quality = qualityGrade,
                ReviewerId = userId,
                RevieweeId = ratingRequest.RevieweeId
            });
            _db.Remove(ratingRequest);
            _db.SaveChanges();
            return "Uw beoordeling is opgeslagen.";
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

        public async Task<object> ModifyAdvertisementAsync(int listingId, List<IFormFile> files, string title, string description,
            string name_nl, int quantity, string plant_type, string plant_order, string give_away, string with_pot, string light, string water, string name_lt = "",
            string ph = "", string indigenous = "", string nutrients = "")
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dbUser = (from user in _db.Users
                                  where user.Id == GetUserId()
                                  select user).First();

                    // Retrieves correct listing from database Listings
                    var listing = (from li in _db.Listings
                                   where li.Id == listingId
                                   select li).First();

                    // Retrieves correct filters from database Filters
                    var filters = (from filter in _db.FilterListings
                                   where filter.ListingId == listingId
                                   select filter).ToList();

                    List<string> errormessages = new List<string>();

                    // Validators for required fields
                    ListingValidator listingVal = new ListingValidator();
                    WithPotFilterValidator potVal = new WithPotFilterValidator();
                    GiveAwayFilterValidator giveVal = new GiveAwayFilterValidator();
                    PlantTypeFilterValidator typeVal = new PlantTypeFilterValidator();
                    OrderFilterValidator orderVal = new OrderFilterValidator();
                    WaterFilterValidator waterVal = new WaterFilterValidator();
                    LightFilterValidator lightVal = new LightFilterValidator();

                    // Validators for optional fields
                    PhFilterValidator phVal = new PhFilterValidator();
                    IndigenousFilterValidator indiVal = new IndigenousFilterValidator();
                    NutrientsFilterValidator nutrientsVal = new NutrientsFilterValidator();

                    // Validationresults of required fields
                    ValidationResult hasTypeFilter = typeVal.Validate(new Filter { Value = plant_type });
                    ValidationResult hasPotFilter = potVal.Validate(new Filter { Value = with_pot });
                    ValidationResult hasGiveFilter = giveVal.Validate(new Filter { Value = give_away });
                    ValidationResult waterresult = waterVal.Validate(new Filter { Value = water });
                    ValidationResult lightresult = lightVal.Validate(new Filter { Value = light });
                    ValidationResult orderFilter = orderVal.Validate(new Filter { Value = plant_order });
                    ValidationResult hasValProps = listingVal.Validate(new Listing
                    {
                        // If value will be empty string if it's null
                        Description = description ?? "",
                        Title = title ?? "",
                        NameNl = name_nl ?? "",
                        Quantity = quantity > 0 ? (uint)quantity : 0
                    });

                    // Validationresults of optional fields
                    ValidationResult phresult = phVal.Validate(new Filter { Value = ph });
                    ValidationResult indigenousresult = indiVal.Validate(new Filter { Value = indigenous });
                    ValidationResult nutrientsresult = nutrientsVal.Validate(new Filter { Value = nutrients });

                    if (hasValProps.IsValid && hasPotFilter.IsValid && hasGiveFilter.IsValid && hasTypeFilter.IsValid && waterresult.IsValid && lightresult.IsValid && orderFilter.IsValid)
                    {
                        if (!(string.IsNullOrWhiteSpace(title)) && listing.Title != title) listing.Title = StandardMessages.CapitalizeFirst(title).Trim();
                        if (!(string.IsNullOrWhiteSpace(description)) && listing.Description != description) listing.Description = StandardMessages.CapitalizeFirst(description).Trim();
                        if (!(string.IsNullOrWhiteSpace(name_nl)) && listing.NameNl != name_nl) listing.NameNl = StandardMessages.CapitalizeFirst(name_nl).Trim();
                        if (!(string.IsNullOrWhiteSpace(name_lt)) && listing.NameLatin != name_lt && name_lt.Length <= 50) listing.NameLatin = StandardMessages.CapitalizeFirst(name_lt).Trim();
                        if (quantity > 0 && listing.Quantity != quantity) listing.Quantity = (uint)quantity;

                        if (!phresult.IsValid) { phresult.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); };

                        if (!indigenousresult.IsValid) { indigenousresult.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); };

                        if (!nutrientsresult.IsValid) { nutrientsresult.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); };

                        // Creates new filterlisting if filter doesn't exist in 
                        List<FilterListing> existingFilters = (from filterListing in _db.FilterListings
                                                               where filterListing.ListingId == listingId
                                                               select filterListing).ToList();
                        List<string> existingFilterValues = (from entry in existingFilters select entry.Value).ToList();
                        List<string> selectedFilters = new List<string> { light, water, plant_type, nutrients, ph, indigenous, with_pot, give_away, plant_order };
                        List<FilterListing> remove = (from filterListing in existingFilters
                                                      where existingFilterValues.Except(selectedFilters).Contains(filterListing.Value)
                                                      select filterListing)
                                                      .ToList();
                        List<FilterListing> add = (from filter in selectedFilters
                                                   where selectedFilters.Except(existingFilterValues).Contains(filter)
                                                   select new FilterListing()
                                                   {
                                                       ListingId = listingId,
                                                       Value = filter
                                                   }).ToList();


                        await OnPostUploadAsync(files, listing, errormessages);

                        if (errormessages.Count > 0)
                        {
                            return errormessages;
                        }

                        remove.ForEach(entry => _db.Remove(entry));
                        add.ForEach(entry => _db.Add(entry));

                        await _db.SaveChangesAsync();
                    }
                    if (!hasPotFilter.IsValid) { hasPotFilter.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); }

                    if (!hasGiveFilter.IsValid) { hasGiveFilter.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); }

                    if (!hasValProps.IsValid) { hasValProps.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); }

                    if (!hasTypeFilter.IsValid) { hasTypeFilter.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); }

                    if (!waterresult.IsValid) { waterresult.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); }

                    if (!lightresult.IsValid) { lightresult.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); }

                    if (!orderFilter.IsValid) { orderFilter.Errors.ToList().ForEach(x => errormessages.Add(x.ErrorMessage)); }

                    if (errormessages.Count > 0)
                    {
                        return errormessages;
                    }
                    
                    return "Wijzigingen succesvol opgeslagen. Het kan enkele minuten duren voordat je de nieuwe wijzigingen kunt bekijken";
                }
                else
                {
                    return "Zorg ervoor dat alle verplichte velden correct zijn ingevuld";
                }
            }
            catch (InvalidSessionException)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Sessie bestaat niet of is verlopen.";
            }
        }

        /// <summary>
        /// Removes images from database
        /// </summary>
        /// <param name="listingId"></param>
        /// <returns></returns>
        public async Task<string> DeleteListingImages(int listingId)
        {
            try
            {
                var images = (from image in _db.Images
                            where image.ListingId == listingId
                            select image).ToList();
                images.ForEach(x => _db.Remove(x));
                await _db.SaveChangesAsync();
                return "Afbeeldingen succesvol verwijderd";
            }
            catch (InvalidSessionException)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Sessie bestaat niet of is verlopen.";
            }
        }

        public async Task OnPostUploadAsync(List<IFormFile> files, Listing listing, List<string> errormessages)
        {
            var images = (from image in _db.Images
                          where image.ListingId == listing.Id
                          select image).FirstOrDefault();
            if(images is null && files.Count == 0)
            {
                errormessages.Add("Het is verplicht om een foto te uploaden");
            }
            if (files.Count > 6) { 
                errormessages.Add("Het maximale aantal foto's dat geüpload mag worden is 6"); 
            } else
            {
                foreach (IFormFile file in files)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);

                        // Upload the file if less than 5 MB
                        if (memoryStream.Length < 5000000)
                        {
                            var imagefile = new ImageData()
                            {
                                Image = memoryStream.ToArray(),
                                Listing = listing,
                            };

                            _db.Add(imagefile);
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            errormessages.Add("De maximale bestandsgrootte van een foto is 5MB");
                        }
                    }
                }
            }
        }
    }
}

