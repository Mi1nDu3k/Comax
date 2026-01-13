using Comax.Common.DTOs.Comic;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.Comic
{
    public class ComicUpdateDTOValidator : AbstractValidator<ComicUpdateDTO>
    {
        public ComicUpdateDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MinimumLength(3);

            RuleFor(x => x.Description)
                .MaximumLength(2000);
        }
    }
}
