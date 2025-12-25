using Comax.Common.DTOs.Author;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.Author
{
    public class AuthorDTOValidator : AbstractValidator<AuthorDTO>
    {
        public AuthorDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100);
        }
    }
}
