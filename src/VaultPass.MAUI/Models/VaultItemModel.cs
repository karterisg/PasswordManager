// Data models for vault items.
// These mirror the DTOs returned/accepted by the VaultItems API controller.
// Using records because they map 1-to-1 to JSON and carry no business logic.
namespace VaultPass.MAUI.Models;

// Returned by GET /api/vaultitems — represents a stored password entry
public record VaultItemModel(
    Guid Id,
    Guid VaultId,
    string Name,          // Display name, e.g. "Gmail"
    int Type,             // Item type (1 = password)
    string EncryptedData, // AES-GCM encrypted password, stored as Base64
    string? WebsiteUrl    // Optional URL shown under the item name
);

// Sent to POST /api/vaultitems — payload for creating a new password entry
public record CreateVaultItemRequest(
    Guid VaultId,
    string Name,
    int Type,
    string EncryptedData,
    string? WebsiteUrl
);
