using MediatR;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Commands.VaultItems;

public record SetItemTravelHiddenCommand(Guid VaultItemId, bool Hidden, Guid CallerVaultId) : IRequest;

public class SetItemTravelHiddenCommandHandler(IVaultItemRepository repo)
    : IRequestHandler<SetItemTravelHiddenCommand>
{
    public async Task Handle(SetItemTravelHiddenCommand request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.VaultItemId, ct);
        if (item is null || item.VaultId != request.CallerVaultId)
            throw new UnauthorizedAccessException("Item not found or access denied.");
        item.SetHiddenInTravelMode(request.Hidden);
        await repo.SaveChangesAsync(ct);
    }
}
