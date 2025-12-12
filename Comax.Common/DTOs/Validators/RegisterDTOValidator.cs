using Comax.Common.DTOs.Auth;
using Comax.Shared;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.Auth
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(ErrorMessages.Validation.UsernameRequired)
                .MinimumLength(3).WithMessage(string.Format(ErrorMessages.Validation.UsernameMinLength, 3));

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ErrorMessages.Validation.EmailRequired)
                .EmailAddress().WithMessage(ErrorMessages.Validation.EmailInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ErrorMessages.Validation.PasswordRequired)
                .MinimumLength(8).WithMessage(string.Format(ErrorMessages.Validation.PasswordMinLength, 8))
                .Matches(@"[A-Z]").WithMessage(ErrorMessages.Validation.PasswordUppercase)
                .Matches(@"[a-z]").WithMessage(ErrorMessages.Validation.PasswordLowercase)
                .Matches(@"[0-9]").WithMessage(ErrorMessages.Validation.PasswordDigit)
                .Matches(@"[\!\?\*\@\#\$\%\^\&\(\)\.\,\;\:\<\>\{\}\[\]\-_=\+]")
                .WithMessage(ErrorMessages.Validation.PasswordSpecialChar);
        }
    }
}