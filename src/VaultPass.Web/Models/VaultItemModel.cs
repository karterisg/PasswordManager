namespace VaultPass.Web.Models;

public record VaultItemModel(
    Guid Id,
    Guid VaultId,
    string Name,
    int Type,
    string EncryptedData,
    string? WebsiteUrl,
    bool IsFavorite = false,
    bool IsHiddenInTravelMode = false,
    Guid? TemplateId = null);

public record CreateVaultItemRequest(
    Guid VaultId,
    string Name,
    int Type,
    string EncryptedData,
    string? WebsiteUrl,
    Guid? TemplateId = null);

public record ItemTemplateFieldModel(
    Guid Id,
    string Name,
    string FieldType,
    bool IsEncrypted,
    bool IsRequired,
    int Order);

public record ItemTemplateModel(
    Guid Id,
    string Name,
    string Icon,
    bool IsBuiltIn,
    List<ItemTemplateFieldModel> Fields);

public record VaultItemHistoryModel(Guid Id, string EncryptedData, DateTime CreatedAt);

public record ExportItemModel(Guid Id, string Name, string EncryptedData, string? WebsiteUrl, DateTime CreatedAt);
public record ImportItemDto(string Name, string EncryptedData, string? WebsiteUrl);