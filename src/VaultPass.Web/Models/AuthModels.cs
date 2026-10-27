namespace VaultPass.Web.Models;

public record LoginRequest(string Email, string MasterPasswordHash);
public record RegisterRequest(string Email, string DisplayName, string MasterPasswordHash);
public record AuthResponse(string Token);
public record RegisterResponse(Guid UserId);