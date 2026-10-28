using MediatR;

namespace VaultPass.Application.Commands.VaultItems;

public record DeleteVaultItemCommand(Guid Id, Guid CallerVaultId) : IRequest;