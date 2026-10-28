using VaultPass.Domain.Common;
using VaultPass.Domain.Enums;

namespace VaultPass.Domain.Entities;

public class ItemTemplateField : BaseEntity
{
    public Guid TemplateId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public FieldType FieldType { get; private set; }
    public bool IsEncrypted { get; private set; }
    public bool IsRequired { get; private set; }
    public int Order { get; private set; }

    private ItemTemplateField() { }

    public static ItemTemplateField Create(Guid templateId, string name, FieldType type, bool isEncrypted, bool isRequired, int order)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Field name is required.");
        return new()
        {
            TemplateId = templateId,
            Name = name,
            FieldType = type,
            IsEncrypted = isEncrypted,
            IsRequired = isRequired,
            Order = order
        };
    }
}
