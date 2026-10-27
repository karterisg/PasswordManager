using VaultPass.Domain.Common;
using VaultPass.Domain.Enums;

namespace VaultPass.Domain.Entities;

//user---team relationship entity
public class TeamMember : BaseEntity
{
    public Guid TeamId { get; private set; }
    public Guid UserId { get; private set; }
    public SharePermission Permission { get; private set; }

    //private constructor to enforce the use of the factory method
    private TeamMember() { }


    //factory method to create a new TeamMember instance
    public static TeamMember Create(Guid teamId, Guid userId, SharePermission permission = SharePermission.View)
    {
        return new TeamMember
        {
            TeamId = teamId,
            UserId = userId,
            Permission = permission,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdatePermission(SharePermission newPermission)
    {
        Permission = newPermission;
        SetUpdated();
    }
}