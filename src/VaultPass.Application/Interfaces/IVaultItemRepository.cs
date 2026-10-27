using VaultPass.Domain.Entities;

namespace VaultPass.Application.Interfaces;

public interface IVaultItemRepository
{
    Task<VaultItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<VaultItem>> GetByVaultIdAsync(Guid vaultId, CancellationToken ct = default); //list all items in a vault
    Task AddAsync(VaultItem item, CancellationToken ct = default);
    Task UpdateAsync(VaultItem item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}