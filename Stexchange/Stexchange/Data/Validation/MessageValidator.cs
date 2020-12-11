using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Stexchange.Data.Models;

namespace Stexchange.Data.Validation
{
    public class MessageValidator : AbstractValidator<Message>
    {

        public MessageValidator()
        {
            RuleFor(x => x.Content.Trim()).NotEmpty();
            
            //todo: handle listingowner.id==responder.id
            //RuleFor(x => from  in Database.)

            //var verification = (from code in Database.UserVerifications
            //    where code.Guid == guid
            //    select code).FirstOrDefault();

        }

    }
}
