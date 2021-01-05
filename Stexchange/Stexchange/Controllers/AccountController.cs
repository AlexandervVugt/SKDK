using Microsoft.AspNetCore.Mvc;
using Stexchange.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Controllers
{
    public class AccountController : Controller
    {
        private readonly Database _db;
        public AccountController(Database db)
        {
            _db = db;
        }
        public IActionResult MyAccount()
        {
            return View();
        }
    }
}
