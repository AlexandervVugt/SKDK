using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Stexchange.Data;
using Stexchange.Data.Models;
using Stexchange.Data.Helpers;
using Stexchange.Data.Validation;
using static System.String;
using Microsoft.AspNetCore.Http;
using System.IO;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Stexchange.Controllers
{
    public class AdvertisementController : StexChangeController
    {
        public AdvertisementController(Database db, IConfiguration config, ILogger<AdvertisementController> logger)
        {
            _database = db;
            _config = config;
            Log = logger;
        }

        private Database _database { get; }
        private IConfiguration _config { get; }
        private ILogger Log { get; }

        public IActionResult PostAdvertisement()
        {
            return View();
        }

        public IActionResult Posted()
        {
            return View("Posted");
        }

        

        [HttpPost]
        public async Task<IActionResult> PostAdvertisement(List<IFormFile> files, string title, string description, 
            string name_nl, int quantity, string plant_type, string give_away, string with_pot, string name_lt="",
            string light = "", string water = "", string ph = "", string indigenous = "", string nutrients="")
        {
            try
            {
                List<string> errormessages = new List<string>();
                if (ModelState.IsValid)
                {
                    EntityBuilder<Listing> listingBuilder = new EntityBuilder<Listing>();
                    Listing finishedListing;

                    ListingValidator listingVal = new ListingValidator();
                    WithPotFilterValidator potVal = new WithPotFilterValidator();
                    GiveAwayFilterValidator giveVal = new GiveAwayFilterValidator();
                    PlantTypeFilterValidator typeVal = new PlantTypeFilterValidator();
                    List<Filter> mandatoryFilters = new List<Filter>();

                    ValidationResult hasTypeFilter = typeVal.Validate(new Filter { Value = plant_type });
                    ValidationResult hasPotFilter = potVal.Validate(new Filter{ Value = with_pot });
                    ValidationResult hasGiveFilter = giveVal.Validate(new Filter{ Value = give_away });
                    ValidationResult hasValProps = listingVal.Validate(new Listing
                                                                            {
                                                                                // If value will be empty string if it's null
                                                                                Description = description ?? "",
                                                                                Title = title ?? "",
                                                                                NameNl = name_nl ?? "",
                                                                                Quantity = quantity > 0 ? (uint)quantity : 0
                                                                            });

                    if (hasValProps.IsValid && hasPotFilter.IsValid && hasGiveFilter.IsValid && hasTypeFilter.IsValid)
                    {
                        listingBuilder.SetProperty("Title", StandardMessages.CapitalizeFirst(title).Trim())
                                      .SetProperty("Description", StandardMessages.CapitalizeFirst(description).Trim())
                                      .SetProperty("NameNl", StandardMessages.CapitalizeFirst(name_nl).Trim())
                                      .SetProperty("Quantity", quantity)
                                      .SetProperty("Visible", true)
                                      .SetProperty("Renewed", false)
                                      .SetProperty("UserId", GetUserId()); 

                        // adding validated required filters to list
                        mandatoryFilters.Add(new Filter{ Value = with_pot });
                        mandatoryFilters.Add(new Filter{ Value = give_away });
                        mandatoryFilters.Add(new Filter{ Value = plant_type });

                        // non-required properties
                        Tuple<bool, List<Filter>, List<string>> filtersWithFlag = FilterListValidator(errormessages, mandatoryFilters, ph, water, indigenous, light, nutrients);

                        if(filtersWithFlag.Item1 == false)
                        {
                            ViewBag.Messages = filtersWithFlag.Item3;
                            return RedirectToAction("PostAdvertisement");
                        }

                        List<Filter> validatedFilters = filtersWithFlag.Item2;

                        if (!IsNullOrEmpty(name_lt)) listingBuilder.SetProperty("NameLatin", StandardMessages.CapitalizeFirst(name_lt).Trim());

                        finishedListing = listingBuilder.Complete();

                        List<FilterListing> filterListings = MakeFilterListing(validatedFilters, finishedListing);

                        // ensures that the listing is inserted before the tables who need this FK
                        await _database.AddAsync(finishedListing);

                        List<Task> tasks = new List<Task>();
                        // Insert byte[] image into database
                        await OnPostUploadAsync(files, finishedListing, errormessages);
                        if(errormessages.Count > 0)
                        {
                            ViewBag.Messages = errormessages;
                            return View();
                        }
                        // loops through filterlist to add each advertisementfilter
                        await _database.AddRangeAsync(filterListings);

                        //passing data to the view
                        TempData["Title"] = finishedListing.Title;
                        TempData.Keep("Title");

                        await _database.SaveChangesAsync();

                        return RedirectToAction("Posted");
                    }
                    
                    if (!hasPotFilter.IsValid)
                    { 
                        foreach (ValidationFailure error in hasPotFilter.Errors)
                        {
                            errormessages.Add(error.ErrorMessage);
                        }
                    }

                    if (!hasGiveFilter.IsValid)
                    {
                        foreach (ValidationFailure error in hasGiveFilter.Errors)
                        {
                            errormessages.Add(error.ErrorMessage);
                        }
                    }

                    if (!hasValProps.IsValid)
                    {
                        foreach (ValidationFailure error in hasValProps.Errors)
                        {
                            errormessages.Add(error.ErrorMessage);
                        }
                    }

                    if (!hasTypeFilter.IsValid)
                    {
                        foreach (ValidationFailure error in hasTypeFilter.Errors)
                        {
                            errormessages.Add(error.ErrorMessage);
                        }
                    }

                    ViewBag.Messages = errormessages;
                }
                else
                {
                    errormessages.Add("Zorg ervoor dat alle verplichte velden correct zijn ingevuld");
                    ViewBag.Messages = errormessages;
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex.ToString());
            }
            return View();
        }



        /// <summary>
        /// Uploads the image data linked to the listing to db
        /// </summary>
        /// <param name="files"></param>
        /// <param name="finishedListing"></param>
        /// <returns></returns>
        public async Task OnPostUploadAsync(List<IFormFile> files, Listing finishedListing, List<string> errormessages)
        {
            //creates memorystream for each image
            foreach (IFormFile file in files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);

                    // Upload the file if less than 5 MB
                    Console.WriteLine(memoryStream.Length);
                    if (memoryStream.Length < 5000000)
                    {
                        var imagefile = new ImageData()
                        {
                            Image = memoryStream.ToArray(),
                            Listing = finishedListing,
                        };

                        _database.Add(imagefile);
                        await _database.SaveChangesAsync();
                    }
                    else
                    {
                        errormessages.Add("De maximale bestandsgrootte van een foto is 5MB");
                    }
                }
            }
        }

        /// <summary>
        /// adds each filter in filterlist and the advertisement listing to list of filterlistings.
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="finishedListing"></param>
        /// <returns></returns>
        private List<FilterListing> MakeFilterListing(List<Filter> filters, Listing finishedListing)
        {
            List<FilterListing> filterListings = new List<FilterListing>();
            foreach (Filter fil in filters)
            {
                // adding new filterlisting instance to filterListings
                filterListings.Add(new FilterListing { Listing = finishedListing, Value = fil.Value });
            }
            return filterListings;
        }

        
        /// <summary>
        /// returns the userId or -1
        /// -1 is filtered away by the constraint in the ListingValidator
        /// </summary>
        /// <returns></returns>
        private int GetUserId()
        {
            long token = Convert.ToInt64(Request.Cookies["SessionToken"]);

            if (GetSessionData((long)token, out Tuple<int, string> data)) 
            {
                var user_id = data.Item1; 
                return user_id;
            }

            return -1;
        }

        /// <summary>
        /// validates filter values and adds these to the filterlist
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="ph"></param>
        /// <param name="water"></param>
        /// <param name="indigenous"></param>
        /// <param name="light"></param>
        /// <param name="nutrients"></param>
        /// <returns></returns>
        private Tuple<bool, List<Filter>, List<string>> FilterListValidator(List<string> errormessages, List<Filter> filters, string ph, string water, string indigenous, string light, string nutrients)
        {
            PhFilterValidator phVal = new PhFilterValidator();
            WaterFilterValidator waterVal = new WaterFilterValidator();
            LightFilterValidator lightVal = new LightFilterValidator();
            IndigenousFilterValidator indiVal = new IndigenousFilterValidator();
            NutrientsFilterValidator nutrientsVal = new NutrientsFilterValidator();

            Filter phFilter = new Filter { Value = ph };
            Filter waterFilter = new Filter { Value = water };
            Filter lightFilter = new Filter { Value = light };
            Filter indigenousFilter = new Filter { Value = indigenous };
            Filter nutrientsFilter = new Filter { Value = nutrients };

            ValidationResult phresult = phVal.Validate(phFilter);
            ValidationResult waterresult = waterVal.Validate(waterFilter);
            ValidationResult lightresult = lightVal.Validate(lightFilter);
            ValidationResult indigenousresult = indiVal.Validate(indigenousFilter);
            ValidationResult nutrientsresult = nutrientsVal.Validate(nutrientsFilter);

            if (phresult.IsValid) { filters.Add(phFilter); } else { foreach (ValidationFailure error in phresult.Errors) { errormessages.Add(error.ErrorMessage); } };
            if (waterresult.IsValid) { filters.Add(waterFilter); } else { foreach (ValidationFailure error in waterresult.Errors) { errormessages.Add(error.ErrorMessage); } };
            if (lightresult.IsValid) { filters.Add(lightFilter); } else { foreach (ValidationFailure error in lightresult.Errors) { errormessages.Add(error.ErrorMessage); } };
            if (indigenousresult.IsValid) { filters.Add(indigenousFilter); } else { foreach (ValidationFailure error in indigenousresult.Errors) { errormessages.Add(error.ErrorMessage); } };
            if (nutrientsresult.IsValid) { filters.Add(nutrientsFilter); } else { foreach (ValidationFailure error in nutrientsresult.Errors) { errormessages.Add(error.ErrorMessage); } };
            bool check = phresult.IsValid && waterresult.IsValid && lightresult.IsValid && indigenousresult.IsValid && nutrientsresult.IsValid;
            
            return Tuple.Create(check, filters, errormessages);
        }
    }
}
