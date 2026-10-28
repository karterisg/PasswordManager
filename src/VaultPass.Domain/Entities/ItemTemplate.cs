using VaultPass.Domain.Common;

namespace VaultPass.Domain.Entities;

public class ItemTemplate : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Icon { get; private set; } = "🔑";
    public bool IsBuiltIn { get; private set; }
    public Guid? VaultId { get; private set; }

    private readonly List<ItemTemplateField> _fields = [];
    public IReadOnlyList<ItemTemplateField> Fields => _fields.AsReadOnly();

    private ItemTemplate() { }

    public static ItemTemplate CreateBuiltIn(string name, string icon)
        => new() { Name = name, Icon = icon, IsBuiltIn = true };

    public static ItemTemplate CreateCustom(Guid vaultId, string name, string icon = "🔑")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");
        return new() { Name = name, Icon = icon, IsBuiltIn = false, VaultId = vaultId };
    }

    public void AddField(string name, Domain.Enums.FieldType type, bool isEncrypted, bool isRequired, int order)
        => _fields.Add(ItemTemplateField.Create(Id, name, type, isEncrypted, isRequired, order));

    public void Update(string name, string icon)
    {
        Name = name;
        Icon = icon;
        SetUpdated();
    }
}
