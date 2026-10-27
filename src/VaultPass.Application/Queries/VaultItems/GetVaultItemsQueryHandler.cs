using MediatR;
using VaultPass.Application.DTOs;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Queries.VaultItems;

public class GetVaultItemsQueryHandler
    : IRequestHandler<GetVaultItemsQuery, IEnumerable<VaultItemDto>>
{
    private readonly IVaultItemRepository _vaultItemRepository;

    public GetVaultItemsQueryHandler(IVaultItemRepository vaultItemRepository)
    {
        _vaultItemRepository = vaultItemRepository;
    }

    public async Task<IEnumerable<VaultItemDto>> Handle(
        GetVaultItemsQuery query,
        CancellationToken ct)
    {
        var items = await _vaultItemRepository.GetByVaultIdAsync(query.VaultId, ct);

        // Map Domain → DTO
        return items.Select(item => new VaultItemDto(
            item.Id,
            item.VaultId,
            item.Name,
            item.Type,
            item.EncryptedData,
            item.WebsiteUrl,
            item.IsFavorite,
            item.LastAccessedAt,
            item.CreatedAt,
            item.UpdatedAt
        ));
    }
}