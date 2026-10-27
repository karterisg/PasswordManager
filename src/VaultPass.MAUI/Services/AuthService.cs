// Handles login and registration API calls.
// This is a straight copy from VaultPass.Web — the HTTP logic is identical.
// The only reason it lives in this project is namespace isolation;
// it could theoretically be shared via a class library.
using System.Net.Http.Json;
using VaultPass.MAUI.Models;

namespace VaultPass.MAUI.Services;

public class AuthService
{
    private readonly HttpClient _http;
    public AuthService(HttpClient http) => _http = http;

    // POST /api/auth/login — returns a JWT token string on success, null on failure
    public async Task<string?> LoginAsync(string email, string hash)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login",
            new { email, masterPasswordHash = hash });
        if (!response.IsSuccessStatusCode) return null;
        var raw = await response.Content.ReadAsStringAsync();
        // The API returns the token as a quoted JSON string — strip the quotes
        return raw.Trim('"');
    }

    // POST /api/auth/register — returns true if the account was created successfully
    public async Task<bool> RegisterAsync(string email, string displayName, string passwordHash)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register",
            new RegisterRequest(email, displayName, passwordHash));
        return response.IsSuccessStatusCode;
    }
}
