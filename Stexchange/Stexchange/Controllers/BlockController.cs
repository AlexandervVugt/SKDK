using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Stexchange.Controllers.Exceptions;
using Stexchange.Data;
using Stexchange.Data.Models;

namespace Stexchange.Controllers
{
    public class BlockController : StexChangeController
    {
        private Database _db;

        public BlockController(Database db)
        {
            _db = db;
        }
        [HttpPost]
        public IActionResult Block(int listid)
        {
            int userId;
            try
            {
                userId = GetUserId();
                int blockedUserId = (from l in _db.Listings
                                     where (l.Id == listid)
                                     select l.UserId).FirstOrDefault();
                var newBlock = new Block
                {
                    BlockerId = userId,
                    BlockedId = blockedUserId
                };
                _db.Blocks.Add(newBlock);
                _db.SaveChanges();
                return RedirectToAction("Trade", "Trade");
            }
            catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            }

        }
    }
}
