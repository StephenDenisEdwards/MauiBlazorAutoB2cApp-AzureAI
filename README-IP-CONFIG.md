

# IP Configuration for .NET MAUI Blazor App

## Windows, Android emulator, IOS Simulator 

// MauiProgram.cs

``` CSharp
builder.Services.AddHttpClient("MyApi", client =>
{
    #if ANDROID
        client.BaseAddress = new Uri("http://10.0.2.2:5000/");
    #elif IOS && !TARGET_IPHONE_SIMULATOR
        client.BaseAddress = new Uri("http://192.168.1.42:5000/");
    #else
        client.BaseAddress = new Uri("http://localhost:5000/");
    #endif
});
```

Windows targets → localhost OK.

Android emulators need 10.0.2.2 (or Genymotion’s 10.0.3.2).

iOS Simulator can keep localhost, real devices need your PC’s LAN IP.

Remember to enable clear-text HTTP or run HTTPS with a dev certificate if you’re not on port 80/443.

That way, in debug mode across all targets you’ll always hit your local Web API.


# WASM

When you move to a Blazor WebAssembly (WASM) client, you’re no longer dealing with emulators or physical devices—the app is running in the user’s browser. That means:

“localhost” is your development machine

When you debug a Blazor WASM app in Visual Studio, it’s served from something like https://localhost:5001 (the client) and your API might live at https://localhost:5005. Any call to https://localhost:5005/api/... really does target your PC, because the browser is running on your PC.

Relative URLs vs. absolute URLs

Hosted template (Client + Server together): the WASM client is served by your ASP .NET Core host. You typically do:

``` CSharp
builder.Services.AddScoped(sp =>
  new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
```

Then any HttpClient.GetFromJsonAsync("weatherforecast") becomes a call to https://localhost:5001/weatherforecast, automatically proxied to your server project.

Standalone WASM (Client-only): you’ll need to point at the API’s port explicitly:

``` CSharp
var apiUrl = builder.Configuration["ApiBaseUrl"]; // e.g. "https://localhost:5005/"
builder.Services.AddScoped(sp =>
  new HttpClient { BaseAddress = new Uri(apiUrl) });
CORS
```
If your client (at https://localhost:5001) is on a different port than your API (at https://localhost:5005), the browser will enforce cross-origin rules. You must enable CORS in your API’s Program.cs:

``` CSharp
builder.Services.AddCors(opts =>
  opts.AddDefaultPolicy(policy =>
    policy.WithOrigins("https://localhost:5001")
          .AllowAnyHeader()
          .AllowAnyMethod()));
// …
app.UseCors();
```
If you host client and server under the same origin (the “hosted” Blazor template), CORS isn’t required.

HTTPS & Certificates

Browsers strongly prefer (or even require) HTTPS. Make sure both your client and API are using the dev HTTPS certificates that Visual Studio provisions.

If you accidentally call http://localhost:5000 from a page served over HTTPS, the browser will block the request as mixed content.

Putting it all together

## Hosted Blazor WASM

``` CSharp
// Program.cs (Client)
builder.Services.AddScoped(sp =>
  new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
Standalone Blazor WASM
```

``` CSharp
// appsettings.Development.json
{
  "ApiBaseUrl": "https://localhost:5005/"
}

// Program.cs (Client)
var apiUrl = builder.Configuration["ApiBaseUrl"];
builder.Services.AddScoped(sp =>
  new HttpClient { BaseAddress = new Uri(apiUrl) });
```


### API Project

``` CSharp
// Program.cs
builder.Services.AddCors(opts =>
  opts.AddDefaultPolicy(policy =>
    policy.WithOrigins("https://localhost:5001") // your client port
          .AllowAnyHeader()
          .AllowAnyMethod()));
// …
app.UseCors();
```
With that in place, your Blazor WASM client will happily talk to your locally-hosted Web API at “localhost” so long as the ports line up and CORS/HTTPS are configured correctly.


# SSL Issues with Android emulator

That SSL error on Android usually means your emulator doesn’t trust the dev-cert you’re using for HTTPS. You’ve got three common ways to solve it:

---

### 1) Trust the ASP.NET Core HTTPS dev-certificate on the emulator

1. **On your machine**, run:

   ```bash
   dotnet dev-certs https --trust
   ```
2. **Push the cert** into the Android emulator’s system store (you’ll need to extract the `.cer` file from your user profile):

   ```bash
   # locate the certificate (on Windows, e.g.) 
   #    %USERPROFILE%\.aspnet\https\YourAppName.pfx 
   # export it:
   openssl pkcs12 -in YourAppName.pfx -nodes -nokeys -out aspnet-dev-cert.cer

   # then push & install in the emulator:
   adb root
   adb remount
   adb push aspnet-dev-cert.cer /system/etc/security/cacerts/
   ```

   After rebooting the emulator, it will trust your local HTTPS endpoint.

---

### 2) Allow clear-text HTTP in debug (bypass HTTPS altogether)

If you’re just prototyping, you can switch to plain `http://10.0.2.2:7038/` and tell Android to allow clear-text traffic:

1. Create `Resources/xml/network_security_config.xml`:

   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <network-security-config>
     <domain-config cleartextTrafficPermitted="true">
       <domain includeSubdomains="true">10.0.2.2</domain>
     </domain-config>
   </network-security-config>
   ```

2. In your `AndroidManifest.xml` (inside the `<application>` tag):

   ```xml
   <application
       android:networkSecurityConfig="@xml/network_security_config"
       android:usesCleartextTraffic="true"
       …>
   ```

That way you can keep using `http://…` without SSL errors.

---

### 3) (Quick & Dirty) Ignore SSL errors in your HttpClient

> **Warning:** only do this in development!

```csharp
builder.Services
  .AddHttpClient<WeatherService>(client =>
  {
      // your BaseAddress logic…
      client.BaseAddress = new Uri("https://10.0.2.2:7038/");
  })
  .ConfigurePrimaryHttpMessageHandler(() =>
      new HttpClientHandler
      {
          // accept any cert (dev only!)
          ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
      });
```

---

#### Which to pick?

* **#1** is ideal if you want real SSL on the emulator.
* **#2** is simplest if you don’t care about HTTPS in dev.
* **#3** is fastest hack but *only* for quick debugging.

Once you’ve done one of these, you should stop seeing the “SSL connection could not be established” error on Android.
