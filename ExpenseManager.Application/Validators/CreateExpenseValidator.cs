using FluentValidation;
using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Validators
{
    public class CreateExpenseValidator : AbstractValidator<CreateExpenseDto>
    {
        public CreateExpenseValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required and must not be an empty.");

            RuleFor(x => x.Amount)
                .NotEmpty().WithMessage("Amount is required.")
                .GreaterThan(0).WithMessage("Amount must be greater than 0.")
                .LessThanOrEqualTo(9999999999999999.9999m).WithMessage("Amount is too large. Please enter a valid amount.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be exactly 3 characters.")
                .Matches("^[A-Z]+$").WithMessage("Currency must be a valid 3-letter currency code (e.g., USD, EUR).");

            RuleFor(x => x.ExchangeRate)
                .GreaterThan(0).When(x => x.ExchangeRate.HasValue)
                .WithMessage("ExchangeRate must be greater than 0 when provided.");

            RuleFor(x => x.Description)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.")
                .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow).WithMessage("Date cannot be in the future.");
        }
    }
}
