using Comax.Common.DTOs.Comic;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.Comic
{
    public class ComicCreateDTOValidator : AbstractValidator<ComicCreateDTO>
    {
        public ComicCreateDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MinimumLength(3);

            RuleFor(x => x.Description)
                .MaximumLength(2000);

            RuleFor(x => x.AuthorId)
                .GreaterThan(0);

            RuleFor(x => x.CategoryID)
                .NotEmpty()
                .WithMessage("At least one category is required.");
        }
    }
}
