using FluentValidation;
using Stexchange.Data.Helpers;
using Stexchange.Data.Models;

namespace Stexchange.Data.Validation
{
    public class ListingValidator : AbstractValidator<Listing>
    {
        private readonly int MaxTitleSize = 50;
        private readonly int MaxDescriptionSize = 500;
        private readonly int MinDescriptionSize = 10;
        private readonly int MaxNlNameLength = 50;
        private readonly uint MinQuantitySize = 1;

        public ListingValidator()
        {
            //todo make constants for static fields listing

            RuleFor(x => x.Title.Trim()).NotEmpty().WithMessage(StandardMessages.RequiredField("Titel"));
            RuleFor(x => x.Title.Trim().Length).LessThanOrEqualTo(MaxTitleSize).WithMessage(StandardMessages.AmountOfCharacters("titel"));
            RuleFor(x => x.Title.Trim()).Matches(@"[\w]+").WithMessage(StandardMessages.NoMatch("titel"));

            RuleFor(x => x.Quantity).NotEmpty().WithMessage(StandardMessages.RequiredField("Hoeveelheid"));
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(MinQuantitySize).WithMessage(StandardMessages.AmountOfCharacters("hoeveelheid"));

            RuleFor(x => x.UserId).NotNull();
            RuleFor(x => x.UserId).NotEqual(-1).WithMessage(StandardMessages.SomethingWW("gebruikersID"));

            RuleFor(x => x.Description.Trim()).NotEmpty().WithMessage(StandardMessages.RequiredField("Beschrijving")); 
            RuleFor(x => x.Description.Trim().Length).LessThanOrEqualTo(MaxDescriptionSize).WithMessage(StandardMessages.AmountOfCharacters("beschrijving"));
            RuleFor(x => x.Description.Trim().Length).GreaterThanOrEqualTo(MinDescriptionSize).WithMessage(StandardMessages.AmountOfCharacters("beschrijving"));
            RuleFor(x => x.Description.Trim()).Matches(@"[\w\s]+").WithMessage(StandardMessages.NoMatch("beschrijving"));

            RuleFor(x => x.NameNl.Trim()).NotEmpty().WithMessage(StandardMessages.RequiredField("Nederlandse naam")); 
            RuleFor(x => x.NameNl.Trim().Length).LessThanOrEqualTo(MaxNlNameLength).WithMessage(StandardMessages.AmountOfCharacters("nederlandse naam"));
            RuleFor(x => x.NameNl.Trim()).Matches(@"[\w]+").WithMessage(StandardMessages.NoMatch("nederlandse naam"));
        }
        
    }
}
