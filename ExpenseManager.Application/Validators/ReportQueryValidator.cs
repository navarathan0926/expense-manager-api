using ExpenseManager.Application.DTOs;
using FluentValidation;

namespace ExpenseManager.Application.Validators;

public class ReportQueryValidator : AbstractValidator<ReportQueryDto>
{
    public ReportQueryValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Month must be between 1 and 12.");

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(2000)
            .LessThanOrEqualTo(DateTime.UtcNow.Year)
            .WithMessage("Invalid year.");
    }
}