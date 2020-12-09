﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
                                    orderby message.Timestamp descending
                                    select message
                                ).ToList())
                                .Complete()).ToList();
            chats = (from chat in chats
                     orderby chat.Messages[0].Timestamp descending
                     select chat).ToList();
            return View(model: new ChatViewModel(chats));
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
        public IActionResult PostMessage(Message message)
        {
            int userId;
            try
            {
                userId = GetUserId();
            } catch (InvalidSessionException) {
                return RedirectToAction("Login", "Login");
            }
            if(message.ChatId == -1)
            {
                //TODO: create a new chat
            }
            //TODO: implement user blocking
            //TODO: implement chat content filter
            _db.Messages.Add(message);
            return RedirectToAction("Chat");
        }
    }
}
