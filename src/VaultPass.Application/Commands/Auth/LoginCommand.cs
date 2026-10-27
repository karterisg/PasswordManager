using MediatR;

namespace VaultPass.Application.Commands.Auth;

public record LoginCommand(
    string Email,
    string MasterPasswordHash
) : IRequest<string>; //jwt token returns