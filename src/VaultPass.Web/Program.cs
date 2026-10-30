using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VaultPass.Web;
using VaultPass.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5243/";

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseAddress)
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VaultItemService>();
builder.Services.AddScoped<LocalStorageService>();

await builder.Build().RunAsync();
