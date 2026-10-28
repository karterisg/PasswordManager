using MediatR;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Queries.Sharing;

public record SharedItemDto(Guid VaultItemId, string EncryptedData, string Name, DateTime ExpiresAt);
public record GetSharedItemQuery(string Token) : IRequest<SharedItemDto?>;

public class GetSharedItemQueryHandler(ISharedItemRepository shareRepo, IVaultItemRepository itemRepo)
    : IRequestHandler<GetSharedItemQuery, SharedItemDto?>
{
    public async Task<SharedItemDto?> Handle(GetSharedItemQuery request, CancellationToken ct)
    {
        var share = await shareRepo.GetByTokenAsync(request.Token, ct);
        if (share is null || !share.IsValid()) return null;

        var item = await itemRepo.GetByIdAsync(share.VaultItemId, ct);
        if (item is null) return null;

        share.RecordUse();
        await shareRepo.SaveChangesAsync(ct);

        return new SharedItemDto(item.Id, item.EncryptedData, item.Name, share.ExpiresAt);
    }
}
