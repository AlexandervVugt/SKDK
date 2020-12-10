using Stexchange.Data.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Models
{
    public class DetailAdvertisementModel
    {
        public Listing Listing { get; }
        public Dictionary<string, string> Filterlist { get; }

        public DetailAdvertisementModel(Listing listing, Dictionary<string, string> filterlist)
        {
            Listing = listing;
            Filterlist = filterlist;
        }

        public List<string> GetImages()
        {
            List<string> res = new List<string>();
            foreach (ImageData img in Listing.Pictures) {
                res.Add(img.GetImage());
            }
            return res;
        }
    }
}
