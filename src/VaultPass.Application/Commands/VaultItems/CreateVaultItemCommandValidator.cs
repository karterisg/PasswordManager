using FluentValidation;

namespace VaultPass.Application.Commands.VaultItems;

public class CreateVaultItemCommandValidator : AbstractValidator<CreateVaultItemCommand>
{
    public CreateVaultItemCommandValidator()
    {
        RuleFor(x => x.VaultId)
            .NotEqual(Guid.Empty).WithMessage("Needs a valid Vault ID");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100);

        RuleFor(x => x.EncryptedData)
            .NotEmpty().WithMessage("Encrypted data is required");

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(500)
            .When(x => x.WebsiteUrl is not null);
    }

}