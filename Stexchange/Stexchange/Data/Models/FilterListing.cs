﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Models
{
    public class FilterListing
    {
        public FilterListing(Listing list, Filter filt)
        {
            Listing = list;
            Filter = filt;
        }
        [Column("listing_id", TypeName = "bigint(20) unsigned")]
        public int ListingId { get; set; }

        [Column("filter_value", TypeName = "varchar(20)")]
        public string Value { get; set; }

        public Listing Listing { get; set; }
        public Filter Filter { get; set; }
    }
}
