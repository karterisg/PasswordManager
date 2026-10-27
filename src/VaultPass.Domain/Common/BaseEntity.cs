namespace VaultPass.Domain.Common;


//base entity class for all entities in the system
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    protected void SetUpdated() => UpdatedAt = DateTime.UtcNow;
}





