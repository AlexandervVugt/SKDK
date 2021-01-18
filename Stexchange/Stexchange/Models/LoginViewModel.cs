using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Models
{
    public class LoginViewModel
    {
        public string UserNameEmail { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Email { get; set; }
        public string VEmail { get; set; }
        public string Password { get; set; }
        public string Confirm_password { get; set; }
        public string Username { get; set; }
        public string Postalcode { get; set; }
    }
}
