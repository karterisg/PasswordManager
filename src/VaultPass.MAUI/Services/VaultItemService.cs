// Handles all vault item API calls (list, create, delete).
// Functionally identical to the Web version, with one difference:
// it depends on SecureStorageService instead of LocalStorageService
// to read the JWT token for the Authorization header.
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VaultPass.MAUI.Models;

namespace VaultPass.MAUI.Services;

public class VaultItemService
{
    private readonly HttpClient _http;
    private readonly SecureStorageService _storage;

    public VaultItemService(HttpClient http, SecureStorageService storage)
    {
        _http = http;
        _storage = storage;
    }

    // GET /api/vaultitems?vaultId={vaultId} — returns all items in the vault
    public async Task<List<VaultItemModel>> GetItemsAsync(Guid vaultId)
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/vaultitems?vaultId={vaultId}");
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        var result = await response.Content.ReadFromJsonAsync<List<VaultItemModel>>();
        return result ?? [];
    }

    // POST /api/vaultitems — creates a new vault item (password entry)
    public async Task<bool> CreateItemAsync(CreateVaultItemRequest request)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/vaultitems", request);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        return response.IsSuccessStatusCode;
    }

    // DELETE /api/vaultitems/{id} — permanently removes a vault item
    public async Task<bool> DeleteItemAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync($"api/vaultitems/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        return response.IsSuccessStatusCode;
    }

    // Reads the stored JWT token and attaches it as a Bearer Authorization header.
    // Called before every request because the token may change after re-login.
    private async Task SetAuthHeader()
    {
        var token = await _storage.GetAsync("token");
        if (token is not null)
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }
}
