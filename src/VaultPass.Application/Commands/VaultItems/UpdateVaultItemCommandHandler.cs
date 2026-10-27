using MediatR;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Commands.VaultItems;

public class UpdateVaultItemCommandHandler : IRequestHandler<UpdateVaultItemCommand>
{
    private readonly IVaultItemRepository _repository;

    public UpdateVaultItemCommandHandler(IVaultItemRepository repository)
        => _repository = repository;

    public async Task Handle(UpdateVaultItemCommand command, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"VaultItem {command.Id} not found.");

        // Delegate mutation to the domain entity so business rules are enforced there
        item.Update(command.Name, command.EncryptedData, command.WebsiteUrl);

        await _repository.UpdateAsync(item, ct);
        await _repository.SaveChangesAsync(ct);
    }
}
