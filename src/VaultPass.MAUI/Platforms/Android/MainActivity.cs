// Android entry point — the Activity that Android launches when the app starts.
// MauiAppCompatActivity is a base class provided by MAUI that wires the Android
// Activity lifecycle (onCreate, onResume, etc.) into the MAUI lifecycle.
// We don't need to override anything here; the base class handles it all.
using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.Fingerprint;

namespace VaultPass.MAUI;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                           ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CrossFingerprint.SetCurrentActivityResolver(() => this);
    }
}
