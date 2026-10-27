using VaultPass.Domain.Common;
using VaultPass.Domain.Enums;

namespace VaultPass.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    // The master password is never stored in plain text — only its SHA-256 hash
    public string MasterPasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    // Private constructor forces callers to use the Create factory method
    private User() { }

    public static User Create(
        string email,
        string displayName,
        string masterPasswordHash,
        UserRole role = UserRole.Employee)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.");

        return new User
        {
            Email = email.ToLower().Trim(),
            DisplayName = displayName,
            MasterPasswordHash = masterPasswordHash,
            Role = role
        };
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        SetUpdated();
    }
}
