# MSAL MAUI Web Login

In a .NET MAUI “web” (Blazor) scenario you really have two flavours of MSAL-based login:

1. **SPA-style (MSAL.js) in a Blazor WebAssembly app**
2. **Native-style (Microsoft.Identity.Client) in a Blazor Hybrid app**

---

## 1. Blazor WebAssembly + MSAL.js

Here you’re running in the browser just like any other JavaScript SPA.  MSAL.js gives you two interaction modes:

* **Redirect** (the default):

  ```csharp
  builder.Services.AddMsalAuthentication(options =>
  {
      builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
      options.ProviderOptions.LoginMode = "redirect";
  });
  ```

  * **What the user experiences:** clicking “Log in” navigates the **same tab** to the Azure AD login page, then when they finish they’re **redirected back** to your app in that same tab.

* **Popup**:

  ```csharp
  builder.Services.AddMsalAuthentication(options =>
  {
      builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
      options.ProviderOptions.LoginMode = "popup";
  });
  ```

  * **What the user experiences:** clicking “Log in” spawns a **small popup window** (not a full browser tab) with the identity provider UI.  When they complete, the popup closes and your main app window is already authenticated.

> **Note:** MSAL.js does *not* by default open a full new browser tab; its “popup” is a controlled window.  If popups are blocked you’ll need to fall back to the redirect flow.

---

## 2. Blazor Hybrid (MAUI + WebView) with Microsoft.Identity.Client

When you host your Blazor UI inside a MAUI app you typically use the **public client** model in the native layer:

```csharp
// in MauiProgram.cs
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>().GetSection("AzureAd").Get<MsalClientConfiguration>();
    return PublicClientApplicationBuilder
              .Create(config.ClientId)
              .WithRedirectUri(config.RedirectUri)       // e.g. "msal{ClientId}://auth"
              .WithIosKeychainSecurityGroup(config.KeychainGroup)  // iOS only
              .Build();
});
```

Then in your C# login code you do:

```csharp
var app = sp.GetRequiredService<IPublicClientApplication>();
var result = await app.AcquireTokenInteractive(scopes)
                      .WithUseEmbeddedWebView(false)   // or true if you want the embedded WebView
                      .ExecuteAsync();
```

* **Embedded WebView** (`true`):

  * Login happens **inside your app**, in an embedded browser control.
* **System Browser** (`false`, the recommended approach):

  * Launches the **platform’s default browser** (outside your app), the user signs in there, and then Azure AD deep-links back into your MAUI app via the registered URI scheme.

> From the user’s POV they briefly switch to the system browser (or see an in-app WebView), complete auth, and then return to your app fully authenticated.

---

### So which “tab” or window?

* **Pure WebAssembly (MSAL.js):**

  * **Redirect flow:** same tab
  * **Popup flow:** popup window

* **Hybrid Native (Microsoft.Identity.Client):**

  * **Embedded WebView:** in-app browser view
  * **System Browser:** separate browser window/tab and then a deep link back

---

#### Best practice recommendations

* **Blazor WASM SPAs:** use **redirect** if you want the simplest flow (same-tab), or **popup** if you want to keep your app UI visible throughout.
* **MAUI Hybrid:** use **system browser** (`WithUseEmbeddedWebView(false)`) for the most secure and up-to-date UX; it will open the user’s browser and then return them to your app via your custom scheme.

---

**Example: Blazor WASM `Program.cs` snippet**

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.LoginMode = "redirect";   // or "popup"
    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://YOUR-API-SCOPE/.default");
});
await builder.Build().RunAsync();
```

That configuration will keep the user on the same tab when they sign in (or, if you swap to `"popup"`, will open a small window and then automatically close it when done).
