using Comax.Common.DTOs.Category;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.Category
{
    public class CategoryDTOValidator : AbstractValidator<CategoryDTO>
    {
        public CategoryDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);
        }
    }
}
