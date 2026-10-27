using MediatR;

namespace VaultPass.Application.Commands.VaultItems;

public record DeleteVaultItemCommand(Guid Id) : IRequest;