using Microsoft.EntityFrameworkCore;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Entities;
using VaultPass.Infrastructure.Persistence;

namespace VaultPass.Infrastructure.Repositories;

public class SharedItemRepository(AppDbContext context) : ISharedItemRepository
{
    public async Task<SharedItem?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await context.SharedItems.FirstOrDefaultAsync(s => s.ShareToken == token, ct);

    public async Task<List<SharedItem>> GetByVaultItemIdAsync(Guid vaultItemId, CancellationToken ct = default)
        => await context.SharedItems.Where(s => s.VaultItemId == vaultItemId).ToListAsync(ct);

    public async Task AddAsync(SharedItem item, CancellationToken ct = default)
        => await context.SharedItems.AddAsync(item, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
