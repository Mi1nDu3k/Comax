using Comax.Common.DTOs.Chapter;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.Chapter
{
    public class ChapterUpdateDTOValidator : AbstractValidator<ChapterUpdateDTO>
    {
        public ChapterUpdateDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty();

            RuleFor(x => x.ChapterNumber)
                .GreaterThan(0);
        }
    }
}
