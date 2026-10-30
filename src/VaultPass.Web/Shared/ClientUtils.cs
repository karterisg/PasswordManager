using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace VaultPass.Web.Shared;

public static class ClientUtils
{
    public static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }

    public static Guid ParseVaultIdFromToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return Guid.Empty;
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4) { case 2: payload += "=="; break; case 3: payload += "="; break; }
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("vaultId", out var v)) return Guid.Parse(v.GetString()!);
            return Guid.Empty;
        }
        catch { return Guid.Empty; }
    }
}
