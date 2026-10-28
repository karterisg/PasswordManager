using VaultPass.Domain.Common;

namespace VaultPass.Domain.Entities;

public class SharedItem : BaseEntity
{
    public Guid VaultItemId { get; private set; }
    public string ShareToken { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public int MaxUses { get; private set; }
    public int UsedCount { get; private set; }
    public bool IsRevoked { get; private set; }

    private SharedItem() { }

    public static SharedItem Create(Guid vaultItemId, int expiryHours = 24, int maxUses = 1)
        => new()
        {
            VaultItemId = vaultItemId,
            ShareToken = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours),
            MaxUses = maxUses
        };

    public bool IsValid() => !IsRevoked && DateTime.UtcNow < ExpiresAt && UsedCount < MaxUses;

    public void RecordUse()
    {
        UsedCount++;
        SetUpdated();
    }

    public void Revoke()
    {
        IsRevoked = true;
        SetUpdated();
    }
}
