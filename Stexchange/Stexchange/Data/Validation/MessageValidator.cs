using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Stexchange.Data.Models;
using Stexchange.Data.Helpers;

namespace Stexchange.Data.Validation
{
    public class MessageValidator : AbstractValidator<Message>
    {

        public MessageValidator()
        {
            RuleFor(x => x.Content.Trim()).NotEmpty();
            RuleFor(x => x).Must(x => !StandardMessages.ContainsProfanity(x.Content)).WithErrorCode(StandardMessages.RewriteTextPlease());
            
            //todo: handle listingowner.id==responder.id
            // i need the listing
            //RuleFor(x => from  in Database.)

            //var verification = (from code in Database.UserVerifications
            //    where code.Guid == guid
            //    select code).FirstOrDefault();

        }

    }
}
