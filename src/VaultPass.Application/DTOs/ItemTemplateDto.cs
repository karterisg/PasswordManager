namespace VaultPass.Application.DTOs;

public record ItemTemplateDto(
    Guid Id,
    string Name,
    string Icon,
    bool IsBuiltIn,
    List<ItemTemplateFieldDto> Fields
);

public record ItemTemplateFieldDto(
    Guid Id,
    string Name,
    string FieldType,
    bool IsEncrypted,
    bool IsRequired,
    int Order
);
