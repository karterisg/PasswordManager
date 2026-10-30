using System.Security.Cryptography;
using System.Text;

namespace VaultPass.MAUI.Services;

public class AuthService
{
    private readonly SecureStorageService _storage;

    public AuthService(SecureStorageService storage) => _storage = storage;

    // Recovery code: 20 chars grouped as XXXXX-XXXXX-XXXXX-XXXXX
    public string GenerateRecoveryCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new byte[20];
        RandomNumberGenerator.Fill(random);
        var raw = new string(random.Select(b => chars[b % chars.Length]).ToArray());
        return $"{raw[..5]}-{raw[5..10]}-{raw[10..15]}-{raw[15..20]}";
    }

    public async Task SaveRecoveryCodeAsync(string code)
    {
        var normalized = code.Replace("-", "").ToUpperInvariant();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("vaultpass_recovery:" + normalized)));
        await _storage.SetAsync("recovery_hash", hash);
    }

    public async Task<bool> VerifyRecoveryCodeAsync(string code)
    {
        var stored = await _storage.GetAsync("recovery_hash");
        if (stored is null) return false;
        var normalized = code.Replace("-", "").ToUpperInvariant();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("vaultpass_recovery:" + normalized)));
        return hash == stored;
    }

    public async Task ResetPinAsync(string newPin)
    {
        await _storage.RemoveAsync("pin_hash");
        await SetPinAsync(newPin);
    }

    public async Task<bool> IsPinSetAsync()
        => await _storage.GetAsync("pin_hash") is not null;

    public async Task SetPinAsync(string pin)
    {
        var hash = HashPin(pin);
        await _storage.SetAsync("pin_hash", hash);
        if (await _storage.GetAsync("vault_id") is null)
            await _storage.SetAsync("vault_id", Guid.NewGuid().ToString());
    }

    public async Task<bool> VerifyPinAsync(string pin)
    {
        var stored = await _storage.GetAsync("pin_hash");
        if (stored is null) return false;
        return HashPin(pin) == stored;
    }

    public async Task<Guid?> GetVaultIdAsync()
    {
        var id = await _storage.GetAsync("vault_id");
        return id is null ? null : Guid.Parse(id);
    }

    public async Task LockAsync()
        => await _storage.RemoveSessionAsync("unlocked");

    public async Task UnlockAsync()
        => await _storage.SetSessionAsync("unlocked", "true");

    public async Task<bool> IsUnlockedAsync()
        => await _storage.GetSessionAsync("unlocked") == "true";

    public async Task<string> DeriveEncKeyAsync(string pin)
    {
        var vaultId = await _storage.GetAsync("vault_id") ?? "";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"vaultpass_enc:{pin}:{vaultId}"));
        return Convert.ToBase64String(bytes);
    }

    private static string HashPin(string pin)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("vaultpass_pin:" + pin));
        return Convert.ToHexString(bytes);
    }
}
