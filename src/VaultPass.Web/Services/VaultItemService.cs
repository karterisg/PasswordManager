using System.Net.Http.Headers;
using System.Net.Http.Json;
using VaultPass.Web.Models;

namespace VaultPass.Web.Services;

public class VaultItemService
{
    private readonly HttpClient _http;
    private readonly LocalStorageService _localStorage;

    public VaultItemService(HttpClient http, LocalStorageService localStorage)
    {
        _http = http;
        _localStorage = localStorage;
    }

    public async Task<List<VaultItemModel>> GetItemsAsync(Guid vaultId)
    {
        await SetAuthHeader();
        var result = await _http.GetFromJsonAsync<List<VaultItemModel>>(
            $"api/vaultitems?vaultId={vaultId}");
        return result ?? [];
    }

    public async Task<bool> CreateItemAsync(CreateVaultItemRequest request)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/vaultitems", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateItemAsync(Guid id, string name, string encryptedData, string? websiteUrl)
    {
        await SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/vaultitems/{id}",
            new { Id = id, Name = name, EncryptedData = encryptedData, WebsiteUrl = websiteUrl });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync($"api/vaultitems/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ItemTemplateModel>> GetTemplatesAsync(Guid vaultId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ItemTemplateModel>>($"api/itemtemplates?vaultId={vaultId}") ?? [];
    }

    public async Task<List<VaultItemHistoryModel>> GetHistoryAsync(Guid itemId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<VaultItemHistoryModel>>($"api/vaultitems/{itemId}/history") ?? [];
    }

    public async Task<List<ExportItemModel>> ExportAsync(Guid vaultId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ExportItemModel>>($"api/vaultitems/export?vaultId={vaultId}") ?? [];
    }

    public async Task<int> ImportAsync(Guid vaultId, List<ImportItemDto> items)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/vaultitems/import", new { VaultId = vaultId, Items = items });
        if (!response.IsSuccessStatusCode) return 0;
        var result = await response.Content.ReadFromJsonAsync<ImportResult>();
        return result?.Imported ?? 0;
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

    private record ImportResult(int Imported);
    private record ShareResult(string Token);

    private async Task SetAuthHeader()
    {
        var token = await _localStorage.GetAsync("token");
        if (token is not null)
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }
}