namespace VaultPass.Domain.Enums;

public enum SharePermission
{
    View = 1, // can only view the shared vault item
    Edit = 2, // can view and edit the shared vault item
    Admin = 3 // can view, edit, and manage the shared vault item   
}