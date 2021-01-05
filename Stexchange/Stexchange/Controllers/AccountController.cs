using Microsoft.AspNetCore.Mvc;
using Stexchange.Controllers.Exceptions;
using Stexchange.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Controllers
{
    public class AccountController : StexChangeController
    {
        private readonly Database _db;

        public AccountController(Database db)
        {
            _db = db;
        }

        public IActionResult MyAccount()
        {
            try
            {
                GetUserId();
            } catch (InvalidSessionException)
            {
                RedirectToAction("Login", "Login");
            }
            //TODO: load users data
            //TODO: load users listings
            //TODO: load users rating requests
            return View();
        }
    }
}
