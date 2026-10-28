using Microsoft.EntityFrameworkCore;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Entities;
using VaultPass.Infrastructure.Persistence;

namespace VaultPass.Infrastructure.Repositories;

public class VaultItemRepository : IVaultItemRepository
{
    private readonly AppDbContext _context;

    public VaultItemRepository(AppDbContext context) => _context = context;

    public async Task<VaultItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.VaultItems.FindAsync([id], ct);

    public async Task<List<VaultItem>> GetByVaultIdAsync(Guid vaultId, CancellationToken ct = default)
        => await _context.VaultItems
            .Where(i => i.VaultId == vaultId)
            .OrderBy(i => i.Name)
            .ToListAsync(ct);

    public async Task<List<VaultItemHistory>> GetHistoryAsync(Guid vaultItemId, CancellationToken ct = default)
        => await _context.VaultItemHistories
            .Where(h => h.VaultItemId == vaultItemId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

    public async Task AddHistoryAsync(VaultItemHistory history, CancellationToken ct = default)
        => await _context.VaultItemHistories.AddAsync(history, ct);

    public async Task AddAsync(VaultItem item, CancellationToken ct = default)
        => await _context.VaultItems.AddAsync(item, ct);

    public Task UpdateAsync(VaultItem item, CancellationToken ct = default)
    {
        _context.VaultItems.Update(item);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _context.VaultItems.FindAsync([id], ct);
        if (item is not null)
            _context.VaultItems.Remove(item);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}