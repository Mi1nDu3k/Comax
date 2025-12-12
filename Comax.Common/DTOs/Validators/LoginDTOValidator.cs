using Comax.Common.DTOs.Auth;
using Comax.Shared;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.Auth
{
    public class LoginDTOValidator : AbstractValidator<LoginDTO>
    {
        public LoginDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ErrorMessages.Validation.EmailRequired)
                .EmailAddress().WithMessage(ErrorMessages.Validation.EmailInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ErrorMessages.Validation.PasswordRequired)
                .MinimumLength(6).WithMessage(string.Format(ErrorMessages.Validation.PasswordMinLength, 6));
        }
    }
}