using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VaultPass.Web;
using VaultPass.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//http client configuration for api calls
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5243/") //api url
});

// Service registration
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VaultItemService>();
builder.Services.AddScoped<LocalStorageService>();

await builder.Build().RunAsync();