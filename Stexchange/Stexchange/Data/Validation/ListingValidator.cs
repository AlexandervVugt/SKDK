using FluentValidation;
using Stexchange.Data.Helpers;
using Stexchange.Data.Models;

namespace Stexchange.Data.Validation
{
    public class ListingValidator : AbstractValidator<Listing>
    {
        private int MaxTitleSize = 50;
        private int MaxDescriptionSize = 500;
        private int MinDescriptionSize = 10;
        private int MaxNlNameLength = 50;

        public ListingValidator()
        {
            //todo make constants for static fields listing
            


            RuleFor(x => x.Title.Trim()).NotEmpty().WithErrorCode(StandardMessages.RequiredField("title"));
            RuleFor(x => x.Title.Trim().Length).LessThanOrEqualTo(MaxTitleSize).WithErrorCode(StandardMessages.AmountOfCharacters());
            RuleFor(x => x.Title.Trim()).Matches(@"[\w]+");
            RuleFor(x => x).Must(x => StandardMessages.ContainsProfanity(x.Title)).WithErrorCode(StandardMessages.RewriteTextPlease());

            RuleFor(x => x.Quantity).NotNull().WithErrorCode(StandardMessages.RequiredField("hoeveelheid"));
            //RuleFor(x => x.Quantity).GreaterThan(0);

            RuleFor(x => x.UserId).NotNull();
            RuleFor(x => x.UserId).NotEqual(-1).WithErrorCode(StandardMessages.SomethingWW("user id"));

            RuleFor(x => x.Description.Trim()).NotEmpty().WithErrorCode(StandardMessages.RequiredField("beschrijving")); 
            RuleFor(x => x.Description.Trim().Length).LessThanOrEqualTo(MaxDescriptionSize).WithErrorCode(StandardMessages.AmountOfCharacters());
            RuleFor(x => x.Description.Trim().Length).GreaterThanOrEqualTo(MinDescriptionSize).WithErrorCode(StandardMessages.AmountOfCharacters());
            RuleFor(x => x.Description.Trim()).Matches(@"[\w\s]+");
            RuleFor(x => x).Must(x => StandardMessages.ContainsProfanity(x.Description)).WithErrorCode(StandardMessages.RewriteTextPlease());

            RuleFor(x => x.NameNl.Trim()).NotEmpty().WithErrorCode(StandardMessages.RequiredField("de Nederlandse naam")); 
            RuleFor(x => x.NameNl.Trim().Length).LessThanOrEqualTo(MaxNlNameLength).WithErrorCode(StandardMessages.AmountOfCharacters());
            RuleFor(x => x.NameNl.Trim()).Matches(@"[\w]+");
            RuleFor(x => x).Must(x => StandardMessages.ContainsProfanity(x.NameNl)).WithErrorCode(StandardMessages.RewriteTextPlease());


        }
        
    }
}
