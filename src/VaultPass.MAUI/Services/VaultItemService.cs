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

    public async Task<bool> UpdateItemAsync(Guid id, string name, string encryptedData, string? websiteUrl)
    {
        await SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/vaultitems/{id}",
            new { Id = id, Name = name, EncryptedData = encryptedData, WebsiteUrl = websiteUrl });
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException();
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync($"api/vaultitems/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ItemTemplateModel>> GetTemplatesAsync(Guid vaultId)
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/itemtemplates?vaultId={vaultId}");
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException();
        return await response.Content.ReadFromJsonAsync<List<ItemTemplateModel>>() ?? [];
    }

    public async Task<List<VaultItemHistoryModel>> GetHistoryAsync(Guid itemId)
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/vaultitems/{itemId}/history");
        return await response.Content.ReadFromJsonAsync<List<VaultItemHistoryModel>>() ?? [];
    }

    public async Task<List<ExportItemModel>> ExportAsync(Guid vaultId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ExportItemModel>>($"api/vaultitems/export?vaultId={vaultId}") ?? [];
    }

    public async Task<string?> CreateShareAsync(Guid itemId, int expiryHours = 24, int maxUses = 1)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/share",
            new { VaultItemId = itemId, ExpiryHours = expiryHours, MaxUses = maxUses });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<ShareResult>();
        return result?.Token;
    }

    public async Task<bool> SetTravelModeAsync(Guid vaultId, bool enabled)
    {
        await SetAuthHeader();
        var response = await _http.PatchAsJsonAsync($"api/vault/{vaultId}/travel-mode", new { Enabled = enabled });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetItemTravelHiddenAsync(Guid itemId, bool hidden)
    {
        await SetAuthHeader();
        var response = await _http.PatchAsJsonAsync($"api/vaultitems/{itemId}/travel-hidden", new { Hidden = hidden });
        return response.IsSuccessStatusCode;
    }

    private record ShareResult(string Token);

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
