using MediatR;
using VaultPass.Application.DTOs;
using VaultPass.Domain.Enums;

namespace VaultPass.Application.Commands.VaultItems;

// Command sent from the API to create a new vault item (password entry).
// Response is the created item's DTO.
public record CreateVaultItemCommand(
    Guid VaultId,
    string Name,
    VaultItemType Type,
    string EncryptedData,
    string? WebsiteUrl
) : IRequest<VaultItemDto>;
