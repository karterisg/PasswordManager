// Mac Catalyst entry point — same pattern as iOS because Mac Catalyst is
// essentially iOS running on macOS. Same AppDelegate, same lifecycle.
using Foundation;

namespace VaultPass.MAUI;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
