using VaultPass.Domain.Common;

namespace VaultPass.Domain.Entities;

public class VaultItemHistory : BaseEntity
{
    public Guid VaultItemId { get; private set; }
    public string EncryptedData { get; private set; } = string.Empty;

    private VaultItemHistory() { }

    public static VaultItemHistory Create(Guid vaultItemId, string encryptedData)
        => new() { VaultItemId = vaultItemId, EncryptedData = encryptedData };
}
