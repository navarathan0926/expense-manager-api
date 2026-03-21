using FluentValidation;
using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches("[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>?]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName is required.")
                .MinimumLength(3).WithMessage("UserName must be at least 3 characters.")
                .MaximumLength(50).WithMessage("UserName must not exceed 50 characters.")
                .Matches("^[a-zA-Z0-9]+$").WithMessage("UserName must contain only alphanumeric characters.");
        }
    }
}
