﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Stexchange.Controllers.Exceptions;
using Stexchange.Data;
using Stexchange.Data.Models;
using Stexchange.Models;

namespace Stexchange.Controllers
{
    public class ChatController : StexChangeController
    {
        private Database _db;

        public ChatController(Database db)
        {
            _db = db;
        }

        /// <summary>
        /// Route to retrieve the chat inbox of the user.
        /// User must be logged in.
        /// If the pre-condition is not met,
        /// the client will be redirected to the Login view.
        /// </summary>
        /// <returns>The Chat view for the user.</returns>
        /// 
        public IActionResult Chat()
        {

            int userId;
            try
            {
                userId = GetUserId();
            } catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            }
            List<Chat> chats = (from chat in _db.Chats
                                where (chat.ResponderId == userId ||
                                    (from listing in _db.Listings
                                     where listing.UserId == userId
                                     select listing.Id).Contains(chat.AdId))
                                select new EntityBuilder<Chat>(chat)
                                .SetProperty("Messages", (
                                    from message in _db.Messages
                                    where message.ChatId == chat.Id
                                    orderby message.Timestamp ascending
                                    select message
                                ).ToList())
                                .SetProperty("Poster", (
                                    from user in _db.Users
                                    join listing in _db.Listings on user.Id equals listing.UserId
                                    where listing.Id == chat.AdId
                                    select user).First())
                                .SetProperty("Responder", (
                                    from user in _db.Users
                                    where user.Id == chat.ResponderId
                                    select user).First())
                                .SetProperty("Listing", (
                                    from listing in _db.Listings
                                    where listing.Id == chat.AdId
                                    select listing).First())
                                .Complete()).ToList();
            List<int> blockedUsers = (from b in _db.Blocks
                                      where b.BlockerId == userId
                                      select b.BlockedId).ToList();
            List<int> blockerUsers = (from b in _db.Blocks
                                      where b.BlockedId == userId
                                      select b.BlockerId).ToList();
            List<int> blockedAds = (from a in _db.Listings
                                    where (blockedUsers.Contains(a.UserId))
                                    select a.Id).ToList();
            List<int> blockerAds = (from a in _db.Listings
                                    where (blockerUsers.Contains(a.UserId))
                                    select a.Id).ToList();

            chats = (from chat in chats
                     where chat.Messages.Any() && !(blockedAds.Contains(chat.AdId) || blockedUsers.Contains(chat.ResponderId))
                                               && !(blockerAds.Contains(chat.AdId) || blockerUsers.Contains(chat.ResponderId))
                     orderby chat.Messages[0].Timestamp descending 
                     select chat).ToList();
            
            
            try
            {
                int recentChat = chats[0].Id;
                var RecentTimestamp = chats[0].Messages[0].Timestamp;
                
                foreach (Chat ch in chats)
                {
                    if (ch.Messages.Last().Timestamp > RecentTimestamp)
                    {
                      RecentTimestamp = ch.Messages.Last().Timestamp;
                      recentChat = ch.Id;
                      TempData["Active"] = recentChat;
                    }
                    
                }
                return View(model: new ChatViewModel(chats, userId, recentChat));
            }
            catch(ArgumentOutOfRangeException)
            {
                return View(model: new ChatViewModel(chats, userId, -1));
            }      
        }

        /// <summary>
        /// Route for the client to post a message.
        /// User must be logged in.
        /// If the pre-condition is not met,
        /// the client will be redirected to the Login view.
        /// If a chat between the sender and receipient does not exist,
        /// it will be created.
        /// If sender or receipient blocked either,
        /// or if the message does not pass the explicit content filter,
        /// the message will not be send and the client
        /// will be notified to display an error message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PostMessage(string message, int activeId)
        {
            int userId;
            TempData["Active"] = activeId;
            TempData.Keep("Active");
            if (message is null || activeId == -1)
            {
                return RedirectToAction("Chat");
            }
            try
            {
                userId = GetUserId();



            }
            catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            }
            var messages = (from m in _db.Messages
                            where m.ChatId == activeId
                            orderby m.Timestamp descending
                            select m.SenderId).Take(10).ToArray();
            if (messages.Length < 10 || !Array.TrueForAll(messages, value => value == userId))
            {
                
                var newMessage = new Message
                {
                    ChatId = activeId,
                    Content = message,
                    SenderId = userId

                };
                _db.Messages.Add(newMessage);
                _db.SaveChanges();

            }
            else
            {
                TempData["Error"] = 1;
                TempData.Keep("Error");
            }
            return RedirectToAction("Chat");    


            //TODO: implement user blocking
            //TODO: implement chat content filter




        }
        [HttpPost]
        public IActionResult NewChat(int listId, string message)
        {
            int userId;
            try
            {
                userId = GetUserId();
            }
            catch (InvalidSessionException)
            {
                return RedirectToAction("Login", "Login");
            }
            var newChat = new Chat
            {
                ResponderId = userId,
                AdId = listId
                
            };
            try
            {
                _db.Chats.Add(newChat);
                _db.SaveChanges();
                PostMessage(message, newChat.Id);

            }
            catch (Exception e) when (e is DbUpdateException || e is MySqlException) 
            {
                _db.Remove(newChat);
                int ChatId = (from c in _db.Chats
                                where (c.AdId == listId && c.ResponderId == userId)
                                select c.Id).FirstOrDefault();
                 PostMessage(message, ChatId ); ;
            }
            return RedirectToAction("Chat");
        }

    }
}
