// This service replaces LocalStorageService from the Web project.
//
// On the web we stored the JWT token and encryption key in the browser's
// localStorage / sessionStorage via JavaScript interop.
// On mobile/desktop we use the OS-level secure storage instead:
//   - iOS/macOS: Keychain
//   - Android:   Keystore (EncryptedSharedPreferences)
//   - Windows:   DPAPI (Data Protection API)
//
// The API surface (SetAsync / GetAsync / RemoveAsync) is intentionally identical
// to LocalStorageService so the Page components needed minimal changes.
//
// Two storage tiers:
//   Persistent (SecureStorage.Default.*) — survives app restarts, e.g. the JWT token
//   Session    (in-memory Dictionary)    — lost when the app closes, e.g. the encryption key
namespace VaultPass.MAUI.Services;

public class SecureStorageService
{
    // --- Persistent storage (OS keychain / keystore) ---

    // Encrypts and stores a value under the given key in the OS secure store
    public async Task SetAsync(string key, string value)
        => await SecureStorage.Default.SetAsync(key, value);

    // Retrieves a previously stored value; returns null if not found
    public async Task<string?> GetAsync(string key)
    {
        try { return await SecureStorage.Default.GetAsync(key); }
        catch { return null; }
    }

    // Removes a stored value (e.g. on logout to clear the token)
    public Task RemoveAsync(string key)
    {
        SecureStorage.Default.Remove(key);
        return Task.CompletedTask;
    }

    // --- Session storage (in-memory, wiped when the app closes) ---
    // Used for the AES encryption key: we never want it written to disk.

    private static readonly Dictionary<string, string> _session = [];

    public Task SetSessionAsync(string key, string value)
    {
        _session[key] = value;
        return Task.CompletedTask;
    }

    public Task<string?> GetSessionAsync(string key)
        => Task.FromResult(_session.TryGetValue(key, out var v) ? v : (string?)null);

    public Task RemoveSessionAsync(string key)
    {
        _session.Remove(key);
        return Task.CompletedTask;
    }
}
