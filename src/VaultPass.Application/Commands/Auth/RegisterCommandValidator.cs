using FluentValidation;

namespace VaultPass.Application.Commands.Auth;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address")
            .MaximumLength(256);

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(100);

        RuleFor(x => x.MasterPasswordHash)
            .NotEmpty().WithMessage("Password hash is required")
            .Length(64).WithMessage("Hash must be SHA-256 (64 characters)");
    }
}           