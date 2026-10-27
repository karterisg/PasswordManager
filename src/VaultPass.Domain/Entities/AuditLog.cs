using VaultPass.Domain.Common;

namespace VaultPass.Domain.Entities;

//log of user actions for auditing purposes
public class AuditLog : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(Guid userId,string action,string entityType,Guid entityId,string? ipAddress = null)
    {
        return new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            IpAddress = ipAddress,
            OccurredAt = DateTime.UtcNow
        };
    }
}