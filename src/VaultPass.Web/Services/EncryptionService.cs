using System.Security.Cryptography;
using System.Text;

namespace VaultPass.Web.Services;

public static class EncryptionService
{
    public static string DeriveKeyBase64(string passwordHash, string vaultId)
    {
        var combined = Encoding.UTF8.GetBytes(passwordHash + vaultId.Replace("-", ""));
        return Convert.ToBase64String(SHA256.HashData(combined));
    }
}
