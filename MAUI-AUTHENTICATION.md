
# Samples

https://learn.microsoft.com/en-gb/entra/identity-platform/quickstart-mobile-app-sign-in?toc=%2Fentra%2Fexternal-id%2Ftoc.json&bc=%2Fentra%2Fexternal-id%2Fbreadcrumb%2Ftoc.json&pivots=external&tabs=android-workforce%2Candroid-netmaui-external



# Authentication

I have a .NET MAUI Blazor Hybrid application. This application provides Android, IOS, Windows and Web interfaces. 
I want users to be able to signup and to authenticate using Microsft Entra External ID. They will be using their proviate emails as their user ID.
How would I approach this in the MAUI app.


Here’s a high-level approach to wire up Microsoft Entra External ID (the converged B2C CIAM) in your .NET MAUI Blazor Hybrid app so your users can sign-up/sign-in with their private emails:

---

## 1. Provision & configure your Entra External ID tenant

1. **Create an External ID (B2C) tenant** in the Entra admin center (or via the Visual Studio Code Entra Extension).
2. **Define a Sign-up / Sign-in user flow** (or custom policy) that collects email + password and any extra attributes you need.
3. (Optional) Add external IdPs (Google, Facebook, etc.) if you ever want social logins ([Microsoft Learn][1]).

---

## 2. Register your MAUI app in Azure

1. **App Registration → New registration**

   * **Name**: e.g. `Contoso.MauiB2C`.
   * **Supported account types**: “Accounts in this organizational directory only” (your External ID tenant).
2. **Authentication → Add a platform → Public client (mobile & desktop)**

   * **Redirect URI**:

     ```
     msal{ClientID}://auth
     ```

     (replace `{ClientID}` with your App’s Client ID) ([Microsoft Learn][2]).
3. **API Permissions** → Add Microsoft Graph delegated scopes: `openid`, `offline_access` → **Grant admin consent**.

---

## 3. Configure MAUI project for MSAL

### a) Install the MSAL.NET package

```shell
dotnet add package Microsoft.Identity.Client
```

### b) Create an MSAL “helper” singleton

In `/Services/MSALClientHelper.cs`:

```csharp
public static class MSALClientHelper
{
    // Initialize once at startup
    public static IPublicClientApplication PCA = PublicClientApplicationBuilder
        .Create("YOUR-CLIENT-ID")
        .WithB2CAuthority("https://<your-tenant>.b2clogin.com/tfp/<your-tenant>.onmicrosoft.com/B2C_1_SignUpSignIn")
        .WithRedirectUri($"msal{"YOUR-CLIENT-ID"}://auth")
        .Build();

    // Scopes your app needs (e.g. openid + any API scopes)
    public static string[] Scopes = { "openid", "offline_access" };
}
```

You’ll want your MSALClientHelper live in the **shared** code—i.e. inside your Shared project—because it’s pure .NET and will be consumed by all platforms (Android, iOS, Windows, Web).

A common pattern is:

1. **Create a `Services` folder** in your Shared project (if you don’t already have one).
2. **Add `MSALClientHelper.cs`** in that folder.
3. Make sure that any platform-specific bits (like the redirect URI scheme and the `WithParentActivityOrWindow(...)` hook) are passed in from your platform 项目 via abstraction or static configuration, but the MSAL client builder itself (the `.Create(...).WithB2CAuthority(...).Build()`) stays in shared code.

```text
/MySolution
  /MyApp.Android
  /MyApp.iOS
  /MyApp.Windows
  /MyApp.Web
  /MyApp.Shared
      /Services
          MSALClientHelper.cs    ← put it here
      /ViewModels
      /Views
      ...
```

That way any Blazor page or ViewModel in your Shared project can just call `MSALClientHelper.PCA` without worrying about which platform it’s on.




### c) Hook up platform callbacks

#### Android

* **Platforms/Android/AndroidManifest.xml**: ensure `intent-filter` with your scheme (`msal{ClientID}`) is registered.
* **Platforms/Android/MsalActivity.cs** (or MainActivity override):

  ```csharp
  protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
  {
      base.OnActivityResult(requestCode, resultCode, data);
      AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
  }
  ```

#### iOS

