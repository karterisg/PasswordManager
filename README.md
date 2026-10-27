# VaultPass

VaultPass is a local-first password manager I built from scratch, designed to run entirely on your own machine with no cloud dependency. The idea was simple: keep your credentials encrypted on your own hardware, access them from a browser extension, a web app, or your phone, and never hand them to a third-party service. Everything (the API, the database, the clients) runs on infrastructure you control.

## How it's built

### The core: Clean Architecture

Before writing a single endpoint or UI component, I structured the backend around Clean Architecture. This was a deliberate choice: I wanted the business logic to be completely independent of frameworks, databases, and HTTP. The result is four separate .NET projects that each have a clear responsibility and a strict dependency direction.

**VaultPass.Domain** is the innermost layer and has zero external dependencies. It contains the entities (User, Vault, VaultItem, Team), the domain exceptions, and the enums. Every entity uses a private constructor and exposes factory methods like `User.Create(...)` and `VaultItem.Create(...)` to enforce invariants at the domain level. You simply cannot construct an invalid object. The master password, for example, is never stored in plain text anywhere in the domain model; the entity only ever accepts a hash.

**VaultPass.Application** sits on top of the domain and defines the use cases. I used the MediatR CQRS pattern throughout, so every operation is either a Command (writes) or a Query (reads), handled by a dedicated handler class. This layer also defines the repository interfaces that the infrastructure must implement, which means the application layer never imports Entity Framework or any database concern. Alongside the handlers, there are three MediatR pipeline behaviors: one for validation (FluentValidation runs before every handler), one for logging (Serilog logs every command and its result), and one for caching.

**VaultPass.Infrastructure** implements everything the application layer declared. It contains the EF Core DbContext, the repository implementations, the JWT service, and the migrations. The database is SQL Server LocalDB for development, which requires no installation beyond the .NET SDK.

**VaultPass.API** is the ASP.NET Core host. It wires everything together, configures the DI container, sets up JWT Bearer authentication, CORS policies for the different clients, and runs as a background task via `UseWindowsService()`.

### The API

The API exposes two main controller groups: authentication (`/api/auth`) and vault items (`/api/vaultitems`). Login takes an email and a SHA-256 hash of the master password, so the raw password never travels over the wire. On success it returns a JWT that the clients use for all subsequent requests. The JWT payload includes the user's vault ID, which the clients extract client-side to scope their requests.

The API is published as a self-contained executable and registered as a Task Scheduler entry that starts automatically when you log into Windows. It runs on `http://localhost:5243` and has no HTTPS requirement in production mode, since it only ever listens on localhost.

### The Web App: Blazor WebAssembly

The browser-based client is a Blazor WebAssembly app (`VaultPass.Web`). It runs entirely in the browser and after the initial load there is no server rendering involved. The app talks to the API over HTTP, stores the JWT in the browser's localStorage, and handles the full login and vault management flow.

The reason I chose Blazor WASM here rather than a JavaScript framework is that the entire codebase is already C#, so sharing types and patterns across the API and the web client comes naturally. The HttpClient is configured at startup with the API base URL, and the AuthService handles login, token storage, and logout.

To run the web app locally, you start it with `dotnet run` and it comes up on `http://localhost:5002`. No build step, no npm, no bundler configuration.

### The Mobile App: MAUI Blazor Hybrid

For iOS and Android I used .NET MAUI with Blazor Hybrid (`VaultPass.MAUI`). This is a native app shell that hosts a Blazor WebView inside it, which means the UI is the same Blazor components used in the web app, running natively in a mobile WebView rather than a browser tab.

The interesting part here was networking. On an Android emulator, `localhost` refers to the emulator itself, not the host machine where the API is running. The special address `10.0.2.2` is what the Android emulator maps to the host PC's loopback interface, so the MAUI app uses that address on Android and plain `localhost` everywhere else. This is handled in `AppConfig.cs` with a compiler directive:

```csharp
#if ANDROID
    public const string ApiBaseUrl = "http://10.0.2.2:5243/";
#else
    public const string ApiBaseUrl = "http://localhost:5243/";
#endif
```

Android 9 and above block unencrypted HTTP traffic by default. Since the API runs on HTTP locally, I had to explicitly opt in to cleartext traffic in `AndroidManifest.xml` with `android:usesCleartextTraffic="true"`. Without this, the app would start, the WebView would load, but every API call would fail silently and Blazor would display its generic unhandled error screen.

