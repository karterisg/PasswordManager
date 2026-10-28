using VaultPass.Domain.Common;
using VaultPass.Domain.Enums;

namespace VaultPass.Domain.Entities;

public class VaultItem : BaseEntity
{
    public Guid VaultId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public VaultItemType Type { get; private set; }

    //AES-256 encrypted payload
    public string EncryptedData { get; private set; } = string.Empty;

    //no encryption in this field — we need it for display purposes
    public string? WebsiteUrl { get; private set; }
    public bool IsFavorite { get; private set; }
    public DateTime? LastAccessedAt { get; private set; }
    public Guid? TemplateId { get; private set; }

    private VaultItem() { }

    public static VaultItem Create(
        Guid vaultId,
        string name,
        VaultItemType type,
        string encryptedData,
        string? websiteUrl = null,
        Guid? templateId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.");

        if (string.IsNullOrWhiteSpace(encryptedData))
            throw new ArgumentException("Encrypted data cannot be empty.");

        return new VaultItem
        {
            VaultId = vaultId,
            Name = name,
            Type = type,
            EncryptedData = encryptedData,
            WebsiteUrl = websiteUrl,
            TemplateId = templateId
        };
    }

    public void UpdateEncryptedData(string newEncryptedData)
    {
        if (string.IsNullOrWhiteSpace(newEncryptedData))
            throw new ArgumentException("Encrypted data cannot be empty.");

        EncryptedData = newEncryptedData;
        SetUpdated();
    }

    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        SetUpdated();
    }


    public void Update(string name, string encryptedData, string? websiteUrl)
    {
        Name = name;
        EncryptedData = encryptedData;
        WebsiteUrl = websiteUrl;
        UpdatedAt = DateTime.UtcNow;
    }



    public void RecordAccess() => LastAccessedAt = DateTime.UtcNow; //this is for tracking purposes only, no need to call SetUpdated() here
}