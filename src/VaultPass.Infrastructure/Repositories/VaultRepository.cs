using Microsoft.EntityFrameworkCore;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Entities;
using VaultPass.Infrastructure.Persistence;

namespace VaultPass.Infrastructure.Repositories;

public class VaultRepository : IVaultRepository
{
    private readonly AppDbContext _context;

    public VaultRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Vault?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Vaults
            .Include(v => v.Items)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Vault?> GetPersonalVaultByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.Vaults
            .Include(v => v.Items)
            .FirstOrDefaultAsync(v => v.OwnerId == userId && v.IsPersonal, ct);

    public async Task<IEnumerable<Vault>> GetTeamVaultsByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.Vaults
            .Include(v => v.Items)
            .Where(v => !v.IsPersonal && _context.TeamMembers
                .Any(tm => tm.TeamId == v.OwnerId && tm.UserId == userId))
            .ToListAsync(ct);

    public async Task AddAsync(Vault vault, CancellationToken ct = default)
        => await _context.Vaults.AddAsync(vault, ct);

    public Task UpdateAsync(Vault vault, CancellationToken ct = default)
    {
        _context.Vaults.Update(vault);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}