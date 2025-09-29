# .NET MAUI Blazor Hybrid Web App

A **.NET MAUI Blazor Hybrid Web App** combines two of Microsoft’s modern frameworks—.NET MAUI (Multi-platform App UI) and Blazor—to let you build a cross-platform native client that renders your UI using web technologies (HTML/CSS) powered by Razor components.

---

## Key Concepts

* **.NET MAUI**

  * Successor to Xamarin.Forms, provides a single-project model to build native iOS, Android, macOS and Windows apps in C#/.NET.
  * Gives you access to native device APIs (geolocation, camera, sensors) and native rendering performance.

* **Blazor (Hybrid)**

  * Blazor lets you build web UIs using Razor syntax (.razor files), C#, and standard web technologies.
  * In *Blazor Hybrid* mode, your Razor components run inside a native host (in-process WebView), not in the browser. UI updates are handled via a real-time bridge between .NET and the WebView.

* **Hybrid Web App**

  * The app is packaged as a native app but uses a WebView to render HTML/CSS.
  * All code (UI logic, rendering, data access) runs on the client device—no JavaScript WebAssembly runtime or server-side calls are required after startup.

---

## How It Works

1. **Single Project**
   You create a `.NET MAUI Blazor App` project in Visual Studio:

   ```bash
   dotnet new maui-blazor -n MyHybridApp
   ```
2. **Razor Components**
   Write Razor components (`.razor`) as you would in a Blazor WebAssembly app:

   ```razor
   @page "/counter"

   <h1>Counter</h1>
   <p>Current count: @_count</p>
   <Button Clicked="@IncrementCount">Click me</Button>

   @code {
     private int _count = 0;
     private void IncrementCount() => ++_count;
   }
   ```
3. **Native Host**
   The `MauiProgram.cs` sets up the BlazorWebView:

   ```csharp
   builder.Services.AddBlazorWebView();
   …
   public static MauiApp CreateMauiApp()
   {
     var builder = MauiApp.CreateBuilder();
     builder
       .UseMauiApp<App>()
       .ConfigureFonts(...)
       .Services.AddBlazorWebView();
     return builder.Build();
   }
   ```
4. **WebView Embedding**
   The `<BlazorWebView>` control in `MainPage.xaml` hosts your Razor components:

   ```xml
   <ContentPage xmlns="..." 
                xmlns:blazor="clr-namespace:Microsoft.AspNetCore.Components.WebView.Maui;assembly=Microsoft.AspNetCore.Components.WebView.Maui">
     <blazor:BlazorWebView HostPage="wwwroot/index.html">
       <blazor:BlazorWebView.RootComponents>
         <blazor:RootComponent Selector="#app" ComponentType="{x:Type local:App}" />
       </blazor:BlazorWebView.RootComponents>
     </blazor:BlazorWebView>
   </ContentPage>
   ```

---

## Benefits

* **Code Reuse**: Share UI logic and components between web and native client.
* **Performance**: Runs .NET code natively; UI updates are efficiently diffed via the in-process bridge.
* **Access to Device APIs**: Call native APIs (Bluetooth, GPS, file system) from your Razor components via dependency injection or platform-specific code.
* **Unified Tooling**: Develop, debug and package with Visual Studio / CLI in one solution.

---

## When to Use

* You already have Blazor components/web UIs and want to ship them as a native app without rewriting in XAML.
* You need offline-capable, client-heavy logic with rich web-style UI.
* You want tight integration with device features alongside your existing web-centric codebase.

---

## Alternatives

* **Blazor WebAssembly** (pure browser-hosted) – if you don’t need native device APIs.
* **.NET MAUI with XAML** – if you prefer native UI definitions over HTML/CSS.
* **React Native / Flutter** – other cross-platform frameworks with JavaScript or Dart.

---

In short, a .NET MAUI Blazor Hybrid Web App gives you the best of both worlds: the rich, familiar Razor/web component model and the power of native cross-platform APIs, all in one unified .NET project.
