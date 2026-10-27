// This is the root Application class for the MAUI app.
// It pairs with App.xaml, which defines global resources (colors, styles, etc.).
//
// Every MAUI app has one Application instance. Our job here is to create the
// single Window and decide what page it shows first.
namespace VaultPass.MAUI;

public partial class App : Application
{
    public App()
    {
        // Loads the resource dictionary and any markup defined in App.xaml
        InitializeComponent();
    }

    // MAUI calls this when the app launches to get the Window it should display.
    // We return a Window containing MainPage, which hosts the BlazorWebView.
    // Note: setting MainPage directly in the constructor was deprecated in .NET 10.
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "VaultPass" };
    }
}
