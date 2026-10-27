// Code-behind for MainPage.xaml.
// No logic is needed here — the page is just a shell that hosts the BlazorWebView.
// All navigation and UI logic lives in the Blazor components (Pages/*.razor).
namespace VaultPass.MAUI;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        // Loads and wires up the XAML layout (the BlazorWebView)
        InitializeComponent();
    }
}
