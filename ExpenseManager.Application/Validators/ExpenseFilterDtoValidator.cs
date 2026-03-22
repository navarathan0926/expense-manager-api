using ExpenseManager.Application.DTOs;
using FluentValidation;

namespace ExpenseManager.Application.Validators
{
    public class ExpenseFilterDtoValidator : AbstractValidator<ExpenseFilterDto>
{
    public ExpenseFilterDtoValidator()
    {
        When(x => x.FromDate.HasValue && x.ToDate.HasValue, () =>
        {
            RuleFor(x => x.FromDate)
                .LessThanOrEqualTo(x => x.ToDate)
                .WithMessage("From Date must be earlier than or equal to To Date.");
        });
    }
}
}