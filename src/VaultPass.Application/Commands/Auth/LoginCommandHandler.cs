using MediatR;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IVaultRepository _vaultRepository;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IVaultRepository vaultRepository,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _vaultRepository = vaultRepository;
        _jwtService = jwtService;
    }

    public async Task<string> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        // The client sends a SHA-256 hash of the master password — compare hashes only, never plain text
        if (user.MasterPasswordHash != command.MasterPasswordHash)
            throw new UnauthorizedAccessException("Invalid credentials.");

        // The vaultId is embedded in the JWT so the frontend knows which vault to operate on
        var vault = await _vaultRepository.GetPersonalVaultByUserIdAsync(user.Id, ct)
            ?? throw new UnauthorizedAccessException("No vault found.");

        user.RecordLogin();
        await _userRepository.SaveChangesAsync(ct);

        return _jwtService.GenerateToken(user, vault.Id);
    }
}
