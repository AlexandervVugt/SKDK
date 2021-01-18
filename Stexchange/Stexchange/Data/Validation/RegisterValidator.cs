using FluentValidation;
using Stexchange.Data.Helpers;
using Stexchange.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Stexchange.Data.Validation
{
    public class RegisterValidator : AbstractValidator<RegisterModel>
    {
        public RegisterValidator()
        {
            // Validate email
            RuleFor(x => x.Email).NotEmpty().WithMessage("E-mail is een verplicht veld");
            RuleFor(x => new EmailAddressAttribute().IsValid(x.Email)).Must(x => x == true).WithMessage("Ongeldig e-mailadres in e-mail veld").When(x => x.Email.Length > 0);
            RuleFor(x => x.Email).Must(x => x.Length <= 254).WithMessage("E-mailadres mag maximaal uit 15 karakters bestaan").When(x => x.Email.Length > 0);

            // Validate verificationEmail
            RuleFor(x => x.VEmail).NotEmpty().WithMessage("Bevestig e-mail is een verplicht veld");
            RuleFor(x => new EmailAddressAttribute().IsValid(x.VEmail)).Must(x => x == true).WithMessage("Ongeldig e-mailadres in bevestig e-mail veld").When(x => x.VEmail.Length > 0);
            RuleFor(x => x.VEmail).Must((x, vEmail) => x.Email.Equals(vEmail, StringComparison.OrdinalIgnoreCase)).WithMessage("E-mailadressen komen niet met elkaar overeen").When(x => x.VEmail.Length > 0);

            // Validate password
            RuleFor(x => x.Password).NotEmpty().WithMessage("Wachtwoord is een verplicht veld");
            RuleFor(x => x.Password).Must(x => x.Length >= 8).WithMessage("Ongeldig wachtwoord. Wachtwoord moet uit minimaal 8 karakters bestaan").When(x => x.Password.Length > 0);

            // Validate confirm password
            RuleFor(x => x.Confirm_password).NotEmpty().WithMessage("Bevestig wachtwoord is een verplicht veld");
            RuleFor(x => x.Confirm_password).Must(x => x.Length >= 8).WithMessage("Ongeldig wachtwoord. Wachtwoord moet uit minimaal 8 karakters bestaan").When(x => x.Confirm_password.Length > 0);
            RuleFor(x => x.Confirm_password).Must((x, password) => x.Password.Equals(password)).WithMessage("Wachtwoorden komen niet met elkaar overeen").When(x => x.Confirm_password.Length > 0);

            // Validate username
            RuleFor(x => x.Username).NotEmpty().WithMessage("Gebruikersnaam is een verplicht veld");
            RuleFor(x => x.Username).Must(x => x.Length <= 15).WithMessage("Ongeldige gebruikersnaam. Gebruikersnaam mag maximaal uit 15 karakters bestaan").When(x => x.Username.Length > 0);

            // Validate postal code
            RuleFor(x => x.Postalcode).NotEmpty().WithMessage("Postcode is een verplicht veld");
            RuleFor(x => x.Postalcode).Must(x => x.Length == 6).WithMessage("Ongeldige postcode. Een postcode bestaat uit 4 cijfers en 2 letters").When(x => x.Postalcode.Length > 0); 
            RuleFor(x => x.Postalcode).Must(x => new Regex(@"\d{4}[A-Z]{2}", RegexOptions.IgnoreCase).IsMatch(x)).WithMessage("Ongeldige postcode").When(x => x.Postalcode.Length > 0);
        }
    }

    public class LoginValidator : AbstractValidator<LoginViewModel>
    {
        public LoginValidator()
        {
            RuleFor(x => x.UserNameEmail).NotEmpty().WithMessage("E-mail of gebruikersnaam is een verplicht veld");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Wachtwoord is een verplicht veld");
        }
    }

    public class ChangeAccountSettingsValidator : AbstractValidator<AccountSettingsModel>
    {
        public ChangeAccountSettingsValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Gebruikersnaam is een verplicht veld");
            RuleFor(x => x.Username).Must(x => x.Length <= 15).WithMessage("Ongeldige gebruikersnaam. Gebruikersnaam mag maximaal uit 15 karakters bestaan").When(x => x.Username.Length > 0);

            RuleFor(x => x.Postalcode).NotEmpty().WithMessage("Postcode is een verplicht veld");
            RuleFor(x => x.Postalcode).Must(x => x.Length == 6).WithMessage("Ongeldige postcode. Een postcode bestaat uit 4 cijfers en 2 letters").When(x => x.Postalcode.Length > 0);
            RuleFor(x => x.Postalcode).Must(x => new Regex(@"\d{4}[A-Z]{2}", RegexOptions.IgnoreCase).IsMatch(x)).WithMessage("Ongeldige postcode").When(x => x.Postalcode.Length > 0);

            RuleFor(x => x.Email).NotEmpty().WithMessage("E-mail is een verplicht veld");
            RuleFor(x => new EmailAddressAttribute().IsValid(x.Email)).Must(x => x == true).WithMessage("Ongeldig e-mailadres").When(x => x.Email.Length > 0);
            RuleFor(x => x.Email).Must(x => x.Length <= 254).WithMessage("E-mailadres mag maximaal uit 15 karakters bestaan").When(x => x.Email.Length > 0);

            RuleFor(x => x.Password).Must(x => x.Length >= 8).WithMessage("Ongeldig wachtwoord. Wachtwoord moet uit minimaal 8 karakters bestaan").When(x => x.Password.Length > 0);

            RuleFor(x => x.Confirm_Password).Must(x => x.Length >= 8).WithMessage("Ongeldig wachtwoord. Wachtwoord moet uit minimaal 8 karakters bestaan").When(x => x.Confirm_Password.Length > 0);
            RuleFor(x => x.Confirm_Password).Must((x, password) => x.Password.Equals(password)).WithMessage("Wachtwoorden komen niet met elkaar overeen").When(x => x.Confirm_Password.Length > 0);
        }
    }

}
