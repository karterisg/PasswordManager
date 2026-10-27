using Microsoft.Extensions.Logging;
using VaultPass.MAUI.Services;

namespace VaultPass.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            // maui tells which class is the entry point for the app 
            .UseMauiApp<App>()
            //any cystom fonts we want to use in the app we can add them here css xaml
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });


        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        // In debug mode, enable browser-style DevTools so we can inspect
        // the HTML/CSS rendered inside the BlazorWebView
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Register HttpClient pointed at the API.
        // The base address differs per platform — see AppConfig.cs.
        builder.Services.AddScoped(_ =>
        {
#if ANDROID && DEBUG
            // Android emulator doesn't trust the ASP.NET Core dev certificate.
            // Skip SSL validation in debug builds only.
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            return new HttpClient(handler) { BaseAddress = new Uri(AppConfig.ApiBaseUrl) };
#else
            return new HttpClient { BaseAddress = new Uri(AppConfig.ApiBaseUrl) };
#endif
        });

        // Dependency injection — same pattern as the Web project.
        // The key difference: SecureStorageService replaces LocalStorageService
        // because mobile apps store sensitive data in the OS keychain, not localStorage.
        builder.Services.AddScoped<SecureStorageService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<VaultItemService>();

        return builder.Build();
    }
}
