namespace VaultPass.Domain.Enums;

public enum UserRole
{
    Employee = 1, // regular user with access to their own vault items
    TeamLead = 2, // user with access to their own vault items and the ability to manage team members
    Admin = 3 // user with access to all vault items and the ability to manage all users //such as creating, updating, and deleting 
}