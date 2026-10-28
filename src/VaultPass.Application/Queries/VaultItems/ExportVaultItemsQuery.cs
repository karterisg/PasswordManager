using MediatR;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Queries.VaultItems;

public record ExportItemDto(Guid Id, string Name, string EncryptedData, string? WebsiteUrl, DateTime CreatedAt);
public record ExportVaultItemsQuery(Guid VaultId) : IRequest<List<ExportItemDto>>;

public class ExportVaultItemsQueryHandler(IVaultItemRepository repo)
    : IRequestHandler<ExportVaultItemsQuery, List<ExportItemDto>>
{
    public async Task<List<ExportItemDto>> Handle(ExportVaultItemsQuery request, CancellationToken ct)
    {
        var items = await repo.GetByVaultIdAsync(request.VaultId, ct);
        return items.Select(i => new ExportItemDto(i.Id, i.Name, i.EncryptedData, i.WebsiteUrl, i.CreatedAt)).ToList();
    }
}
