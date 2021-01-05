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
}
