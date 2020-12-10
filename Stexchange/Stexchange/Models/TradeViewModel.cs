using Microsoft.Extensions.Logging;
using Stexchange.Controllers;
using Stexchange.Data;
using Stexchange.Data.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stexchange.Models
{
    public sealed class TradeViewModel
    {
        public List<Listing> Listings { get; }
        public TradeViewModel(List<Listing> listings)
        {
            Listings = listings;
        }
    }
}
