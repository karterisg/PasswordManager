using VaultPass.Domain.Entities;

namespace VaultPass.Application.Interfaces;

public interface ISharedItemRepository
{
    Task<SharedItem?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<List<SharedItem>> GetByVaultItemIdAsync(Guid vaultItemId, CancellationToken ct = default);
    Task AddAsync(SharedItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
