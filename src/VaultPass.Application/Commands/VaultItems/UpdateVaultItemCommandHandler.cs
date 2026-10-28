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
        var item = await _repository.GetByIdAsync(command.Id, ct);
        if (item is null || item.VaultId != command.CallerVaultId)
            throw new UnauthorizedAccessException("Item not found or access denied.");

        var history = VaultPass.Domain.Entities.VaultItemHistory.Create(item.Id, item.EncryptedData);
        await _repository.AddHistoryAsync(history, ct);

        item.Update(command.Name, command.EncryptedData, command.WebsiteUrl);

        await _repository.UpdateAsync(item, ct);
        await _repository.SaveChangesAsync(ct);
    }
}