* **Platforms/iOS/AppDelegate.cs**:

  ```csharp
  public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
  {
      if (AuthenticationContinuationHelper.IsBrokerResponse(url))
      {
          AuthenticationContinuationHelper.SetBrokerContinuationEventArgs(url);
          return true;
      }
      return AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
  }
  ```
* Enable Keychain access in your `Entitlements.plist` so that silent renewals work ([Microsoft Learn][2]).

---

## 4. Wire up sign-in / sign-out in your Blazor UI

Because you’re in a Hybrid (Blazor WebView) app, you’ll invoke MSAL from C# and then drive your Razor UI:

```razor
@inject NavigationManager Nav
@code {
    async Task SignInAsync()
    {
        try
        {
            // Try silent first
            var result = await MSALClientHelper.PCA
                .AcquireTokenSilent(MSALClientHelper.Scopes, MSALClientHelper.PCA.GetAccounts().FirstOrDefault())
                .ExecuteAsync();

            // proceed if silent succeeded...
        }
        catch (MsalUiRequiredException)
        {
            // Fallback to interactive
            var result = await MSALClientHelper.PCA
                .AcquireTokenInteractive(MSALClientHelper.Scopes)
                .WithParentActivityOrWindow(PlatformConfig.Instance.ParentWindow)
                .ExecuteAsync();
        }

        // After you have a valid ID token / access token:
        Nav.NavigateTo("fetchdata"); 
    }

    async Task SignOutAsync()
    {
        var accounts = await MSALClientHelper.PCA.GetAccountsAsync();
        foreach (var acct in accounts)
            await MSALClientHelper.PCA.RemoveAsync(acct);
        Nav.NavigateTo("signout-confirm");
    }
}
```

---

## 5. Test on each platform

* **Web variant**: you can still host a Blazor WebAssembly version side-by-side; use the built-in `AddMsalAuthentication` Blazor extension for that ([Microsoft Learn][3]).
* **Mobile/Desktop**: run on emulator/device to verify the native browser flow and silent token cache.

---

### References & Samples

* Complete MAUI sample (CIAM) by Microsoft: ms-identity-ciam-dotnet-tutorial (Sign-in MAUI) ([Microsoft Learn][2])
* “A .NET MAUI app using MSAL.NET to authenticate users with Azure AD for Customers” (CIAM sample) ([Microsoft Learn][1])
* ASP.NET Core Blazor Hybrid auth guidance ([Microsoft Learn][4])

This setup gives you a fully branded, secure sign-up/sign-in experience in your Blazor Hybrid UI across Android, iOS, Windows and Web, backed by Entra External ID’s B2C CIAM service.

[1]: https://learn.microsoft.com/en-us/samples/azure-samples/ms-identity-ciam-dotnet-tutorial/ms-identity-ciam-dotnet-tutorial-2-sign-in-maui/?utm_source=chatgpt.com "A .NET MAUI app using MSAL.NET to authenticate users with Azure ..."
[2]: https://learn.microsoft.com/en-us/samples/azure-samples/ms-identity-ciam-dotnet-tutorial/ms-identity-ciam-dotnet-tutorial-2-sign-in-maui/ "A .NET MAUI app using MSAL.NET to authenticate users with Azure AD for Customers - Code Samples | Microsoft Learn"
[3]: https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-azure-active-directory-b2c?view=aspnetcore-7.0&utm_source=chatgpt.com "Secure a hosted ASP.NET Core Blazor WebAssembly app with ..."
[4]: https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/security/?view=aspnetcore-9.0&utm_source=chatgpt.com "ASP.NET Core Blazor Hybrid authentication and authorization"



# Additional Information


## Hook up platform callbacks - you show Android and IoS but what about Windows and the Web? 

For the Windows (WinUI/Desktop) and the Web (Blazor WebAssembly) flavors of your Hybrid app, the “hook” looks a bit different:

---

### Windows (WinUI 3/.NET MAUI on Windows)

1. **Declare your URI scheme** in your `Package.appxmanifest` under the `<Extensions>` section. It should match the same custom scheme you registered (e.g. `msal{CLIENT_ID}`).

   ```xml
   <Extensions>
     <uap:Extension Category="windows.protocol">
       <uap:Protocol Name="msal{YOUR-CLIENT-ID}" />
     </uap:Extension>
   </Extensions>
   ```

