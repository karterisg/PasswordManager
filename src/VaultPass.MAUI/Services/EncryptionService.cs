// Derives the AES-256 encryption key used to encrypt/decrypt vault passwords.
// Exact copy from VaultPass.Web — the algorithm must be identical on both platforms
// so that passwords encrypted on the web can be decrypted in the app and vice versa.
//
// How it works:
//   1. Concatenate the master password hash + vaultId (hyphens removed)
//   2. SHA-256 hash the result → 32 bytes → valid AES-256 key
//   3. Return as Base64 so it can be passed to the vault-crypto.js functions
//
// The actual AES-GCM encrypt/decrypt runs in vault-crypto.js (WebCrypto API)
// because the BlazorWebView has a full JavaScript engine available.
using System.Security.Cryptography;
using System.Text;

namespace VaultPass.MAUI.Services;

public static class EncryptionService
{
    public static string DeriveKeyBase64(string passwordHash, string vaultId)
    {
        var combined = Encoding.UTF8.GetBytes(passwordHash + vaultId.Replace("-", ""));
        return Convert.ToBase64String(SHA256.HashData(combined));
    }
}
