// Request models for the authentication API endpoints.
// Records are used because they are immutable value objects with no extra behavior —
// they exist only to serialize into JSON for HTTP POST bodies.
namespace VaultPass.MAUI.Models;

// Matches the body expected by POST /api/auth/login
public record LoginRequest(string Email, string MasterPasswordHash);

// Matches the body expected by POST /api/auth/register
public record RegisterRequest(string Email, string DisplayName, string MasterPasswordHash);
