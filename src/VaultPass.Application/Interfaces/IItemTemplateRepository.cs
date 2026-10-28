using VaultPass.Domain.Entities;

namespace VaultPass.Application.Interfaces;

public interface IItemTemplateRepository
{
    Task<List<ItemTemplate>> GetAllAsync(Guid? vaultId, CancellationToken ct = default);
    Task<ItemTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ItemTemplate template, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
