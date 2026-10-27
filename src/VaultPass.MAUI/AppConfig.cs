// This file solves a practical networking problem:
// The API base URL must be different depending on where the app is running.
//
// On an Android Emulator, "localhost" refers to the emulator itself, not the PC.
// The special address 10.0.2.2 is what the Android Emulator maps to the host PC's localhost.
//
// On Windows or iOS Simulator the app runs on the same machine as the API,
// so plain "localhost" works fine.
namespace VaultPass.MAUI;

public static class AppConfig
{
#if ANDROID
    // Android Emulator: 10.0.2.2 resolves to the host PC's localhost.
    // If testing on a physical device, replace with the PC's LAN IP (e.g. 192.168.1.100).
    public const string ApiBaseUrl = "http://10.0.2.2:7012/";
#else
    // Windows / iOS Simulator: app and API are on the same machine.
    public const string ApiBaseUrl = "http://localhost:7012/";
#endif
}
