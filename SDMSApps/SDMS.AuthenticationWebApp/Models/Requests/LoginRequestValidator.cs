using FluentValidation;

namespace SDMS.AuthenticationWebApp.Models.Requests;

/// <summary>
/// Validator for LoginRequest using FluentValidation
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // Email validation (required if provider is not specified)
        When(x => string.IsNullOrEmpty(x.Provider), () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required when using local authentication")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
        });

        // Password validation (required if provider is not specified)
        When(x => string.IsNullOrEmpty(x.Provider), () =>
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required when using local authentication")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters");
        });

        // Provider validation
        When(x => !string.IsNullOrEmpty(x.Provider), () =>
        {
            RuleFor(x => x.Provider)
                .Must(p => p == "auth0" || p == "google")
                .WithMessage("Provider must be either 'auth0' or 'google'");

            // If provider is specified, either idToken or code must be provided
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.IdToken) || !string.IsNullOrEmpty(x.Code))
                .WithMessage("Either IdToken or Code must be provided when using external authentication");
        });
    }
}

