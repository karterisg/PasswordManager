using MediatR;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Queries.VaultItems;

public record VaultItemHistoryDto(Guid Id, string EncryptedData, DateTime CreatedAt);
public record GetVaultItemHistoryQuery(Guid VaultItemId, Guid CallerVaultId) : IRequest<List<VaultItemHistoryDto>>;

public class GetVaultItemHistoryQueryHandler(IVaultItemRepository repo)
    : IRequestHandler<GetVaultItemHistoryQuery, List<VaultItemHistoryDto>>
{
    public async Task<List<VaultItemHistoryDto>> Handle(GetVaultItemHistoryQuery request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.VaultItemId, ct);
        if (item is null || item.VaultId != request.CallerVaultId)
            throw new UnauthorizedAccessException("Item not found or access denied.");
        var history = await repo.GetHistoryAsync(request.VaultItemId, ct);
        return history.Select(h => new VaultItemHistoryDto(h.Id, h.EncryptedData, h.CreatedAt)).ToList();
    }
}
