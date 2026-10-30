using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace VaultPass.MAUI.Services;

public class BiometricService
{
    public async Task<bool> IsAvailableAsync()
    {
        try { return await CrossFingerprint.Current.IsAvailableAsync(); }
        catch { return false; }
    }

    public async Task<bool> AuthenticateAsync()
    {
        try
        {
            var request = new AuthenticationRequestConfiguration("VaultPass", "Unlock your vault");
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);
            return result.Authenticated;
        }
        catch { return false; }
    }
}
