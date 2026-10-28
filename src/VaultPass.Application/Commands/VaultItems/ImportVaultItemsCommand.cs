using MediatR;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Entities;
using VaultPass.Domain.Enums;

namespace VaultPass.Application.Commands.VaultItems;

public record ImportItemDto(string Name, string EncryptedData, string? WebsiteUrl);
public record ImportVaultItemsCommand(Guid VaultId, List<ImportItemDto> Items) : IRequest<int>;

public class ImportVaultItemsCommandHandler(IVaultItemRepository repo)
    : IRequestHandler<ImportVaultItemsCommand, int>
{
    public async Task<int> Handle(ImportVaultItemsCommand request, CancellationToken ct)
    {
        var items = request.Items.Select(i =>
            VaultItem.Create(request.VaultId, i.Name, VaultItemType.Login, i.EncryptedData, i.WebsiteUrl)
        ).ToList();

        foreach (var item in items)
            await repo.AddAsync(item, ct);

        await repo.SaveChangesAsync(ct);
        return items.Count;
    }
}
