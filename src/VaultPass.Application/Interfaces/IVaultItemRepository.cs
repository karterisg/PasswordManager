using VaultPass.Domain.Entities;

namespace VaultPass.Application.Interfaces;

public interface IVaultItemRepository
{
    Task<VaultItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<VaultItem>> GetByVaultIdAsync(Guid vaultId, CancellationToken ct = default);
    Task AddAsync(VaultItem item, CancellationToken ct = default);
    Task UpdateAsync(VaultItem item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<List<VaultItemHistory>> GetHistoryAsync(Guid vaultItemId, CancellationToken ct = default);
    Task AddHistoryAsync(VaultItemHistory history, CancellationToken ct = default);
}