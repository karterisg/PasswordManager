using MediatR;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Commands.Vault;

public record SetTravelModeCommand(Guid VaultId, bool Enabled) : IRequest;

public class SetTravelModeCommandHandler(IVaultRepository repo)
    : IRequestHandler<SetTravelModeCommand>
{
    public async Task Handle(SetTravelModeCommand request, CancellationToken ct)
    {
        var vault = await repo.GetByIdAsync(request.VaultId, ct)
            ?? throw new InvalidOperationException("Vault not found.");
        vault.SetTravelMode(request.Enabled);
        await repo.SaveChangesAsync(ct);
    }
}
