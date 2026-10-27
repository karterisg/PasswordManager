using VaultPass.Domain.Enums;

namespace VaultPass.Application.DTOs;

//only read from api not domain entities
public record VaultItemDto(
    Guid Id,
    Guid VaultId,
    string Name,
    VaultItemType Type,
    string EncryptedData,  // encrypted data (e.g., password, note, etc.)
    string? WebsiteUrl,
    bool IsFavorite,
    DateTime? LastAccessedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);