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

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync($"api/vaultitems/{id}");
        return response.IsSuccessStatusCode;
    }

    private async Task SetAuthHeader()
    {
        var token = await _localStorage.GetAsync("token");
        if (token is not null)
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }
}