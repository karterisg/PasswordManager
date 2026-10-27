// Android entry point — the Activity that Android launches when the app starts.
// MauiAppCompatActivity is a base class provided by MAUI that wires the Android
// Activity lifecycle (onCreate, onResume, etc.) into the MAUI lifecycle.
// We don't need to override anything here; the base class handles it all.
using Android.App;
using Android.Content.PM;

namespace VaultPass.MAUI;

[Activity(
    Theme = "@style/Maui.SplashTheme",   // Shows the splash screen while the app loads
    MainLauncher = true,                  // This is the first Activity Android opens
    // Tells Android not to recreate the Activity on these configuration changes —
    // MAUI handles them internally instead
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                           ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
