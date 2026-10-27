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
        // Mark the item for removal, then flush to the database
        await _repository.DeleteAsync(command.Id, ct);
        await _repository.SaveChangesAsync(ct);
    }
}
