using VaultPass.Domain.Common;
using VaultPass.Domain.Exceptions;

namespace VaultPass.Domain.Entities;

public class Vault : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsPersonal { get; private set; }
    public Guid OwnerId { get; private set; }
    public bool TravelModeEnabled { get; private set; }

    private readonly List<VaultItem> _items = new();
    public IReadOnlyCollection<VaultItem> Items => _items.AsReadOnly();

    private Vault() { }

    public static Vault CreatePersonal(Guid userId, string ownerName)
    {
        return new Vault
        {
            Name = $"{ownerName}'s Vault",
            IsPersonal = true,
            OwnerId = userId
        };
    }

    public static Vault CreateShared(Guid teamId, string teamName)
    {
        return new Vault
        {
            Name = $"{teamName} Shared Vault",
            IsPersonal = false,
            OwnerId = teamId
        };
    }

    public void AddItem(VaultItem item)
    {
        if (_items.Any(i => i.Id == item.Id))
            throw new InvalidOperationException("Item already exists in this vault.");

        _items.Add(item);
        SetUpdated();
    }

    public void SetTravelMode(bool enabled)
    {
        TravelModeEnabled = enabled;
        SetUpdated();
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new VaultNotFoundException(itemId);

        _items.Remove(item);
        SetUpdated();
    }
}