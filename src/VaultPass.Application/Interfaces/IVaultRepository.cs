using VaultPass.Domain.Entities;

namespace VaultPass.Application.Interfaces;

public interface IVaultRepository
{
    Task<Vault?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Vault?> GetPersonalVaultByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Vault>> GetTeamVaultsByUserIdAsync(Guid userId, CancellationToken ct = default); //list of vaults that the user has access to through their teams
    Task AddAsync(Vault vault, CancellationToken ct = default);
    Task UpdateAsync(Vault vault, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}