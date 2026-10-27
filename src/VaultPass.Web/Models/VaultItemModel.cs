namespace VaultPass.Web.Models;

public record VaultItemModel(
    Guid Id,
    Guid VaultId,
    string Name,
    int Type,
    string EncryptedData,
    string? WebsiteUrl);

public record CreateVaultItemRequest(
    Guid VaultId,
    string Name,
    int Type,
    string EncryptedData,
    string? WebsiteUrl);