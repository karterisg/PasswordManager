using VaultPass.Domain.Entities;

namespace VaultPass.Application.Interfaces;

public interface IJwtService
{
    //string GenerateToken(User user);
    string GenerateToken(User user, Guid vaultId); //changed signatures to this
}