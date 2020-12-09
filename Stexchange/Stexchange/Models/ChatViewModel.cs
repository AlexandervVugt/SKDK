﻿using Stexchange.Controllers;
using Stexchange.Data;
using Stexchange.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Models
{
    public class ChatViewModel
    {
        public List<Chat> ChatInbox { get; set; }

        public ChatViewModel(List<Chat> chatInbox)
        {
            ChatInbox = chatInbox;
        }
    }
}
