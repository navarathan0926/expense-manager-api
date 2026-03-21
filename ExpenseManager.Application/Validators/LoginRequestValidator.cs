using FluentValidation;
using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MaximumLength(128).WithMessage("Password must not exceed 128 characters.");
        }
    }
}