2. **Override `OnActivated`** in your WinUI `App.xaml.cs` so MSAL can pick up the redirect response:

   ```csharp
   protected override void OnActivated(WindowActivatedEventArgs args)
   {
       base.OnActivated(args);

       // This hands off any Windows protocol activation (msal{…}://...) back to MSAL
       AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(args);
   }
   ```

3. **Ensure your MAUI Windows project has the same redirect URI** in Azure (it’ll look like `ms-app://{YourPackageFamilyName}/msal{YourClientId}` if you’re using the broker) and that it matches the scheme in code and manifest .

---

### Blazor WebAssembly (Web)

On the pure-Web side you don’t wire platform callbacks yourself; MSAL.js handles redirects in the browser. Instead you configure it in `Program.cs`:

```csharp
builder.Services.AddMsalAuthentication(options =>
{
    options.ProviderOptions.Authentication.Authority = 
        "https://<your-tenant>.b2clogin.com/tfp/<your-tenant>.onmicrosoft.com/B2C_1_SignUpSignIn";
    options.ProviderOptions.Authentication.ClientId = "<YOUR-CLIENT-ID>";
    options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
    // If you need additional scopes, add them here.
});
```

That extension will automatically register the redirect URIs you set up in Azure (e.g. `https://localhost:5001/authentication/login-callback`) and handle the browser’s `window.location` changes for you .

---

#### Summary

* **Windows**: Declare the `msal{ClientId}` protocol in your app manifest, override `OnActivated` in `App.xaml.cs` to call `AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(...)`.
* **Web**: Use `AddMsalAuthentication` in your Blazor WebAssembly host—no manual callback wiring needed.



# Entra App

Today
Explain the following text primarily based on the surrounding page content: Please add a reply url to your application to use the 'Run user flow' experience.
This message appears in the Microsoft Entra admin center, specifically in the context of configuring a user flow for customer authentication.

Explanation:
A user flow in Microsoft Entra External ID allows applications to guide users through sign-up and sign-in processes. To enable this, an application must be properly registered with a reply URL—this is the endpoint where authentication responses (such as access tokens or authorization codes) are sent after a user successfully completes the login process.

Why is the reply URL required?
Security & Redirect Handling: Without a reply URL, the system won't know where to send authentication responses, making the login process incomplete.

Application Association: The user flow needs to be linked to an application, and the reply URL ensures that users are returned to the correct place after authentication.

How to resolve this?
You'll need to add a reply URL to your registered application within Microsoft Entra. You can do this by:

Navigating to Microsoft Entra admin center.

Selecting your application.

Configuring the Authentication settings.

