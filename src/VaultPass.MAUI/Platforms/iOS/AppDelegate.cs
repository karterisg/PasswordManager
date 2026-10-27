// iOS/macOS entry point — the AppDelegate is the iOS equivalent of Android's Application class.
// MauiUIApplicationDelegate hooks into the UIApplication lifecycle and bootstraps MAUI.
// We just tell it how to build the app via our shared MauiProgram.
using Foundation;

namespace VaultPass.MAUI;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
