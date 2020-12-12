using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Stexchange.Data.Helpers;
using Stexchange.Data.Models;

namespace Stexchange.Data.Validation
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.Password.Length).GreaterThanOrEqualTo(8);

        }
    }
}
