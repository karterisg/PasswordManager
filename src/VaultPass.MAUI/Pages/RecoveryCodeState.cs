namespace VaultPass.MAUI.Pages;

// Transient state to pass recovery code from PinSetup to RecoveryShow
// without query strings (BlazorWebView NavigationManager doesn't support them reliably)
public static class RecoveryCodeState
{
    public static string? PendingCode { get; set; }
}
