// Windows-specific entry point for the MAUI app.
// On Windows, MAUI runs on top of WinUI 3 (Microsoft.UI.Xaml).
// This class boots WinUI and hands control over to our cross-platform MauiProgram.
// Every platform has its own entry point file like this (see Platforms/Android, Platforms/iOS).
using Microsoft.UI.Xaml;

namespace VaultPass.MAUI.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        // Loads WinUI resources defined in the paired App.xaml
        this.InitializeComponent();
    }

    // MAUI calls this to get the configured MauiApp — we delegate to our shared MauiProgram
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