For physical Android or iOS devices on the same network, the `ApiBaseUrl` should be replaced with the host machine's LAN IP (e.g. `192.168.1.x:5243`).

### The Chrome Extension

The extension is probably the most interesting piece of the project. It is a Manifest V3 Chrome extension consisting of three files: a background service worker, a content script, and a popup.

**The background service worker** (`background.js`) is the brain of the extension. It handles all communication with the API and manages the session token in `chrome.storage.local`. The reason the API calls live in the background worker rather than the popup is that service workers persist across popup opens and closes, so the token stays in storage and the worker is always there to use it. The popup and content script communicate with the worker via `chrome.runtime.sendMessage`, sending typed messages like `LOGIN`, `GET_ITEMS`, `GET_MATCHING`, and `SAVE_ITEM`.

**Encryption** is handled entirely in the browser using the Web Crypto API with AES-GCM. When saving a credential, the extension derives an AES-256 key from the user's password hash and their vault ID using SHA-256, then encrypts the username and password together as a JSON object. The IV is randomly generated for each encryption and prepended to the ciphertext before base64 encoding. The API stores this opaque blob and never sees the plaintext. It couldn't decrypt it even if it wanted to, because the key is derived from the master password that only the user knows.

**The content script** (`content.js`) runs on every page after it loads. It checks with the background worker whether there are any saved credentials matching the current site's origin. If there are, it injects a small autofill banner at the top of the page offering to fill the username and password fields. The matching is done by comparing the saved `websiteUrl` to `window.location.origin`, so `https://github.com` matches any GitHub URL regardless of path. The content script also handles the case where older saved entries store the password as a plain string rather than a JSON object with username and password, keeping backwards compatibility with entries created before the joint encryption format was introduced.

**The popup** (`popup.html` + `popup.js`) is what you see when you click the extension icon. It shows a login form if no token is present, or the vault list if you are already authenticated. Each vault item has a Fill button that sends the credentials to the content script on the current tab. The popup also lets you save a new credential for the current site directly from the extension, without opening the web app.

## Running everything

### Windows

The API background task starts automatically at every login via Task Scheduler. The entry is named `VaultPassAPI` and runs the published executable at `D:\desktop\VaultPass\publish\VaultPass.API\VaultPass.API.exe`.

For the web app, run `dotnet run --project src/VaultPass.Web` and open `http://localhost:5002` in a browser. For development with hot reload and the Scalar API explorer, run the API from Visual Studio or with `dotnet run --project src/VaultPass.API`. The development launch settings bring it up on `https://localhost:7012` with the Scalar UI available at `/scalar/v1`.

The Chrome extension is loaded unpacked from the `src/VaultPass.Extension` folder via `chrome://extensions` with Developer Mode enabled.

### Android

The app is distributed as a Release APK built with `dotnet publish src/VaultPass.MAUI/VaultPass.MAUI.csproj -f net10.0-android -c Release` and installed with `adb install`. Make sure the API is running before opening the app, and that the emulator or device can reach port 5243 on the host machine.

For emulator testing the address `10.0.2.2:5243` works out of the box. For a physical device on the same WiFi network, update `AppConfig.cs` to use the host machine's LAN IP before building.

### iOS

The MAUI app targets iOS as well. Run it from Visual Studio for Mac or Windows with a connected device or the iOS Simulator. The `AppConfig.cs` non-Android path resolves to `http://localhost:5243`, which works correctly on the iOS Simulator since it shares the host machine's network stack. For a physical iOS device, the same LAN IP approach as Android applies.

### macOS

The web app and the API both run cross-platform with no changes. Instead of Task Scheduler, run the API with `dotnet run --project src/VaultPass.API` and optionally set up a launchd agent to keep it running in the background.

## Security model

The master password never leaves the device in plaintext. The client hashes it with SHA-256 before sending it to the API for authentication. The API stores only that hash, which it re-hashes internally before persisting.

Vault items are encrypted client-side before being sent to the API. The encryption key is derived from the password hash and the vault ID, two values that are never stored together anywhere, so the server-side database contains only ciphertext. If someone extracted the database, they would have no way to decrypt the contents without the master password.

The JWT secret is defined in `appsettings.json`. In a real deployment this should be rotated and stored in an environment variable or a secrets manager, not committed to the repository.
