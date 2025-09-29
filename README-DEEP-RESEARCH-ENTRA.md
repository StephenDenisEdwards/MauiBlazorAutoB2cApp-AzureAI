
<!--TOC-->
- [Implementing Entra ID (Azure AD B2C) in a .NET MAUI Blazor Hybrid App](#implementing-entra-id-azure-ad-b2c-in-a-.net-maui-blazor-hybrid-app)
  - [Overview and Architecture](#overview-and-architecture)
  - [Integrating Entra ID into the MAUI Blazor Client (MSAL.NET)](#integrating-entra-id-into-the-maui-blazor-client-msal.net)
  - [Securing the Web API with Microsoft.Identity.Web](#securing-the-web-api-with-microsoft.identity.web)
  - [Application Registration & B2C Configuration](#application-registration-b2c-configuration)
  - [Token Flow and Validation Details](#token-flow-and-validation-details)
  - [Token Storage, Refresh, and Silent Sign-In](#token-storage-refresh-and-silent-sign-in)
  - [Limitations and Challenges](#limitations-and-challenges)
  - [Pricing and Quotas Considerations](#pricing-and-quotas-considerations)
  - [Security Features and Benefits of Entra ID B2C](#security-features-and-benefits-of-entra-id-b2c)
  - [Best Practices Recap](#best-practices-recap)
- [GPT Request](#gpt-request)
- [Implementing Microsoft Entra ID (Azure AD B2C) Authentication in .NET MAUI Blazor Hybrid (Android & iOS)](#implementing-microsoft-entra-id-azure-ad-b2c-authentication-in-.net-maui-blazor-hybrid-android-ios)
  - [MSAL.NET Integration in .NET MAUI](#msal.net-integration-in-.net-maui)
  - [Android Integration Specifics](#android-integration-specifics)
  - [iOS Integration Specifics](#ios-integration-specifics)
  - [Redirect URI Configuration Requirements](#redirect-uri-configuration-requirements)
  - [Token Caching and Secure Storage](#token-caching-and-secure-storage)
  - [Silent Token Refresh and Login Persistence](#silent-token-refresh-and-login-persistence)
  - [System Browser vs Embedded WebView](#system-browser-vs-embedded-webview)
  - [Platform-Specific Behaviors and User Experience](#platform-specific-behaviors-and-user-experience)
  - [Best Practices and Troubleshooting Tips](#best-practices-and-troubleshooting-tips)
<!--/TOC-->


_
Great. I will compile a detailed and up-to-date analysis of using Microsoft Entra ID (Azure AD B2C) to implement security for a .NET MAUI Blazor Hybrid application with a web frontend and web API backend.
This will include architectural best practices, implementation guidance, limitations, and any potential pricing considerations. I’ll update you as soon as the findings are ready.


# Implementing Entra ID (Azure AD B2C) in a .NET MAUI Blazor Hybrid App

## Overview and Architecture

A **.NET MAUI Blazor Hybrid** application combines a native client (running .NET MAUI) with a Blazor web UI. In this scenario, we have a **public-facing client app** (the MAUI Blazor frontend) and a **separate Web API backend**. We will use **Microsoft Entra ID for Customers** (formerly *Azure AD B2C*) to handle user authentication and secure calls to the API. The high-level flow is:

* The MAUI app authenticates the user via Entra ID (Azure AD B2C) using **OpenID Connect**. This yields an **ID Token** (for user identity) and an **Access Token** (for API calls).
* The MAUI app calls the backend Web API, **including the Access Token in the HTTP Authorization header** (`Authorization: Bearer <token>`).
* The Web API **validates the JWT access token** (signature, issuer, audience/scopes) and authorizes the request. Only calls with a valid token (issued by our B2C tenant and containing required scopes) are served.

**Application Registrations:** A best practice is to create **two separate app registrations** in the Entra ID (B2C) tenant:

* **Client App Registration (App ID 1):** For the MAUI Blazor app (and any other front-end, e.g. SPA or web). This is a *public client* (no client secret). It enables user sign-in and requests tokens from B2C.
* **Web API App Registration (App ID 2):** For the protected API. This app registration represents the backend resource; you will **expose API scopes** on it and the client app will request those scopes. The API will accept tokens issued for these scopes.

Below is a diagram of the architecture and token flow between the Blazor client, Azure AD B2C, and the Web API:

&#x20;*Architecture for MAUI Blazor app with Azure AD B2C and protected Web API (App ID 1 represents the client; App ID 2 the API)*

As shown above, the **user flows** are managed by Azure AD B2C (Entra External ID). The client obtains an **ID token (App ID 1)** upon successful login, and an **Access token** for the Web API (App ID 2 scope)【34†】. The Web API trusts tokens issued by your B2C tenant (it checks the token’s **issuer (iss)** and **audience (aud)** to match the API’s registration).

## Integrating Entra ID into the MAUI Blazor Client (MSAL.NET)

On the client side, use the **Microsoft Authentication Library (MSAL.NET)** to authenticate users and acquire tokens. In a MAUI application, MSAL operates as a **native client** library:

* **MSAL Configuration:** Configure MSAL with your B2C **tenant and policy (user flow)**, the **Client ID** of the client app registration, and the appropriate **redirect URI**. In B2C, the redirect URI for mobile/desktop is typically of the form `msal{ClientId}://auth`. In the Azure portal, add a “Mobile and desktop application” platform for your app and specify this redirect URI.

* **User Flows:** Specify the B2C **authority** (endpoint) corresponding to your sign-in/sign-up user flow. For example:

  ```
  https://<tenant>.b2clogin.com/<tenant>.onmicrosoft.com/B2C_1_SignInSignUp/oauth2/v2.0/authorize
  ```

  This ensures MSAL directs users to the right **user flow** policy (such as a combined SignUp/SignIn flow). You can create the user flow in the Entra ID B2C portal and configure its name (e.g., `B2C_1_Sign_Up_Sign_In`) in your app settings.

* **Authentication UI:** MSAL will invoke the native system browser or an embedded web view to show the B2C hosted login page. (Using the system browser is recommended for better security and SSO.) The user can sign in with email/password or social identity providers configured in your B2C tenant (Google, Facebook, etc., as enabled).

* **Token Acquisition:** MSAL.NET uses the **OAuth 2.0 Authorization Code flow with PKCE** under the hood for public clients. The typical code pattern is:

  1. **Silent token attempt:** Call `AcquireTokenSilent` at startup to see if an existing token/refresh token is in the cache. This can re-authenticate the user without UI if a valid session exists.
  2. **Interactive sign-in:** If silent auth fails (e.g. first login or expired), call `AcquireTokenInteractive` to launch the B2C sign-in UI. You must specify the scopes your app needs (e.g. the API scope, plus `offline_access` for refresh tokens, and `openid` for ID token). In MAUI, use `.WithParentActivityOrWindow(...)` to attach the browser to your app’s window on mobile/desktop.

  Example (simplified):

  ```csharp
  var pca = PublicClientApplicationBuilder.Create(ClientId)
            .WithB2CAuthority(signInPolicyAuthority) // B2C authority with policy
            .WithRedirectUri("msal<clientid>://auth")
            .Build();
  string[] scopes = new[] { "https://<tenant>.onmicrosoft.com/<api-app-id>/API.Access", "offline_access", "openid" };
  AuthenticationResult result;
  try {
      // Try silent
      result = await pca.AcquireTokenSilent(scopes, pca.GetAccountsAsync().Result.FirstOrDefault())
                        .ExecuteAsync();
  } catch (MsalUiRequiredException) {
      // Interactive if needed
      result = await pca.AcquireTokenInteractive(scopes)
                        .WithParentActivityOrWindow(window)
                        .ExecuteAsync();
  }
  ```

* **Token Cache:** MSAL.NET automatically **caches tokens** on device. In MAUI, by default MSAL will securely serialize the cache for mobile platforms (e.g. in iOS Keychain and Android secure storage). **Important:** on iOS, you must enable Keychain access in your app’s entitlements and configure a Keychain Group (usually your Bundle ID) to allow the token to persist for silent logins. On Android, MSAL uses a redirect URI and an activity to handle the auth redirect; ensure you have the `<activity>` setup (with `android:scheme="msal<clientid>"`) as per MSAL Xamarin/MAUI guidelines. When properly configured, the cache will allow `AcquireTokenSilent` to succeed on app restart (as the sample notes, on Windows the token persisted so the user did not need to sign in again on app reopen).

* **Blazor Integration:** Since the UI is Blazor, you should propagate the logged-in state and potentially the ID token/claims to the Blazor web view. The **Blazor Hybrid** app does not automatically know the authentication state, so implement a custom `AuthenticationStateProvider` that queries the native MSAL login state. For example, after MSAL acquires a token, you can create a `ClaimsPrincipal` from the ID token claims and notify Blazor of the auth state. This enables Blazor components to use `<AuthorizeView>` and `[Authorize]` directives as usual.

* **Calling the API:** Use the Access Token from MSAL when making HTTP calls to the backend API. Typically, you’d store the token and attach it as a Bearer in the `Authorization` header of HttpClient requests. MSAL provides `AuthenticationResult.AccessToken` for this. Keep in mind the Access Token will expire (default \~60 minutes), so be prepared to catch 401 responses in the app, trigger a silent refresh (MSAL will use the refresh token if available), or re-interactive login if needed.

* **Logout:** Provide a way for users to log out. This typically means clearing the token cache in MSAL (e.g. `PublicClientApplication.RemoveAsync(account)` to remove the cached tokens for that user), and informing Blazor to clear any user state. You might also redirect the user to a B2C logout URL to end the B2C server session if you want (not strictly required, but without it a “logged out” user might still appear logged in if the web view retains a B2C cookie).

## Securing the Web API with Microsoft.Identity.Web

On the server side, use **Microsoft.Identity.Web** (which extends ASP.NET Core authentication) to protect the API with the tokens issued by your B2C tenant:

* **Startup Configuration:** In your ASP.NET Core Web API, add JWT Bearer authentication with B2C settings. Microsoft provides an extension for this. For example, in `Startup.cs` or program builder:

  ```csharp
  services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddMicrosoftIdentityWebApi(options => {
              Configuration.Bind("AzureAdB2C", options);
              options.TokenValidationParameters.NameClaimType = "name";
          },
          options => {
              Configuration.Bind("AzureAdB2C", options);
          });
  ```

  This call reads B2C settings from configuration (like *Instance*, *Domain*, *TenantId*, *ClientId*, *SignUpSignInPolicyId*, etc., under an `"AzureAdB2C"` section) and configures JWT validation. It sets up the expected issuer and audience, validates the token’s signature (using the B2C discovery metadata & signing keys), and maps the `"name"` claim as the NameIdentifier (since B2C tokens often use `name` or `emails` as the display name).

* **Require Auth on Controllers:** Use `[Authorize]` on your API controllers or endpoints to enforce that only authenticated calls are allowed. You can also require specific scopes/roles. For example, to ensure the token has the correct scope, you might configure a policy:

  ```csharp
  services.AddAuthorization(options => {
      options.AddPolicy("API.Access", policy => policy.RequireClaim("scp", "API.Access"));
  });
  ```

  And then apply `[Authorize(Policy="API.Access")]` on controllers. (In B2C, the scope claim is usually in `"scp"` for OAuth2 scopes.)

* **Token Validation:** The Microsoft.Identity.Web setup above automatically checks that the token’s **issuer** matches your tenant and policy, and that the **audience** (`aud`) matches the API’s App ID or custom API URI. It also validates that the token is not expired and is properly signed. Only tokens issued by your B2C tenant’s endpoint will be accepted. This ensures that *public tokens or tokens from other IDPs are rejected*, enforcing **authorization by Entra ID (B2C)**.

* **Protecting Data:** Once the framework validates the token, your controllers can access the user’s identity via `HttpContext.User` (which will contain the claims from the JWT). You can use this to personalize responses or do authorization checks (e.g., check the user’s object ID claim against a record, etc.).

* **CORS:** If your MAUI app and API are on different origins (not typically an issue for native apps calling an API), ensure you configure CORS appropriately. Native apps aren’t subject to browser CORS, but if you also have a web front-end (e.g., a web Blazor app or SPA) calling the API, you’ll need CORS rules to allow the front-end’s origin.

**Aside: Web Front-end Scenario** – If you plan to also have a web-based front-end (e.g., hosting the Blazor app as a web site), the integration is similar: you’d register a **Web app** (maybe reuse the same client app registration or a separate one) and use **Microsoft.Identity.Web** on the web app for user sign-in (OpenID Connect). The web app can then acquire tokens for the API on behalf of the signed-in user (using MSAL .NET or Microsoft.Identity.Web’s token acquisition service). In a server-rendered Blazor (Blazor Server) app, you’d use cookie authentication with B2C (OIDC flow) and then use **On-Behalf-Of** flow to call the API. In a Blazor WASM or other SPA, you’d use MSAL.js to get an access token and call the API directly. The core concepts (separate app registrations, B2C user flows, validating JWT in API) remain the same across these scenarios.

## Application Registration & B2C Configuration

Setting up the Entra ID (Azure AD B2C) side correctly is critical:

* **Create a B2C Tenant:** If not already done, create an External ID tenant (Azure AD B2C tenant) in the Entra admin center. Ensure you have a **user account** in that tenant for testing (you can use a personal email if allowed, or create a local account).

* **Register the Client Application:** In the B2C tenant, go to **App Registrations** and **New Registration**. Create an app for the MAUI client (e.g., "MyApp-Client"). Choose **Accounts in this B2C directory only**. After creation, note the **Application (Client) ID**. In **Authentication** settings:

  * Add a platform: choose **Mobile and desktop applications** (Public client). Enter the Redirect URI in the format `msal<client-id>://auth`.
  * Enable the option "Allow public client flows" if available (to allow the OAuth code flow without client secret).
  * (If you plan to also use this for a web SPA, you might add a Web platform with appropriate redirect URI as well, e.g. `https://localhost:5001/auth-callback` for development. For MAUI, the above is sufficient.)

* **Register the Web API Application:** Create another app registration for the API (e.g., "MyApp-API"). Again, accounts in this directory only. After creation, in **Expose an API**:

  * Set an Application ID URI (e.g., `api://<guid>` or a custom URI). Add a **Scope**, e.g., name it `API.Access` with an appropriate description (this represents a permission that clients can request to call the API).
  * No redirect URIs needed for the API. You *do not* mark it as public; it can be left as default (it will be a *resource* application).
  * If the API itself might call other downstream APIs, you could also register client secrets or certificates for it, but for a simple scenario that’s not required.

* **Define Permissions:** Back in the **Client app** registration, under **API Permissions**, add a permission for your API. Because this is within B2C, you’ll select *My APIs* and add the scope you defined (e.g., `API.Access` from "MyApp-API"). This lists the API permission the client will request. Since these are custom scopes in a B2C tenant, the **consent** behavior differs from Azure AD: B2C users are *not organizational users*, so they typically **see a consent screen** for permissions. To simplify UX, you as the admin can **pre-consent** to your API scopes on behalf of all users. In the Azure portal, after adding the permission, click **Grant admin consent** for the tenant. This way, when the user signs in, they won’t be prompted to consent to your API scope – the permission is already granted. (Note: For the built-in `openid` and `offline_access` permissions which appear under Microsoft Graph in B2C, you also should grant admin consent, since B2C local accounts cannot consent to those on their own.)

* **User Flows / Policies:** In Entra ID (B2C), create the user flow(s) for your app:

  * **Sign Up and Sign In**: This is the main policy for interactive login. Choose the attributes you want to collect (e.g., email, display name) and the identity providers (local accounts, social logins). Enable **“MFA”** in the user flow if you want to enforce two-factor auth (B2C can send an OTP via SMS or email as part of the flow).
  * **Password Reset** (optional): It’s common to set up a separate policy for self-service password reset. In your app, you can detect when B2C returns an error indicating the user clicked "Forgot Password" and then trigger this policy.
  * **Edit Profile** (optional): If you want users to edit profile info via B2C.
  * Note the names of these policies (e.g., `B2C_1_SignInUp`, `B2C_1_PasswordReset`) and configure them in your app (as authority endpoints or separate buttons that launch those flows).

* **Merging App Registrations (Optional):** It is possible to use a *single app registration* for both the client and API (by exposing scopes on the same app and also marking it as having public client redirect URIs). However, separating them (one for client, one for API) is cleaner for management and aligns with least privilege (you can scope the token’s `audience` to just the API). The official guidance uses two apps, so this discussion assumes that pattern.

## Token Flow and Validation Details

Once configuration is done, the runtime flow is:

1. **User Login:** The MAUI app opens the B2C **Sign-in/Sign-up page** (via MSAL). The user signs in (or signs up, if new). B2C authenticates the user (for local accounts, verifying password; or via social IdP OAuth for Google/Facebook, etc., depending on your setup).

2. **Token Issuance:** Upon success, Azure AD B2C issues tokens to the client:

   * An **ID Token (JWT)** which contains user identity claims (such as `sub` – the user’s unique ID in B2C, `name`, `emails`, etc.). The MAUI app can use this to display user info.
   * An **Access Token (JWT)** with the **audience** set to your API (App ID URI or client ID of API). It will include the **scope** (`scp`) claim listing the permissions (e.g., `API.Access`). It also contains the `iss` (issuer) which encodes your tenant and policy, for example: `iss: https://{tenant}.b2clogin.com/{tenantId}/v2.0/` (the `{tenantId}` or domain and the policy name are typically included). These tokens are signed by Azure AD (B2C) and have a validity period (e.g. 60 minutes for access token by default).

   Additionally, because the client requested `offline_access`, a **Refresh Token** is issued (long-lived opaque token that MSAL uses to get new access tokens after the old one expires). The refresh token in B2C by default is valid for 14 days, with a maximum lifetime of 90 days with continuous use. This means if the user runs the app at least once within 14 days, the token refresh will roll forward and can continue up to 90 days without an interactive login. (These defaults can be adjusted in user flow settings or custom policies, but 14/90 days is standard.) After this, the user would need to log in again.

3. **API Call with Token:** The MAUI app now calls the Web API, including the **Access Token** in the header. For example:

   ```
   GET /api/values HTTP/1.1  
   Host: api.myapp.com  
   Authorization: Bearer <eyJhbGciOiJSUzI1NiIsInR5cCI...>
   ```

   The token is just a base64 JWT string in the header.

4. **API Authentication/Authorization:** The ASP.NET Core API’s **JwtBearerMiddleware** (configured by Microsoft.Identity.Web) intercepts the request. It reads the token from the header, and checks:

   * **Signature:** Ensures the JWT is signed by the trusted issuer (it uses the OpenID Connect discovery for your B2C policy to get the signing keys and valid issuer info).
   * **Issuer and Audience:** Ensures the `iss` matches your tenant and policy, and `aud` matches the API’s registered client ID or API URI. If you used `.AddMicrosoftIdentityWebApi`, these are configured via the `"AzureAdB2C"` settings (for instance, *Domain* might be your tenant name, *TenantId* and *Instance* define the issuer, *ClientId* of API sets the audience).
   * **Expiry:** Checks `exp` claim to reject expired tokens.
   * **Scopes:** If you configured API scope requirements (e.g., using `[Authorize(Policy="API.Access")]`), it will ensure the `scp` (or `roles` claim if using app roles) contains the required value.

   If all checks out, the request is authenticated and the user identity is established (`HttpContext.User`). If anything fails (token missing, invalid, wrong audience, etc.), the middleware will reject the request with 401 Unauthorized.

5. **API Response:** Your controller logic executes and returns data if authorized. The client receives the data. From the client’s perspective, it just adds the token and gets a response – but if it were to call without a token or with an invalid token, it would get a 401/403 and know to re-authenticate.

This pattern ensures **end-to-end authorization**: the API is confident the call is on behalf of an authenticated user from *your app*, because the token is issued for your client & API by your trusted IdP (B2C). The client cannot forge the token (it’s signed by Azure) and the user cannot access the API without going through the B2C login.

## Token Storage, Refresh, and Silent Sign-In

One challenge in a mobile or hybrid app is managing tokens over time (especially when the app is closed or offline):

* **Secure Storage:** As mentioned, MSAL handles caching. On iOS, the keychain is used (which requires a provisioning profile and keychain entitlement to work even on simulator). On Android, MSAL uses Android’s secure storage. On Windows, MSAL might use DPAPI or encrypted files. **Do not manually store tokens in plaintext** – leverage MSAL’s cache. If you need to store additional state (like a flag that user is logged in), use Xamarin.Essentials/Maui Essentials `SecureStorage` or similar, but often checking MSAL’s cache for an account is enough to determine login state.

* **Silent Token Renewal:** When the access token expires (say after 60 minutes), MSAL will automatically use the **refresh token** to get a new one (when you call `AcquireTokenSilent` or when an API call fails with 401 and you retry through MSAL). This should happen without user interaction. However, if the refresh token itself expires or is revoked (e.g., user changed password, or 14 days of no use exceeded), MSAL’s silent call will throw an exception indicating interaction is required. At that point you must call `AcquireTokenInteractive` again (prompting the user to sign in). Designing your app to catch such scenarios (and maybe proactively checking token expiration on app resume) is important for a smooth UX.

* **“Keep me signed in”:** B2C has an option in user flows for "Keep me signed in". If enabled, after the first login, B2C sets a persistent cookie so that subsequent logins (even via web browser control) might not require credentials. MSAL also may leverage broker apps (like Microsoft Authenticator on mobile) for SSO. For example, on iOS, MSAL can detect if the Authenticator app or Company Portal is installed and use it as a broker; this can allow SSO between apps and can even enable conditional access compliance. In short, if KMSI is enabled, a returning user might tap the login button and get in without entering credentials (because B2C finds an active session).

* **Sharing Tokens with Blazor WebView:** In a hybrid app, the Blazor webview could in theory navigate to pages that require authentication (though typically, the Blazor content is running on .NET and doesn’t go out to external login pages). You usually won’t be sharing a cookie between the system browser (used by MSAL for login) and the WebView, since the WebView is an isolated context. Instead, after MSAL gets the token, you inform Blazor of the login and use the token directly in HttpClient calls (from .NET code). The Blazor UI itself should not attempt its own OAuth flow – it should defer to the native MSAL integration. This separation keeps the credentials handling in the native layer (which is more secure).

* **Multiple Accounts:** If your app supports multiple user accounts, MSAL can handle that too (the token cache can store multiple accounts distinguished by their IDs). You’d present a UI for the user to pick or switch accounts. In most consumer apps, however, a single account at a time is the norm.

* **Sign-Out:** Clearing the token cache via MSAL as described logs the user out in the app. However, note that Azure AD B2C might still have an **auth session cookie** for that user on the web. If the user immediately tries to log in again, B2C might not ask for credentials (if the session is still active) – it will SSO them. If you want a true logout (where the next login requires credentials), you can redirect to the B2C **end-session endpoint**: `https://<tenant>.b2clogin.com/<tenant>.onmicrosoft.com/<policy>/oauth2/v2.0/logout` (with optional post\_logout\_redirect\_uri). This clears the B2C cookie. In MAUI, you might do this by invoking a WebView or Browser to hit that URL. It’s a nice-to-have for a complete sign-out.

## Limitations and Challenges

While Azure AD B2C (Entra External ID) provides a powerful solution, be aware of some limitations or complexities in this scenario:

* **Platform-specific pitfalls:** As described, on iOS you **must have a valid Apple developer provisioning** (to use Keychain in simulator or device). This is a common stumbling block – without a proper provisioning profile, the token cache won’t persist on iOS. On Android, ensure the redirect URI and activity setup is correct or `AcquireTokenInteractive` may not return to your app.

* **Blazor Hybrid complexity:** In a pure Blazor WebAssembly app, authentication can be configured with a few lines and the library will handle redirecting to B2C. In Blazor Hybrid, it’s not so turnkey – you have to manage the auth flow in native code and then feed it into the Blazor UI. This requires writing the glue code (the custom `AuthenticationStateProvider`, etc.) and making sure the app reacts to login/logout events properly. It’s more complex, but the Microsoft docs provide guidance on this pattern.

* **User Experience considerations:** The B2C hosted pages are web-based. On a mobile device, by default MSAL will use the system browser which switches the user out of your app to a browser (and then back). This is secure and leverages SSO, but be prepared for that context switch. Alternatively, MSAL can use an embedded webview (by configuring `.WithUseEmbeddedWebView(true)`), which keeps it in-app – but then you lose SSO with other apps. Generally, the system browser is recommended.

* **Refresh token limitations:** As noted, B2C refresh tokens have a finite lifetime. If your users expect offline access beyond 14 days, you might need to handle the case where they open the app after a long period and get prompted to sign in again. There is **no unlimited offline access** – some reauthentication will eventually be required by design. (If you need longer-lived access without user interaction, one alternative is to use **MSAL device code flow** or ROPC in a pinch, but those aren’t ideal for interactive apps and B2C may not support device code flow for social accounts. It’s better to live with the security-driven limits and prompt the user when needed.)

* **B2C vs Entra External ID changes:** Microsoft has introduced *Entra External ID* as an evolution of Azure AD B2C. New features (and potentially different pricing tiers) are available there. Notably, **new B2C tenants after May 2025 use External ID**, and Azure AD B2C legacy tiers (P1/P2) are not available for new customers. This primarily affects licensing and features like Identity Protection, but the implementation for auth as described remains the same. Just be mindful that documentation might refer to *External ID* for new tenants, which is essentially the successor to B2C (but the technical integration – user flows, tokens, MSAL – continues to work as described).

* **Consent and permissions:** In a B2C tenant, the concept of user consent is limited. As we saw, for things like `offline_access` and any Graph API permissions, an admin must grant consent upfront. If your app needs to call Microsoft Graph on behalf of the user (e.g., read their Microsoft 365 data), B2C can issue tokens for Graph but you’ll have to treat it as a *separate API* with admin pre-consent (and the user must have an Azure AD identity – social accounts won’t have Microsoft Graph data). Many B2C scenarios avoid calling Graph at all, sticking to custom APIs, to keep things simpler.

* **Complex policies:** B2C offers **Custom Policies** (written in XML) for advanced scenarios (e.g., custom identity verification, integration with REST APIs for claims, more complex user journeys). These are powerful but complex. If your requirements are straightforward (just login and maybe social providers), the built-in user flows will suffice. If you need something like linking a user to an existing account by a custom ID, or doing step-up authentication in certain cases, custom policies might be needed – which is a steep learning curve. Plan accordingly (this is more of a development consideration than a runtime limitation).

* **Mature but distinct product:** Azure AD B2C is a separate service from the standard Azure AD used by enterprises. Certain features available in enterprise Azure AD (like device-based Conditional Access, or Intune app protection policies, etc.) might not apply to B2C users. For example, *Identity Protection* risk policies exist in B2C (in Premium P2 tier) to detect risky logins, but they are not as full-fledged as in enterprise Azure AD. You also can’t directly use Azure AD B2C for authenticating employees in the same tenant as your corporate Azure AD – it’s meant for external/consumer identities. Knowing these boundaries can save time – e.g., you wouldn’t try to register a B2C app and expect an Azure AD work account to log into it, unless that work account is linked via guest or social ID (which normally it wouldn’t be).

## Pricing and Quotas Considerations

One big advantage of Azure AD B2C is its **pricing model** for consumer identities: it’s based on **Monthly Active Users (MAUs)**, which can be much more cost-effective than per-user licensing.

* **Free Tier:** The first **50,000 MAUs per month are free**. This means if your app has up to 50k unique users logging in within a month, you pay nothing for the authentication service.

* **Paid Tier:** Beyond 50k, you pay on a per-user basis. As of 2025, the pricing for the new Entra External ID is about **\$0.0163 per MAU** after the free tier (this was roughly €0.0156, which is about 1.6 cents USD). This rate may vary by region and could be subject to change as early discount promotions end. (Legacy Azure AD B2C had two tiers: *Premium P1* around \$0.0033 per MAU and *P2* around \$0.0165 per MAU, with P2 including advanced features. The new External ID model is closer to the higher tier but includes most features.) Even at \~2 cents per active user, for consumer apps this is typically a good deal compared to building and securing your own identity system.

* **Other Costs:** B2C doesn’t charge per login or per token; it’s purely MAU-based. However, certain **add-ons** do have costs:

  * **SMS/Phone OTP:** If you enable phone-based sign-in or MFA via SMS, there is a cost per SMS sent. The rates depend on country, ranging from a fraction of a cent up to a few cents per message. For example, an SMS in a “high-cost” country might cost a few cents. These are charged as **authentication transactions**. Email OTP is generally free aside from the MAU counting.
  * **Azure AD B2C Tenants:** The B2C tenant itself is free to have. You just need an Azure subscription linked to it for billing. There’s no charge for creating multiple B2C tenants except that MAU counts and billing are per tenant.
  * **Scale/Throughput:** There are *service limits*, like requests per second per user flow, etc., but these are quite high and can be increased by contacting Microsoft if needed. Most apps won’t hit these limits. B2C is designed to scale to millions of users.

* **Premium Features:** In legacy B2C, “Premium P2” (paid MAUs) enabled **Identity Protection** (risky login detection, MFA enforcement on risk, etc.) and **Custom Policies**. In Entra External ID, these capabilities are included or available as add-ons. As of May 2025, Microsoft is converging these—so effectively if you’re paying the standard MAU fee, you have access to the full feature set (custom policies, identity protection, etc.) for those users. Always double-check current documentation for any feature-specific costs or required tiers.

* **Example calculation:** If your app has 100k users logging in each month, the first 50k are free, and the next 50k are charged. At \~\$0.016 per user, that’s \~\$800 per month. If you scale to 1 million MAUs, the cost \~(\$0.016 \* 950k) ≈ \$15,200 per month. This linear scaling makes costs predictable. Also, note that if one user logs in multiple times in a month, it still counts as one MAU. Conversely, if a user does not log in during a month, they don’t count for that month.

* **Quotas on Users:** There’s effectively no hard limit on number of user accounts in a B2C tenant (it can handle millions). The main thing to watch is MAU count. You can also delete or disable accounts to manage MAUs if needed (disabled users don’t count as MAU unless they log in).

* **Dev/Test**: Microsoft provides **Azure AD B2C emulator/test** options: you can use B2C in a free tier for testing easily since 50k is a high threshold. If you need isolation, you could even create a second B2C tenant for test, separate from prod (just remember to configure your apps to use the right one).

## Security Features and Benefits of Entra ID B2C

Using Microsoft Entra ID (Azure AD B2C) for a consumer-facing app provides a range of security benefits out-of-the-box, allowing you to leverage Microsoft’s investments in security:

* **Managed Identity Provider:** B2C handles **password storage and validation** for local accounts with high security (hashed passwords, account lockout for brute force, etc.). It also seamlessly integrates with **social identity providers** (so users can log in with Google, Facebook, Microsoft accounts, etc. without you handling those protocols yourself).

* **Multi-Factor Authentication (MFA):** B2C can enforce MFA in user flows. You can require MFA for all users or conditionally (for example, only for risky sign-ins if using Identity Protection). Built-in methods include SMS verification, Email OTP, and the ability to integrate TOTP apps. These can significantly increase account security for your application’s users without you building an MFA system from scratch.

* **Conditional Access & Identity Protection:** Azure AD B2C (particularly with the equivalent of “P2” features enabled) supports **risk-based conditional access policies** similar to enterprise Azure AD. This means it can detect atypical or suspicious login attempts (sign-in from new locations, unknown devices, or credentials seen in leaked databases). For example, if a login is deemed “high risk”, you could configure a policy to block it or require MFA. These identity protection features help **detect and respond to risky sign-ins automatically**, reducing fraud. (It’s worth noting that as of now, these risk detections in B2C apply to local accounts, not social accounts, since social login security is managed by the IdP).

* **Session Management & Secure Token Service:** B2C issues time-limited tokens (as discussed) and can revoke refresh tokens when needed (for example, if you disable a user account or if a user changes their password, B2C can invalidate refresh tokens). This ensures that access can be rescinded. The service also manages user sessions with cookies under the hood, with options like “Keep Me Signed In” to balance security and convenience.

* **Compliance and Standards:** Entra ID B2C is compliant with OAuth 2.0 and OpenID Connect standards, which have been scrutinized for security. The tokens are JWTs, signed with strong algorithms (usually RSA or ECC) and can be validated independently. Microsoft’s cloud regularly rotates signing keys and publishes metadata, so you don’t have to manage key rollover. B2C is also compliant with privacy and security standards (SOC, ISO, GDPR compliance for user data, etc.), so using it can help with your application’s compliance requirements.

* **Account Protection Features:** B2C includes features like **email verification** during sign-up (to verify the user owns the email), **password complexity policies**, and **account lockout** (to prevent brute-force attacks). You can configure custom password complexity (e.g., minimum length, characters) and B2C leverages Azure AD’s “banned password” list to stop users from choosing common passwords. There’s also built-in **self-service password reset** (which, when enabled, lets users reset their own password via email/SMS verification – reducing support burden and keeping recovery secure).

* **Monitoring and Logging:** All sign-ins and token requests are logged in Azure AD B2C. You can view **audit logs** and **sign-in logs** in the Azure portal, which show details like sign-in success/failure, IP addresses, reasons for failure, etc. This is crucial for security monitoring and troubleshooting. You can even stream these logs to an Azure Monitor, Splunk, or other SIEM for analysis. If an incident occurs, you have an audit trail of authentication events.

* **Future Enhancements:** Microsoft is actively developing Entra External ID (the successor to B2C). This will unify external user authentication and likely introduce more **secure features** like continuous access evaluation (re-evaluating active sessions when risk changes), better integration with **Verified IDs / verifiable credentials**, and more. By using B2C now, your app is in a good position to take advantage of these new features when they roll out.

In summary, offloading authentication to Microsoft Entra ID (Azure AD B2C) means you get a **holistically secure identity system** for your app: your team doesn’t touch passwords or implement crypto; you rely on an enterprise-grade system. Features like MFA, risk detection, and social login integration come almost for free. This significantly reduces the risk of data breaches and fraud for your app, compared to a home-grown auth system.

## Best Practices Recap

To tie everything together, here are some best practices for this architecture:

* **Use the official libraries** (MSAL for the client, Microsoft.Identity.Web for API) to reduce complexity and avoid errors. They handle a lot of the protocol details for you.
* **Keep tokens secure**: Let MSAL manage the cache. Do not expose tokens to the Blazor UI or anywhere unnecessary. Communicate authentication state via abstractions (e.g., don’t store the access token in a JavaScript accessible location in a web context).
* **Minimal scopes principle**: Only request the minimal scopes your app needs. For example, don’t request Microsoft Graph scopes if you don’t use Graph. For your own API, you might even start with a single scope like `API.Access`. You can always expand if needed.
* **Validate on the server**: The API should *always* validate tokens and not trust any client input beyond that. Use `[Authorize]` on all sensitive endpoints. If different levels of permission are needed, implement additional claims checks or use separate scopes.
* **Error handling and UX**: Handle MSAL exceptions gracefully. For instance, if the user cancels the sign-in, handle that event. If the token expires and can’t be refreshed silently, catch it and prompt login. Ensure your UI indicates when the user is not logged in or if an action requires login.
* **Testing**: Test the end-to-end flow in **all platforms** (Android, iOS, Windows) because there can be platform-specific nuances. For example, test that Android back button doesn’t break the auth flow, or that the iOS Keychain actually retains the token after app reinstall if that’s a requirement (Keychain can survive app reinstall unless keychain sharing is configured otherwise).
* **Monitor**: Once in production, monitor sign-in success/failure rates in the Azure portal. If you see a lot of errors (like a particular error code), it could indicate an issue in the config or an attack. Azure AD B2C has some built-in metrics and you can set up alerts (e.g., alert if sign-in failure rate spikes).

By following these practices, you create a robust solution where Microsoft Entra ID B2C provides a **unified identity layer** for your .NET MAUI Blazor app and its web API. This lets you focus on the app’s functionality while relying on a secure, scalable authentication system maintained by Microsoft.

**Sources:**

* Microsoft Learn – *Blazor Hybrid Authentication Guidance*
* Microsoft Learn – *Azure AD B2C Protecting a Web API* (JWT validation setup)
* Microsoft Learn – *.NET MAUI MSAL Authentication Sample* (token acquisition flow)
* Microsoft Learn – *App Registration for MAUI in B2C* (redirect URI setup)
* Microsoft Q\&A – *Token lifetimes in B2C* (refresh token 14-day/90-day)
* Stephan van Rooij – *Azure AD B2C vs Entra External ID (Pricing)*
* Microsoft Q\&A – *B2C Security Features (Conditional Access)*


# GPT Request 
I’ll now expand the analysis to specifically address .NET MAUI Blazor Hybrid app behavior on Android and iOS platforms. This will include MSAL.NET integration details such as redirect URI setup, token caching, system browser behavior, silent refresh reliability, and any platform-specific requirements or challenges. I’ll update you once that’s ready.


# Implementing Microsoft Entra ID (Azure AD B2C) Authentication in .NET MAUI Blazor Hybrid (Android & iOS)

&#x20;*Overview of a .NET MAUI (Blazor Hybrid) app acquiring an ID token interactively from Azure AD B2C (Microsoft Entra External ID).*

**Introduction:** A .NET MAUI Blazor Hybrid app is a native mobile app (for Android/iOS) that hosts a Blazor web UI. Implementing authentication against **Microsoft Entra ID (Azure AD B2C)** in such an app requires using the Microsoft Authentication Library (MSAL.NET) to handle OAuth2/OIDC flows. This involves proper **MSAL integration on each platform**, correct **redirect URI configurations**, and secure **token caching** to preserve login state. The app will use **MSAL.NET** (as a public client) to interactively acquire tokens from Azure AD B2C and then silently refresh them as needed. Below, we detail the key considerations and best practices for a production-ready implementation, including platform-specific setups (Android vs iOS), redirect URIs, token storage, silent renewal, web authentication UI options, and common UX behaviors (like back-navigation and biometric support).

## MSAL.NET Integration in .NET MAUI

Using MSAL.NET in a MAUI Blazor app follows the same principles as any native app using Azure AD B2C. You’ll register a Public Client Application in Azure AD B2C (Entra External ID) and use MSAL’s **PublicClientApplicationBuilder** to configure it in your MAUI app. The **authority** should be set to your B2C tenant’s authority (including the B2C sign-in/sign-up policy), and you must provide the **ClientId** of your app registration. In code, for example, you might do:

```csharp
// (Pseudo-code for illustration)
var pca = PublicClientApplicationBuilder.Create(clientId)
    .WithAuthority($"https://{tenantName}.b2clogin.com/tfp/{tenantName}.onmicrosoft.com/{policyName}")
    .WithRedirectUri($"msal{clientId}://auth")
    .WithParentActivityOrWindow(parentWindowOrActivity) // on Android
    .Build();
```

* **Platforms**: The above builder is cross-platform, but some specifics (like `.WithParentActivityOrWindow`) need platform context. For MAUI Blazor, you typically call this in platform-specific code or use dependency injection to provide a platform-specific parent window handle on each platform.

* **Scopes**: Ensure you request the appropriate scopes (at minimum, **`openid`** and **`offline_access`** for B2C) when calling `AcquireTokenInteractive`. `openid` gives you an ID token and `offline_access` is required to receive a refresh token for silent renewals.

Below we cover Android and iOS integration specifics in detail.

## Android Integration Specifics

**Redirect URI & Manifest Setup:** On Android, you must configure a **custom scheme redirect URI** that matches what is registered in Azure AD B2C. The common pattern (and default for MSAL) is `msal<CLIENT-ID>://auth`. In the Azure portal, add this under your app’s **Authentication** settings (as a public client redirect URI). The Android app must be set up to handle this custom URI scheme. Typically, this is done by declaring an activity with an intent-filter in the **AndroidManifest.xml**:

```xml
<activity android:name="microsoft.identity.client.BrowserTabActivity"
          android:configChanges="orientation|screenSize"
          android:exported="true">
  <intent-filter>
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />
    <data android:scheme="msal<your-client-id>" android:host="auth" />
  </intent-filter>
</activity>
```

This ensures that when the system browser or web-view redirects to `msal<client-id>://auth`, Android will launch your app to handle it. The `<client-id>` in the scheme **must exactly match** your GUID client ID (no braces or hyphens omitted, and note that the scheme is lowercase). In Azure AD B2C’s registration, the redirect URI should exactly match this scheme and host.

Alternatively, instead of manually editing the manifest, you can create a stub **MsalActivity** class in your Android platform project:

```csharp
[Activity(Exported = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
    DataScheme = "msal<your-client-id>",
    DataHost = "auth")]
public class MsalActivity : BrowserTabActivity
{
}
```

This accomplishes the same thing via attributes in code. In the official MSAL samples, a `Platforms/Android/MsalActivity.cs` is provided where you just plug in your client ID.

**Launching the Authentication UI:** MSAL on Android uses the system browser by default (via **Chrome Custom Tabs** or the default browser) for interactive sign-in. You need to provide a reference to the current Activity so MSAL can launch the browser. In MAUI, you can obtain the current `Activity` (for example via the **CurrentActivity** plugin or MAUI’s platform services) and supply it with `.WithParentActivityOrWindow(...)` when building the PublicClientApplication. This ensures that the browser UI is tied to your app’s activity for proper UX and that the result can be delivered back. If using the manifest/BrowserTabActivity approach as above, MSAL will handle returning the result via that activity, so you typically do not need to override `OnActivityResult` manually in MainActivity. (Under the hood, **BrowserTabActivity/MsalActivity** will catch the redirect and invoke MSAL’s continuation logic.)

> **Note:** In older implementations (Xamarin), one would override an Activity’s `OnActivityResult` and call `AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data)` to notify MSAL of the auth completion. When using the modern approach with the BrowserTabActivity, MSAL handles this automatically. Just ensure your `MsalActivity` (if used) is declared as above so it can intercept the callback.

**Back Navigation & Cancellation (Android):** If the user presses the **Back** button during the web authentication (in the browser tab), the flow will be canceled and control returns to your app. MSAL will typically throw a cancellation exception in this case (e.g. an `MsalClientException` indicating the user canceled). Your code should handle this gracefully – e.g., treat it as a login canceled scenario (perhaps no action or a message, rather than an error). MSAL cannot always distinguish a silent abandonment vs an explicit cancel, so it’s good practice to also implement a timeout for the interactive call. Microsoft recommends using a **CancellationToken** with a few minutes timeout for interactive flows, to avoid hanging if a user simply closes the browser or navigates away.

**Chrome Custom Tabs:** By default MSAL will try to use Chrome Custom Tabs for better performance and SSO. If a device doesn’t have a suitable custom tabs provider or you haven’t allowed querying for it, MSAL may log a message like “Browser with custom tabs package not available… Launching with alternate browser.” In Android 11+, you might need to add a `<queries>` element in your manifest (allowing the app to query for browsers) so MSAL can detect available browser apps. If you omit that, MSAL will still attempt to launch the default browser. Typically, this is handled for you, but it’s something to be aware of if you see issues launching the web UI on Android.

## iOS Integration Specifics

**Redirect URI & URL Scheme:** On iOS, you also use a custom scheme redirect URI (`msal<client-id>://auth`). In the Azure portal app registration, add this under Mobile platform redirect URIs (similar to Android). The iOS app must declare this URL scheme so the OS knows to route the redirect back to your app. In your iOS project’s **Info.plist**, add a URL type entry with the scheme `msal<client-id>` (and any identifier or role, typically role = Editor). For example:

```xml
<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleURLSchemes</key>
    <array>
      <string>msalxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</string>
    </array>
    <key>CFBundleURLName</key>
    <string>AuthRedirect</string>
  </dict>
</array>
```

This allows iOS to open your app when the `msal<client-id>://auth` redirect is invoked.

**App Delegate – Handling Redirect:** iOS requires your app to handle the inbound auth response via `AppDelegate.OpenUrl`. In MAUI (which uses a UIKit delegate under the hood), you need to override `OpenUrl` and call into MSAL’s continuation helper. The MSAL.NET library provides `AuthenticationContinuationHelper` for this. In your **`Platforms/iOS/AppDelegate.cs`**, implement something like:

```csharp
public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
{
    // If using broker (Authenticator):
    if (AuthenticationContinuationHelper.IsBrokerResponse(null))
    {
        // This ensures MSAL can process broker responses on a background thread
        _ = Task.Factory.StartNew(() => AuthenticationContinuationHelper.SetBrokerContinuationEventArgs(url));
        return true;
    }
    // For non-broker responses:
    else if (!AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url))
    {
        return false;
    }
    return true;
}
```

This code checks if the URL is from the broker and handles it, otherwise passes it to MSAL’s continuation handler. By returning true for handled URLs, you indicate to iOS that your app consumed the URL. This is crucial – without this, MSAL cannot complete the sign-in flow on iOS.

**Keychain and Token Storage (Entitlements):** By default, MSAL on iOS will use the iOS **Keychain** to securely store tokens. However, iOS requires you to explicitly enable Keychain sharing access for your app. In your Xcode project settings (exposed in Visual Studio via the Entitlements.plist), you must turn on **Keychain access groups**. Typically, you add an entry like `<keykeychain-access-groups</key> <array><string>$(AppIdentifierPrefix)$(CFBundleIdentifier)</string></array>` in *Entitlements.plist* (or simply enable the Keychain checkbox which auto-adds your app’s AppIdentifier Prefix + Bundle ID). Then, in the **iOS Bundle Signing** settings for the project, make sure this Entitlements.plist is included. Using the app’s bundle ID as the Keychain group is sufficient if you don’t need to share tokens with other apps. Also ensure you are using an Apple Developer provisioning profile that allows Keychain (the same Apple team ID must be in the AppIdentifierPrefix).

If Keychain entitlements are not configured, MSAL cannot persist tokens on iOS. You might see runtime errors about failing to save to Keychain or missing access groups. So this step is critical for **login persistence** on iOS.

**System Browser on iOS:** MSAL uses the **ASWebAuthenticationSession** by default (on iOS 12+), which presents a Safari-based secure browser UI to the user. This is considered a system browser and benefits from any existing Safari cookies for SSO. You don’t need to write any code to launch it – calling `AcquireTokenInteractive` will automatically launch this UI. (Under the covers, MSAL’s iOS binding chooses ASWebAuthenticationSession, or SFAuthenticationSession on iOS 11, etc., to handle the web flow.)

**Back Navigation & Cancellation (iOS):** On iOS, there is typically a **Cancel** button or a swipe-down gesture to dismiss the ASWebAuthenticationSession. If the user cancels, MSAL will invoke the OpenUrl handler with an error indicating cancellation (which the continuation helper handles). Your `AcquireTokenInteractive` task will throw an exception or return a result indicating the user canceled. Handle this as you would on Android (no credentials acquired). One nuance: iOS’s ASWebAuthenticationSession by default does **not share persistent cookies** with Safari (it uses an ephemeral session for security). Thus, if a user cancels and later tries again, they may have to re-enter credentials unless “Keep me signed in” was used. We discuss this more under silent refresh.

## Redirect URI Configuration Requirements

**Redirect URIs** are a vital part of the auth flow – they are the mechanism by which the MSAL library receives the auth tokens from the web IdP. As noted, the standard for mobile is to use a **custom scheme URI**. Using the same scheme on both Android and iOS (e.g., `msal<clientid>://auth`) is convenient, and MSAL will default to it if you configure nothing else. In Azure AD B2C’s registration, you should add this under **Mobile and Desktop applications** platform (the portal will prefix it with `msal` automatically if you choose that option).

* **Android:** The redirect URI must also be reflected in the manifest intent-filter (scheme and host). Android will match the scheme case-sensitively to decide if your app can handle the URI. If there’s a mismatch (even an uppercase vs lowercase difference), the redirect back to the app will fail. Also ensure `android:host="auth"` in the intent filter, as MSAL’s default expects the URI in the form `...://auth` (with “auth” as the static path). The intent filter `BrowserTabActivity` or `MsalActivity` must be marked `android:exported="true"` (as shown above) so the system can launch it.

* **iOS:** The redirect URI’s scheme must appear in the app’s Info.plist URL types, otherwise the OS won’t route the URL to your app. Again, ensure the scheme string is exactly `msal<clientId>` (all lowercase). The path “auth” after the scheme is handled in code (you don’t need to specify it in Info.plist, only the scheme matters for iOS). When the auth completes, Azure AD B2C will invoke `msal<clientid>://auth` which brings your app to foreground and calls your AppDelegate `OpenUrl`. If nothing happens on completion, double-check that the scheme is registered and that `OpenUrl` is implemented correctly.

* **No Loopback URIs:** Unlike some desktop apps, you will **not** use `http://localhost` redirects for mobile B2C apps. (Azure AD B2C does not support `http://localhost` for public clients anyway.) Always use the custom scheme method for mobile.

**Gotcha – B2C custom domains or multiple policies:** If your B2C tenant uses a custom domain for the b2clogin.com endpoint, or if you have multiple user flows (policies), ensure your MSAL authority and redirect URI configuration are consistent. Typically, the redirect URI stays the same for all policies in the app; you don’t need a different redirect for each user flow. The authority URL will include the policy (e.g., `.../oauth2/v2.0/authorize?p=<PolicyName>` or the newer `.../<PolicyName>/oauth2/v2.0/authorize` path), but that does not change the redirect URI. Just register the one `msal...://auth` and use it for all MSAL operations.

## Token Caching and Secure Storage

**Token Cache Basics:** MSAL.NET maintains an in-memory token cache for the user’s tokens (ID token, access token, refresh token). On **mobile platforms (Android/iOS)**, MSAL **by default** enables a secure persistent cache so that tokens survive app restarts. In other words, once the user signs in, their tokens (especially the refresh token) are saved securely on the device, and subsequent app launches can use the cache to obtain valid tokens silently. Microsoft documentation confirms that *“when writing mobile apps, caching is already pre-configured by MSAL.”* This uses the platform’s secure storage: on iOS the Keychain, and on Android a secure mechanism (historically, Xamarin.Android MSAL used Android’s AccountManager or encrypted SharedPreferences under the hood).

Because of this, **login persistence** is largely automatic – the user will stay signed in until the refresh token expires or is revoked. You do **not** need to manually serialize or encrypt the cache on mobile; MSAL handles it. (On desktops, one would need to use a helper library or serialize to disk, but on mobile MSAL leverages OS storage by default.)

**Android Secure Storage:** On Android, MSAL’s default cache leverages the AndroidKeyStore to encrypt tokens at rest (or uses AccountManager which is backed by AccountManager’s own mechanism). The details are abstracted away, but you can trust that the tokens are stored in a private area of the app’s data, and if possible, encryption is applied. One thing to remember is that if the user deletes the app, the secure storage is wiped and the tokens are lost – they’d have to sign in again (which is expected).

**iOS Secure Storage:** On iOS, as mentioned, MSAL stores tokens in the Keychain. The Keychain can persist data even if the app is uninstalled (unless keychain sharing groups are used across apps), but by default Keychain items remain available to the app upon reinstall (this is an Apple feature). If this is not desired, you may consider using the Keychain item attribute to tie it to the device (so it doesn’t survive uninstall), but that’s an advanced scenario. By default, MSAL likely uses an accessibility like after-first-unlock (so that tokens are available as soon as the device is unlocked after reboot). **Important:** Keychain items may sync to iCloud by default. If your tokens are particularly sensitive, you might want to disable iCloud backup for those items. This can be done by using a Keychain access group with `keychain-access-groups` that doesn’t have the iCloud entitlement, or by storing with the attribute `ThisDeviceOnly`. MSAL.NET doesn’t expose this in the public API; you’d have to customize the token cache serialization if needed.

**Multiple Accounts and Cache:** If your app allows multiple user accounts (B2C supports that in some cases), MSAL’s cache can store multiple accounts’ tokens. You can use `PublicClientApplication.GetAccountsAsync()` to retrieve accounts from the cache. In a B2C context, accounts are usually identified by the user’s objectId or email. Ensure you call cache-clearing (RemoveAsync as discussed in sign-out) if needed to prevent “silent” logins as the wrong user.

## Silent Token Refresh and Login Persistence

After a successful interactive login, MSAL receives an **access token** (usually valid for 1 hour) and a **refresh token** (valid for a longer period) for Azure AD B2C. The presence of the refresh token is what allows **silent token renewal** – the app can get a new access token without user interaction.

**Using AcquireTokenSilent:** MSAL provides `AcquireTokenSilentAsync(scopes, account)` which will first look in its cache for a usable access token. If the access token is expired or about to expire, it will use the refresh token (stored in cache) to silently obtain a new access token from Azure AD B2C, all behind the scenes. This silent call will succeed as long as the refresh token is still valid and not revoked. You should call `AcquireTokenSilent` (or simply call `AcquireTokenAsync` which internally uses the cache) whenever you need a token and want to avoid prompting the user. For example, when your Blazor webview needs to call a secure API, you can invoke a .NET MAUI service that calls `AcquireTokenSilent` to get an access token, and only if that fails, fall back to an interactive login.

**Refresh Token Lifetime & Expiration:** By default, Azure AD B2C issues refresh tokens with a long lifetime (the exact values can vary). Generally, **refresh tokens in Azure AD (non-SPA) have a 90-day default lifetime**, with sliding window renewal. This means if the user uses the app regularly, the 90-day period can continually slide forward, keeping them signed in indefinitely. However, if the refresh token hasn’t been used for the duration or certain conditions occur (password change, account revoke, etc.), it can expire or be revoked. Microsoft docs note: *“The default lifetime for refresh tokens is 24 hours for single page apps and 90 days for all other scenarios.”* – as a native mobile app, yours falls in the “other” category, so likely 90 days. B2C also allows custom token lifetimes via user flow settings, so your tenant might have specific values (e.g., some B2C policies might set refresh tokens to never expire until revoked).

**Silent Renewal Implementation:** In practice, MSAL handles most of this. Once the user is logged in, you can attempt silent calls at app startup or when needed. If `AcquireTokenSilent` throws an exception (like `MsalUiRequiredException`), that indicates silent renewal failed (possibly the refresh token expired or no account was in cache), and you should initiate an interactive login again. To make the user experience seamless, you might design the app to silently renew in the background and only prompt the user if absolutely necessary.

**Login Persistence:** Because the token cache is persisted on the device, a user who has signed in previously can often open the app days or weeks later and still have an active session via silent token refresh. The user will *not* see the login screen each time, which is the goal for good UX. However, note the **“Keep Me Signed In (KMSI)”** setting on B2C pages. B2C’s KMSI, if enabled in the user flow, causes the B2C service to issue a persistent browser cookie so that even if the refresh token expires, the next interactive login can silently authenticate the user via the web session. On mobile, the system browser control on iOS is privacy-focused: it **clears session cookies after the ASWebAuthenticationSession completes**. As Microsoft notes, *“iOS clears session cookies right away after login due to the use of a temporary browser. To make SSO work on iOS, KMSI must be enabled to utilize persistent cookies.”*. In simpler terms, if you want the user to remain signed in even beyond refresh token expiry, enable **KMSI** in your B2C policy – this sets a persistent cookie in the browser. Then, if a refresh token is ever invalid (e.g., after 90 days or user reinstalled app), the next `AcquireTokenInteractive` will find the user’s browser cookie and log them in without asking for credentials. On Android, the system browser (Chrome Custom Tab) will by default share cookies with the user’s Chrome browser instance, so persistent cookies from B2C are retained across sessions unless the user clears them. That means Android tends to have SSO by default for the same user if they come back months later and the app triggers an interactive login – it may not prompt for credentials if the cookie is still valid.

**Silent Token Refresh Frequency:** It’s generally best to let MSAL decide when to refresh tokens. The access token’s expiry is known (in the token response, usually 60 minutes). MSAL will automatically use the refresh token after expiry. You don’t need to proactively call `AcquireTokenSilent` on a fixed schedule; just call it whenever you need a valid token, and MSAL will handle if it needs refreshing. If you prefer to pre-empt expiration (say, refresh 5 minutes before expiry), MSAL provides token cache notifications or you can check token expiry in the cache, but this is usually unnecessary. Simplicity: attempt silent auth at app launch, handle failures by interactive prompt.

**Concurrency and Caching:** Be cautious not to call interactive login if a silent call is in progress or vice-versa, as MSAL’s token cache is not thread-safe for concurrent writes. In a Blazor Hybrid app, ensure that token refresh calls are queued or synchronized if coming from the web UI side.

## System Browser vs Embedded WebView

**System Browser (Default):** By default, MSAL.NET on Xamarin/MAUI uses the system web browser for interactive authentication on mobile. On Android this means launching a Custom Tab (or the user’s default browser), and on iOS it means ASWebAuthenticationSession (SafariServices). This approach is **recommended** for most scenarios. Advantages of using the system browser include:

* **Single Sign-On (SSO):** System browsers share authentication state with other apps and with the device’s default browser. If the user has already signed in to the same Azure AD or social IdP via a browser (or another app using MSAL), they might not need to enter credentials again. MSAL notes that the system browser is better at “remembering the users that have logged in before”. It can leverage cookies/token storage that persist beyond the app (especially on Android where it shares Chrome’s cookie jar by default).

* **Security & Compliance:** Using the system browser ensures you’re using a trusted UI (the user can see the actual URL, and on iOS the Safari view controller provides the familiar UI), which prevents UI spoofing. It also means features like **Conditional Access** policies, MFA prompts, and **Apple “Sign in with Apple” flows** or Google login flows will work properly. (Some providers **block embedded webviews** for login due to security – e.g., Google and Apple require a system browser/OAuth session for their login. Using the system browser avoids those issues.)

* **Broker Integration:** Some advanced scenarios involve the **Microsoft Authenticator** app as a broker for Azure AD. System browser is required to hand off to the broker. (While B2C by itself doesn’t always use Authenticator, if your B2C allows social login via Microsoft accounts, etc., Authenticator could act as an broker.) By using system web view, MSAL can detect the broker and hand off if available.

MSAL’s design is to choose the appropriate system web control automatically. You usually do not have to configure anything to use system web – it’s the default for MAUI apps (the MSAL.NET docs show that for .NET MAUI, system web view is available and is the default).

**Embedded WebView (Optional):** MSAL.NET *also* supports using an embedded webview (essentially an in-app browser control) on MAUI if you explicitly enable it. In practice, on Android this might use an embedded Chrome WebView, and on iOS it could use WKWebView. You can force this by calling `.WithUseEmbeddedWebView(true)` on the `AcquireTokenInteractive` builder or the PCA builder. However, using embedded webviews is generally not recommended unless you have a specific need, because:

* The embedded webview will not share cookies or SSO state with other apps or the device’s browser. That means the user will always have to enter credentials afresh in the embedded view (no SSO from a previously logged-in browser session).

* Some identity providers (like Google OAuth or Apple ID) may not allow the auth flow to complete in an embedded webview (Google famously started disallowing OAuth requests from embedded webviews). If your B2C tenant uses Google as an IdP, a custom tab/system browser is needed for the best compatibility.

* There are potential maintenance issues; for example, on iOS, WKWebView might not handle some modern authentication flows (like device certificate authentication or Apple SSO extensions) as seamlessly as ASWebAuthenticationSession.

The primary use-case for embedded webview would be if you want to **customize the UI** or prevent the user from leaving your app’s context at all. But note that even in embedded mode, the web content is served by Azure AD B2C, so you cannot fully control the styling beyond what B2C’s custom HTML/CSS allows.

Given Microsoft’s guidance, **sticking with the system browser is best** for mobile apps. There is one corner case: if your users do not have a default browser or it’s an unusual device without Chrome/Browser (rare on Android), an embedded view might be a fallback. But most developers won’t need to force embedded on MAUI Android/iOS. (On Windows, MAUI apps might use WebView2 in the future, but that’s out of scope here.)

**System Browser UX:** One thing to design for is that when the system browser opens, the user sees a separate screen (on iOS it’s an overlay modal, on Android it might look like a Chrome Custom Tab with a toolbar). Ensure that this transition is expected (perhaps a loading indicator in your app until the callback arrives). On Android, the Custom Tab will show the URL domain (e.g., \*.b2clogin.com) – you can customize the toolbar color to match your app if desired, using MSAL’s configuration (MSAL allows passing an object with custom tab configuration in some libraries).

**Closing the Browser:** After successful sign-in, the browser tab will close automatically (Android) or the modal will dismiss (iOS) via the redirect. If the user *fails* to sign in or closes it manually, your app should detect the cancellation as discussed. In system browser mode, MSAL cannot detect intermediate navigation (for security, it only knows when the final redirect to your scheme happens or the user manually cancels), so keep that in mind.

In summary, **use the default system browser** unless you have a concrete reason to change. MSAL’s default was chosen to maximize SSO and compatibility.

## Platform-Specific Behaviors and User Experience

Building a smooth user experience requires understanding a few platform quirks:

* **Browser Navigation and Back Button:** Mobile users might hit back or close the auth window. As covered, on Android the back button will cancel the flow. Ensure your app’s UI is prepared for that (e.g., if the login was invoked from a login page, simply remain on that page if canceled). On iOS, the user can tap “Cancel” on the Safari auth dialog – treat that similarly. The MSAL result will indicate a cancellation (no tokens acquired).

* **App Resume and State Management:** When the auth completes (or is canceled), your app will come back to the foreground. MAUI apps have a lifecycle event for when they come back (like `OnResume`), but more directly, your code awaiting `AcquireTokenInteractive` will just continue. It’s a good practice to separate the authentication logic from UI, perhaps in a service. For example, you might have an `AuthService.LoginAsync()` that wraps MSAL calls. This method will “pause” while the system browser is up, and then resume when the result comes in. If using Blazor, you can call this via dependency injection and then use the result (e.g., store the ID token/claims in a user session state). Make sure the Blazor UI is updated on the main thread after resume (usually not an issue if you `InvokeAsync` to update UI state). Also, consider scenarios like **app going to background** (user switched apps during login). Usually the system web flow will continue even if the user backgrounded your app, as long as they return and complete it. But if the app was suspended/killed by OS due to memory, MSAL might not resume properly. This is a rare scenario, but in critical applications you might want to handle a fresh login if you detect that the app was restarted.

* **Biometric Authentication Support:** MSAL itself does not directly provide biometric unlock for the authentication flow (because the primary auth is happening against Azure AD B2C). However, you can incorporate biometrics in a couple of ways:

  * **Protecting the Token Cache:** Since MSAL uses secure storage, you could add an extra layer by requiring biometric unlock to access the tokens. For example, on iOS you could use Keychain’s access control flags (if you were doing custom cache serialization) to require Face ID/Touch ID before releasing the item. This would mean even if someone has the phone unlocked and manages to run the app, they’d need to pass a Face ID check to use the refresh token. MSAL.NET doesn’t have a built-in switch for this, so implementing it would involve writing a custom token cache persistence (possibly with **Xamarin.Essentials SecureStorage**, which on iOS can use biometrics). On Android, similarly, you could encrypt the token cache with a key stored in Android Keystore that itself is protected by the device credential or biometric (Android KeyStore allows marking a key as requiring user authentication). .NET MAUI’s `SecureStorage.Default` could be leveraged in combination with such flags.
  * **Broker-based biometric**: If you enable broker (using `.WithBroker(true)` on MSAL and having Authenticator installed and configured), the Microsoft Authenticator app can handle the auth. If the Authenticator app is protected by biometric or PIN (users often secure it), then indirectly you get biometric verification when they approve the sign-in in the broker. However, broker support for Azure AD B2C accounts is not as straightforward as for Azure AD tenants – it might not work for local B2C accounts at all, except for social accounts that Authenticator knows about.
  * **Biometric as primary auth**: Depending on your B2C setup, you could integrate with an IdP that itself uses biometric (for instance, some B2C setups allow phone sign-in via Microsoft account, which on a device could prompt for biometric in Authenticator). But this is more on the server side. Out of the box, B2C doesn’t support Touch ID/Face ID as a direct credential – you’d have to use something like Hello for Business or Authenticator as an IdP.

  In summary, you **can** use biometrics to enhance security, but it requires custom work beyond MSAL’s default. A simpler approach for many apps is to use device biometric to lock the app or certain features (app-level security), rather than tying it to token retrieval. For example, you can have the user perform a fingerprint/FaceID check when the app starts (using MAUI Essentials `Fingerprint` API) and only then use the token from cache. This doesn’t integrate with MSAL directly, but it meets the security requirement that a token isn’t used without biometric verification.

* **Consent and Prompt Behavior:** In B2C, users typically aren’t prompted for consent for the defined scopes (especially not for built-in scopes like openid/offline\_access). If your app calls Microsoft Graph or other APIs, ensure those permissions are granted by an admin upfront (since B2C users can’t consent in some cases by themselves). In your MSAL `AcquireTokenInteractive` call, you can specify `.WithPrompt(Prompt.SelectAccount)` or others if you need to force a login each time (e.g., to let user choose a different account). But generally for B2C you use the default prompt=select\_account on first login, and then silent thereafter.

* **Multiple Policies (User Flows):** A peculiarity of Azure AD B2C is that each user flow is technically a separate authority. If you have a sign-in/sign-up flow and, say, a password reset flow, these are different authorities. MSAL can cache a refresh token per authority. If you invoke a different policy, MSAL might treat the user as a separate “account” (since the authority string differs). One best practice is to have a single combined sign-in/sign-up policy if possible (so you don’t juggle multiple tokens). If you do need to call multiple policies, be aware you might need to call interactive for those and handle their responses separately (e.g., after a password reset, you might get a separate token). This is a complex topic, but just remember: **policy changes = different authority** in MSAL’s eyes.

* **UI Customization:** Because this is a Blazor Hybrid app, you can design your web UI to reflect authentication state (logged-in vs logged-out views) and call .NET methods to trigger MSAL logins. After MSAL obtains an ID token, you can parse it (it’s just a JWT) to get user info (or call Graph/your own API to get profile data). Many developers will surface the user’s name or avatar in the UI. Ensure you handle the case where tokens expire – perhaps by showing a message or seamlessly acquiring a new token when needed without user interruption.

## Best Practices and Troubleshooting Tips

Finally, here are **best practices** and common **troubleshooting tips** for implementing MSAL in a MAUI Blazor app:

* **Use the Latest MSAL Library:** As of 2025, MSAL.NET is regularly updated. Keep your `Microsoft.Identity.Client` NuGet package up to date. New versions fix bugs (for example, issues with newer Android or iOS versions). Note that MSAL.NET 4.61+ officially dropped support for Xamarin.iOS/Android in favor of MAUI, so ensure you’re on a version intended for MAUI (the package still is Microsoft.Identity.Client but you target net6.0/ net7.0 etc.). The current MSAL (4.72.x as of early 2025) supports .NET MAUI.

* **Register multiple Redirect URIs if needed:** If you also target desktop or other platforms with the same client app, you might have additional redirect URIs (like `http://localhost` for desktop). Azure AD B2C allows multiple redirect URIs. For mobile, ensure the `msal<clientid>://auth` is added for “Android” and for “iOS/MacOS” in the platform configuration (in the new Entra portal experience, the **Mobile and Desktop** application option covers both Android and iOS). If you see an error during login like *“The redirect URI specified in the request does not match the redirect URIs configured”*, it’s likely a mismatch in this configuration.

* **Entitlement and Profile Setup (iOS):** If your app fails to cache tokens on iOS, double-check the Entitlements as discussed. The error **“application does not have keychain access groups enabled”** is a tell-tale sign. Make sure your provisioning profile and signing certificate are correctly set up with Keychain. Run a test on a physical iPhone if possible – sometimes simulators handle Keychain more permissively than real devices.

* **Android Manifest and Configuration:** For Android, ensure that **MsalActivity** (or the intent filter) is correctly declared. A common mistake is a typo in the scheme (for instance, including braces or using uppercase). Also, if your package name (application ID) has uppercase letters, Android might normalize schemes to lowercase – best to use the msal<GUID> which is all lowercase. Another tip: if you changed the **Package name** of your app after registering with B2C, remember that for broker scenarios, Azure AD uses package name and signature hash – but if you’re not using broker, you don’t need to configure those in B2C; just the msal<clientid> scheme is enough.

* **Testing with Emulators/Simulators:** When testing on Android emulators, ensure you have the **Android System WebView** component or a browser installed (most modern emulator images do). If the emulator lacks a browser, MSAL’s system browser call might not find one. In such cases, use an emulator with Google Play which has Chrome, or force embedded webview for testing. On iOS simulator, ASWebAuthenticationSession should work out of the box.

* **Silent Token Errors:** If `AcquireTokenSilentAsync` fails with `MsalUiRequiredException`, inspect the `exception.Message` and `ErrorCode`. Common reasons: no account in cache, refresh token expired, or the scope is different. For instance, if you try silent for a scope the user never consented to, it will fail. For B2C, usually you only use the scopes the user flow is configured for (and you consented to openid/offline\_access at app registration time). Also, B2C refresh tokens can become invalid if certain **account events** happen (user changed password or an admin revoked sessions). Your app should catch these and in response, call `AcquireTokenInteractive` to prompt the user to sign in again.

* **Logging and Monitoring:** During development, enable MSAL logging to debug issues. You can do this by configuring `PublicClientApplicationBuilder.WithLogging(...)` and outputting logs to Console or Debug. For production, you might log only warnings/errors. The logs can help if, for example, the app isn’t redirecting back (it might show “Intent mismatch” or something). Also consider instrumenting your app to detect if a user is stuck on a login page or if tokens are not being acquired, so you can diagnose with telemetry.

* **User Experience Tips:**

  * Only trigger the interactive login in response to a user action (like clicking a “Sign In” button) – this gives context to the user. Auto-launching the system browser on app launch (without user interaction) can be jarring unless your app **requires** login to function. If you do auto-launch, be sure to handle the scenario where the user might immediately close it.
  * Provide a clear sign-out option in your UI. When the user chooses to log out, call MSAL to remove accounts. Specifically, call `PublicClientApplication.RemoveAsync(account)` for each account (or `RemoveAsync(null)`/`RemoveAsync(account)` for the single account) to clear tokens. This wipes the tokens from cache. In B2C, you may *optionally* also redirect the user to the B2C sign-out URL in a browser to end the web session (`https://<tenant>.b2clogin.com/<tenant>.onmicrosoft.com/<policy>/oauth2/v2.0/logout`). This will clear the B2C cookie, ensuring no SSO. This is more important if multiple users use the same device. However, the downside is it will require full credentials next time. Decide based on your app’s needs. At minimum, do the RemoveAsync so that the app doesn’t try silent login with an old refresh token.
  * If using social logins in B2C (Google, Facebook, etc.), test each on both platforms. For example, test Google login on an Android device with only the embedded webview – you might see Google’s special notice about not allowing embedded login. This will confirm that system browser is needed.
  * Test the **“reset password”** or **“edit profile”** user flows if you have them. These often return with a special error code to the app (for instance, a reset password policy might return error `AADB2C90118` prompting you to start the password reset flow). Make sure your app can handle these if you include those policies (this typically involves catching an exception from MSAL with that code and then invoking a different policy authority).
  * **Biometric/PIN fallback:** If you want an extra security step for the user when they return to the app (even if still logged in), you can use MAUI Essentials **SecureStorage** or **Preferences** to store a flag like “require PIN on resume” and implement a simple PIN or fingerprint check. This is outside MSAL but can bolster security for cached logins.
  * **UI Loading State:** When initiating a token request, especially silent ones, consider showing a loading indicator in the UI if it might take more than a few seconds (e.g., if the refresh token is expired and MSAL must do an interactive login, etc.). This keeps the user informed.

* **Troubleshooting Non-Returning Browser (Android):** If the behavior is that the browser launches and user signs in, but then the app doesn’t receive a response (stuck on the login page or it closes but no result in app), likely causes are:

  1. **Misconfigured redirect or missing intent-filter** – the browser may be trying to redirect to the scheme but Android isn’t handing it to the app. Check logcat for an error about “Activity not found to handle Intent”. Fix the manifest scheme and host as described.
  2. **Signature hash issues (broker scenarios)** – if you enabled broker (Auth app), the redirect URI becomes `msauth://<packagename>/<signature>` in Azure. If that’s wrong or not in manifest, auth might complete in Authenticator but never return. The fix is to ensure the broker redirect is added as shown in MS docs if using broker. If not using broker, you don’t need to worry about this.
  3. **Chrome Custom Tab not returning** – On some devices, custom tab may keep the session alive. The MSAL log line we saw indicates a fallback to an alternate browser if custom tabs not available. If it’s truly stuck, a workaround can be to force embedded webview (`WithUseEmbeddedWebView(true)`) just to test – if that works, the issue is likely with the app not getting the intent from the custom tab. This points back to the intent-filter config again.

* **Troubleshooting iOS non-return:** If tapping “Continue” after login does nothing or the app doesn’t get a callback:

  * Ensure **OpenUrl** is being called. You can put a breakpoint or log in your AppDelegate.OpenUrl. If it’s not hit, the issue is that iOS isn’t recognizing the URL scheme or your app wasn’t running. Check the scheme registration in Info.plist.
  * If OpenUrl is called but MSAL doesn’t complete, maybe the `AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url)` is missing or returning false. Double-check that code matches the MSAL sample. If using broker, also ensure the broker handling as in sample.
  * Another subtle issue: The **application’s bundle identifier** in the redirect URI must match exactly how it’s registered. If you change the app’s bundle ID, update the Azure AD B2C redirect to use the new clientId (though clientId remains same unless you made a new app registration).

* **Testing in Release mode:** Always test the authentication in a Release build on device. Sometimes things like the linker or different behaviors appear (especially on iOS with Keychain or if any ATS/App Transport Security policies in Info.plist that might affect external endpoints). For example, ensure that `b2clogin.com` is allowed (ATS by default allows HTTPS, so that should be fine).

In conclusion, implementing Azure AD B2C (Microsoft Entra ID External ID) in a .NET MAUI Blazor app involves configuring the application registration with the right redirect URIs and permissions, setting up MSAL on each platform (intent filters on Android, URL handling on iOS), and leveraging MSAL’s features for token caching and renewal. Following the above best practices – using system web views, securing the token cache, handling cancellation, and thoroughly testing – will lead to a robust authentication experience. With the state of tooling in 2025, MSAL.NET and .NET MAUI provide the building blocks for a first-class sign-in flow on mobile, with support for modern auth requirements like MFA, device trust, and more, largely handled for you by the library. By adhering to these guidelines, you can deliver a seamless and secure login experience in your MAUI Blazor Hybrid application.

**Sources:**

* Microsoft Learn – *“.NET MAUI app using MSAL.NET to authenticate users with Azure AD for Customers”* (Microsoft Entra External ID/B2C sample)
* Microsoft Learn – *“Xamarin/MAUI MSAL redirect URI configuration (Android)”*
* Microsoft Learn – *“Handling iOS MSAL return (OpenUrl and Keychain)”*
* Microsoft Learn – *“Token cache serialization in MSAL.NET”* (mobile defaults)
* Microsoft Learn – *“Refresh tokens in Microsoft identity platform”* (token lifetimes)
* Microsoft Learn – *“MSAL.NET using system browser vs embedded webview”* (guidance as of May 2024)
* Microsoft Learn – *“Customize MSAL iOS/macOS webviews”* (system browser usage)
* Stack Overflow – Various threads on MSAL in .NET MAUI (covering cache persistence, Android redirect issues, .NET 9 changes)
