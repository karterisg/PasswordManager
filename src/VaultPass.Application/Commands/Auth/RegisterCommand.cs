using MediatR;

namespace VaultPass.Application.Commands.Auth;

public record RegisterCommand(
    string Email,
    string DisplayName,
    string MasterPasswordHash
) : IRequest<Guid>;