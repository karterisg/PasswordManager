// The Android Application class — created before any Activity.
// MauiApplication is a base class provided by MAUI; we only need to tell it
// how to build our app by pointing to our shared MauiProgram.
using Android.App;
using Android.Runtime;

namespace VaultPass.MAUI;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
