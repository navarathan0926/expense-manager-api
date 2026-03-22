using FluentValidation;
using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Validators
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description must not exceed 500 characters.");
        }
    }
}
