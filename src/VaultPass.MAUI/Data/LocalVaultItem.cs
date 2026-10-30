using SQLite;

namespace VaultPass.MAUI.Data;

[Table("VaultItems")]
public class LocalVaultItem
{
    [PrimaryKey]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public int Type { get; set; }
    public string EncryptedData { get; set; } = "";
    public string? WebsiteUrl { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsHiddenInTravelMode { get; set; }
    public string? TemplateName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("VaultItemHistory")]
public class LocalVaultItemHistory
{
    [PrimaryKey]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VaultItemId { get; set; }
    public string EncryptedData { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
