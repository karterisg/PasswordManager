using Microsoft.JSInterop;

namespace VaultPass.Web.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _js;
    public LocalStorageService(IJSRuntime js) => _js = js;

    public async Task SetAsync(string key, string value)
        => await _js.InvokeVoidAsync("localStorage.setItem", key, value);

    public async Task<string?> GetAsync(string key)
        => await _js.InvokeAsync<string?>("localStorage.getItem", key);

    public async Task RemoveAsync(string key)
        => await _js.InvokeVoidAsync("localStorage.removeItem", key);

    public async Task SetSessionAsync(string key, string value)
        => await _js.InvokeVoidAsync("sessionStorage.setItem", key, value);

    public async Task<string?> GetSessionAsync(string key)
        => await _js.InvokeAsync<string?>("sessionStorage.getItem", key);

    public async Task RemoveSessionAsync(string key)
        => await _js.InvokeVoidAsync("sessionStorage.removeItem", key);
}