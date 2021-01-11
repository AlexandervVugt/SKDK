using Stexchange.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Models
{
    public class AccountViewModel
    {
        public User User { get; set; }
        public List<Listing> Listings { get; set; }
        public List<RatingRequest> RatingRequests { get; set; }
    }

    public class AccountSettingsModel
    {
        public string Username { get; set; }
        public string Postalcode { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Confirm_Password { get; set; }
    }

}
