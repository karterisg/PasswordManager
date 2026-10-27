using FluentValidation;

namespace VaultPass.Application.Commands.Auth;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        //email must be valid
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");

        //password hash must be 64 characters long (SHA-256)
        RuleFor(x => x.MasterPasswordHash)
            .NotEmpty().WithMessage("Password hash is required")
            .Length(64).WithMessage("Hash must be SHA-256 (64 characters)");
    }
}