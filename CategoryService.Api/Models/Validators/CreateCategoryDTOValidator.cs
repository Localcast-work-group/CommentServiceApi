using CategoryService.Api.Interfaces.Services;
using CategoryService.Api.Models.DTOs;
using FluentValidation;

namespace CategoryService.Api.Models.Validators
{
    public class CreateCategoryDTOValidator : AbstractValidator<CreateCategoryDTO>
    {
        public CreateCategoryDTOValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(200);

        }
    }
}
