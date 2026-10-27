using VaultPass.Domain.Common;

namespace VaultPass.Domain.Entities;

public class Team : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    private readonly List<TeamMember> _members = new();
    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    private Team() { }

    public static Team Create(string name, Guid createdByUserId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be empty.");

        return new Team
        {
            Name = name,
            Description = description,
            CreatedByUserId = createdByUserId
        };
    }

    public void AddMember(TeamMember member)
    {
        if (_members.Any(m => m.UserId == member.UserId))
            throw new InvalidOperationException("User is already a member of this team.");

        _members.Add(member);
        SetUpdated();
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new InvalidOperationException("User is not a member of this team.");

        _members.Remove(member);
        SetUpdated();
    }
}