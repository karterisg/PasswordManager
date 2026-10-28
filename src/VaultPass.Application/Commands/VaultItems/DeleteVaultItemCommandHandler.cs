using MediatR;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Commands.VaultItems;

public class DeleteVaultItemCommandHandler : IRequestHandler<DeleteVaultItemCommand>
{
    private readonly IVaultItemRepository _repository;

    public DeleteVaultItemCommandHandler(IVaultItemRepository repository)
        => _repository = repository;

    public async Task Handle(DeleteVaultItemCommand command, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(command.Id, ct);
        if (item is null || item.VaultId != command.CallerVaultId)
            throw new UnauthorizedAccessException("Item not found or access denied.");
        await _repository.DeleteAsync(command.Id, ct);
        await _repository.SaveChangesAsync(ct);
    }
}