Adding a valid reply URL (typically, the application's redirect endpoint).

Since you're working with social media login integration in your MAUI application, ensure that the URL matches what your app is expecting for successful authentication.

Would you like detailed steps for adding the reply URL in Microsoft Entra? I can guide you through it.

What is a valid reply URL format?
A valid reply URL (also called a redirect URI) must follow these rules to ensure proper authentication in Microsoft Entra External ID:

Format Requirements:
Must be an HTTPS URL – Authentication responses require encryption to ensure security.

Example: https://yourapp.com/auth/callback

Invalid: http://yourapp.com/auth/callback (because it's not HTTPS)

Must exactly match the registered URL – The redirect URI used in your application must match what is configured in Microsoft Entra exactly.

If your app sends users to https://yourapp.com/auth/callback, that same URL must be registered.

Can include query parameters but must be consistent – While query parameters (?param=value) can exist, they must follow expected patterns and match registered formats.

Must be reachable by your application – The reply URL should be an endpoint your app is actively listening for authentication responses.

If using localhost (for testing), it must be formatted properly:

Example for testing: https://localhost:5001/signin-oidc

Avoid using IP addresses directly like https://127.0.0.1/auth/callback.

Steps to Configure in Microsoft Entra:
Since you're working with social media login integration in your MAUI application, follow these steps:

Open Microsoft Entra admin center.

Go to App registrations and select your app (Tingler Application).

Navigate to Authentication settings.

Under Platform configurations, add a new redirect URI that matches your app’s authentication flow.

Save changes.

Would you like help troubleshooting possible errors, such as redirect URI mismatches? I can guide you through common issues.

is msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth a valid redirect URL?
No, msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth is not a valid redirect URL for Microsoft Entra authentication.

Why is it invalid?
It lacks HTTPS – Microsoft Entra requires redirect URIs to use the HTTPS protocol for security reasons.

It follows a custom scheme (msal...://auth) – This format is typically used for mobile applications that use the MSAL (Microsoft Authentication Library), but it needs additional configuration.

It is missing a registered domain – Entra External ID expects reply URLs to direct authentication responses to a known and registered endpoint.

Possible Solution:
If you're integrating authentication for your MAUI application, and need to use a custom scheme for mobile deep linking, consider:

Registering the custom URI scheme in your mobile app settings.

Using a valid HTTPS URL for web-based authentication, such as:

https://yourapp.com/auth/callback

https://localhost:5001/signin-oidc (for testing)

Would you like help configuring this properly for MSAL in your MAUI app? I can guide you through it.

Yes
Since you're configuring authentication for your MAUI application with Microsoft Entra External ID, here’s how to properly handle redirect URIs—including custom schemes like MSAL.

1. Configuring Redirect URIs for MAUI Apps
In mobile applications, custom URI schemes (like msal{app-id}://auth) are used for MSAL-based authentication flows. Since this differs from web authentication, you must:

Register the custom scheme in your app (Android & iOS).

Configure the matching redirect URI in Microsoft Entra.

2. Setting Up the Redirect URI in Microsoft Entra
To ensure proper authentication, follow these steps:

Open Microsoft Entra admin center.

Navigate to App registrations → Select Tingler Application.

Click Authentication.

Under Redirect URIs, add:

msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth

AND an HTTPS-based URI for web authentication (if applicable).

Save changes.

3. Configuring the URI in Your MAUI App
Your mobile application must handle this custom scheme:

Android: Add an intent filter for the redirect URI in AndroidManifest.xml.

iOS: Configure CFBundleURLTypes in Info.plist.

4. Common Issues & Fixes
If authentication fails, ensure that the redirect URI exactly matches what’s in Microsoft Entra.

If using social media login, confirm that the provider supports custom URI schemes.

Would you like help troubleshooting authentication errors or configuring these settings in MAUI?

There is no ability to add Redirect URIs in the Tingler Application -> Authentication screen
It looks like the Authentication screen for your Tingler Application in Microsoft Entra doesn't provide an option to add Redirect URIs directly. This is likely because the application does not yet have a platform configuration.

How to Fix This
Since Redirect URIs are tied to platform configurations, follow these steps:

Go to the Authentication screen in Microsoft Entra admin center.

Look for the section Platform configurations.

Click Add a platform.

Choose the appropriate platform for your app:

Web (for browser-based authentication)

Single-page application (for JavaScript-based apps)

Mobile and desktop applications (for MAUI apps)

Once the platform is added, you should see an option to add Redirect URIs.

Why is this necessary?
Microsoft Entra requires applications to specify their platform type before allowing Redirect URI configuration. Since you're working with MAUI and social media login integration, selecting the correct platform ensures authentication flows work properly.

Would you like help selecting the right platform for your app? I can guide you through it.

what is the problen now. It says "Need admin approval" when I log in with a local account
The "Need admin approval" message appears because your Tingler Application requires admin consent to access certain resources in Microsoft Entra External ID. This happens when:

Your tenant is configured to require admin consent for certain permissions.

The application is requesting permissions that require admin approval (such as User.Read.All or Directory.Read.All).

User consent settings prevent non-admin users from granting permissions.

How to Fix This
Option 1: Grant Admin Consent Manually
Sign in to Microsoft Entra admin center as a Global Administrator.

Navigate to Enterprise applications → Select Tingler Application.

Go to Permissions → Click Grant admin consent for the required permissions.

Option 2: Adjust User Consent Settings
If you want non-admin users to log in without requiring approval:

Go to Microsoft Entra admin center.

Navigate to Identity → Users → User settings.

Under User consent settings, allow users to consent to verified applications.

Option 3: Configure Admin Consent Workflow
If you don’t have admin access, you can request approval:

Enable Admin Consent Workflow in Microsoft Entra.

Users can submit a request for approval, and designated reviewers will grant access.

Would you like help troubleshooting specific permissions for your MAUI application? I can guide you through the exact settings needed.