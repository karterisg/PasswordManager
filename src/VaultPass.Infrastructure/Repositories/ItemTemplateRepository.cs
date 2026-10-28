using Microsoft.EntityFrameworkCore;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Entities;
using VaultPass.Infrastructure.Persistence;

namespace VaultPass.Infrastructure.Repositories;

public class ItemTemplateRepository(AppDbContext db) : IItemTemplateRepository
{
    public async Task<List<ItemTemplate>> GetAllAsync(Guid? vaultId, CancellationToken ct = default)
        => await db.ItemTemplates
            .Include(t => t.Fields)
            .Where(t => t.IsBuiltIn || t.VaultId == vaultId)
            .ToListAsync(ct);

    public async Task<ItemTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.ItemTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(ItemTemplate template, CancellationToken ct = default)
        => await db.ItemTemplates.AddAsync(template, ct);

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var template = await db.ItemTemplates.FindAsync([id], ct);
        if (template is not null) db.ItemTemplates.Remove(template);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}
