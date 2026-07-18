using ExpenseManager.Application.DTOs;
using ExpenseManager.Domain.Enums;
using FluentValidation;

namespace ExpenseManager.Application.Validators;

public class ConfirmReceiptExtractionValidator : AbstractValidator<ConfirmReceiptExtractionDto>
{
    public ConfirmReceiptExtractionValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be exactly 3 characters.")
            .Matches("^[A-Z]+$").WithMessage("Currency must be a valid 3-letter currency code.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow).WithMessage("Date cannot be in the future.");

        RuleFor(x => x.ImportMode)
            .IsInEnum().WithMessage("Import mode is invalid.");

        RuleFor(x => x.Expenses)
            .NotEmpty().WithMessage("At least one expense is required.");

        RuleFor(x => x)
            .Must(dto => dto.ImportMode != ReceiptImportMode.Combined || dto.Expenses.Count == 1)
            .WithMessage("Combined import must create exactly one expense.");

        RuleFor(x => x)
            .Must(dto => dto.ImportMode != ReceiptImportMode.Itemized || dto.Expenses.Count >= 1)
            .WithMessage("Itemized import requires at least one expense.");

        RuleForEach(x => x.Expenses).ChildRules(expense =>
        {
            expense.RuleFor(e => e.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");

            expense.RuleFor(e => e.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            expense.RuleFor(e => e.Description)
                .MaximumLength(500).When(e => !string.IsNullOrEmpty(e.Description))
                .WithMessage("Description must not exceed 500 characters.");
        });
    }
}
