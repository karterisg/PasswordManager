using System.Net.Http.Json;
using VaultPass.Web.Models;
using static System.Net.WebRequestMethods;

namespace VaultPass.Web.Services;

public class AuthService
{
    private readonly HttpClient _http;

    public AuthService(HttpClient http) => _http = http;

    public async Task<string?> LoginAsync(string email, string hash)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login",
            new { email, masterPasswordHash = hash });
        if (!response.IsSuccessStatusCode) return null;

        var raw = await response.Content.ReadAsStringAsync();
        return raw.Trim('"');  // Remove quotes from the token string
    }

    public async Task<bool> RegisterAsync(string email, string displayName, string passwordHash)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register",
            new RegisterRequest(email, displayName, passwordHash));

        return response.IsSuccessStatusCode;
    }
}