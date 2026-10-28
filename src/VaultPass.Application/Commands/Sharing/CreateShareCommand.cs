using MediatR;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Entities;

namespace VaultPass.Application.Commands.Sharing;

public record CreateShareCommand(Guid VaultItemId, int ExpiryHours, int MaxUses, Guid CallerVaultId) : IRequest<string>;

public class CreateShareCommandHandler(ISharedItemRepository repo, IVaultItemRepository itemRepo)
    : IRequestHandler<CreateShareCommand, string>
{
    public async Task<string> Handle(CreateShareCommand request, CancellationToken ct)
    {
        var item = await itemRepo.GetByIdAsync(request.VaultItemId, ct);
        if (item is null || item.VaultId != request.CallerVaultId)
            throw new UnauthorizedAccessException("Item not found or access denied.");
        var share = SharedItem.Create(request.VaultItemId, request.ExpiryHours, request.MaxUses);
        await repo.AddAsync(share, ct);
        await repo.SaveChangesAsync(ct);
        return share.ShareToken;
    }
}
