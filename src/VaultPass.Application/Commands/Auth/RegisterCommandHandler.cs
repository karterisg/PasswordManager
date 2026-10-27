using MediatR;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Entities;

namespace VaultPass.Application.Commands.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IVaultRepository _vaultRepository;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IVaultRepository vaultRepository)
    {
        _userRepository = userRepository;
        _vaultRepository = vaultRepository;
    }

    public async Task<Guid> Handle(RegisterCommand command, CancellationToken ct)
    {
        if (await _userRepository.ExistsAsync(command.Email, ct))
            throw new InvalidOperationException("Email already registered.");

        var user = User.Create(
            command.Email,
            command.DisplayName,
            command.MasterPasswordHash
        );

        await _userRepository.AddAsync(user, ct);

        // Every new user gets a personal vault created automatically
        var vault = Vault.CreatePersonal(user.Id, user.DisplayName);
        await _vaultRepository.AddAsync(vault, ct);

        // Single SaveChanges call persists both the user and the vault atomically
        await _userRepository.SaveChangesAsync(ct);

        return user.Id;
    }
}
