using Comax.Common.DTOs.Chapter;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.Chapter
{
    public class ChapterCreateDTOValidator : AbstractValidator<ChapterCreateDTO>
    {
        public ChapterCreateDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty();

            RuleFor(x => x.ComicId)
                .GreaterThan(0);

            RuleFor(x => x.ChapterNumber)
                .GreaterThan(0);
        }
    }
}
