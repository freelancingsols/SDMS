using FluentValidation;

namespace SDMS.AuthenticationWebApp.Models.Requests;

/// <summary>
/// Validator for RegisterRequest using FluentValidation
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.DisplayName)
            .MaximumLength(256).WithMessage("Display name must not exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));
    }
}

