using MediatR;

namespace VaultPass.Application.Commands.VaultItems;

public record UpdateVaultItemCommand(
    Guid Id,
    string Name,
    string EncryptedData,
    string? WebsiteUrl) : IRequest;