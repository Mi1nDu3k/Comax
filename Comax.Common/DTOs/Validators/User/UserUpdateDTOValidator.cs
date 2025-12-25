using Comax.Common.DTOs.User;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.User
{
    public class UserUpdateDTOValidator : AbstractValidator<UserUpdateDTO>
    {
        public UserUpdateDTOValidator()
        {
            //RuleFor(x => x.Username)
            //    .NotEmpty()
            //    .MinimumLength(3);

            //RuleFor(x => x.Email)
            //    .NotEmpty()
            //    .EmailAddress();
        }
    }
}
