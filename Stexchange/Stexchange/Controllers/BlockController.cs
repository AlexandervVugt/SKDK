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
        public IActionResult Block(int id , string block)
        {
            int userId;
            try
            {
                userId = GetUserId();
                int blockedUserId;
                if (block == "list")
                {
                     blockedUserId = (from l in _db.Listings
                                         where (l.Id == id)
                                         select l.UserId).FirstOrDefault();
                    UpdateDB(userId, blockedUserId);
                    return RedirectToAction("Trade", "Trade");
                }
                else if (block == "chat")
                {
                    int AdId = (from c in _db.Chats
                                     where (c.Id == id)
                                     select c.AdId).FirstOrDefault();
                    blockedUserId = (from l in _db.Listings
                                     where (l.Id == AdId)
                                     select l.UserId).FirstOrDefault();
                    UpdateDB(userId, blockedUserId);
                    return RedirectToAction("Chat", "Chat");
                }
                else
                {
                    return View();
                }

            }
            catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            }

        }
        [HttpPost]
        public IActionResult Unblock(int blockedId)
        {
            var block = (from b in _db.Blocks
                             where (b.BlockedId == blockedId && b.BlockerId == GetUserId())
                             select b);
            if (block != null)
            {
                _db.Remove(block);
                _db.SaveChanges();
            }
            return View();

        }
        public async void UpdateDB (int userId, int blockedUserId)
        {
            var newBlock = new Block
            {
                BlockerId = userId,
                BlockedId = blockedUserId
            };
            await _db.Blocks.AddAsync(newBlock);
            await _db.SaveChangesAsync();
            
        }
    }
}
