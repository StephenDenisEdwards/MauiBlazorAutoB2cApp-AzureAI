# NET MAUI Blazor Hybrid + Web App

## Render mode for the web app

When you use the **“.NET MAUI Blazor Hybrid + Web App”** template you can decide up-front which interactive render mode the *web* part of the solution will use, and “Interactive WebAssembly” is one of the built-in choices.

### 1  Pick WebAssembly when you create the solution

| Tooling                                                            | How to do it                                                                                                                                                                                                                  |
| ------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **.NET CLI**                                                       | \`\`\`bash                                                                                                                                                                                                                    |
| dotnet new maui-blazor-web -o MyApp -I WebAssembly   # or  -I Auto |                                                                                                                                                                                                                               |
| \`\`\`<br>The \`-I                                                 | --InteractivityPlatform\` switch accepts **Server**, **WebAssembly**, or **Auto**.  Choosing **WebAssembly** scaffolds an extra ***.Web.Client*** project that holds the WebAssembly host and runtime. ([Microsoft Learn][1]) |
| **Visual Studio “Blazor Hybrid App with Web” wizard**              | In the *Interactive render mode* dropdown simply pick **WebAssembly** (or **Auto** if you want the site to start on the server and transparently switch to WASM after the bundle is cached).                                  |

### 2  What’s generated

* **MyApp.Maui** – the native (.NET MAUI) app.
  *Still runs its Razor components in-process—there’s no WebAssembly involved here.*
* **MyApp.Web** – the ASP.NET Core host.
  Its *Program.cs* is pre-wired with:

````csharp
builder.Services.AddRazorComponents()
               .AddInteractiveWebAssemblyComponents();

app.MapRazorComponents<App>()
   .AddInteractiveWebAssemblyRenderMode();
``` :contentReference[oaicite:1]{index=1}
* **MyApp.Web.Client** – the WebAssembly boot-strapper (index.html, service-worker, etc.).  
  Razor components that must ship to the browser are built from this project so they end up in the WASM bundle. :contentReference[oaicite:2]{index=2}

### 3  Switching an existing *Server* solution to WebAssembly

If you already created the template with **Server** you can still convert it:

1. Add a *Client* project (File → Add → Project → “Blazor WebAssembly App (Standalone)”) and reference it from *MyApp.Web*.  
2. In *MyApp.Web/Program.cs* replace the “Server” lines with the --WebAssembly ones shown above.
3. In any Razor page/component you want to run client-side add  
   ```razor
   @rendermode InteractiveWebAssembly
````

or set a global default in **\_Imports.razor**. ([Microsoft Learn][2])

### 4  A quick note on behaviour

* **Interactive WebAssembly** – C# executes in the browser via WASM from the first request.
* **Interactive Auto** – first visit uses Blazor Server; subsequent visits (after the bundle is cached) run in WASM, giving you the best of both worlds. ([Microsoft Learn][2])

The MAUI-side experience is unchanged either way, because a BlazorHybrid app always runs its components directly in the native .NET runtime and just paints the HTML inside a `BlazorWebView`.

So, simply choose **WebAssembly** (or change the render-mode code later) and your web app will be delivered as a true Blazor WebAssembly site instead of a Blazor Server one. Happy coding!

[1]: https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/maui-blazor-web-app?view=aspnetcore-9.0 "Build a .NET MAUI Blazor Hybrid app with a Blazor Web App | Microsoft Learn"
[2]: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-9.0 "ASP.NET Core Blazor render modes | Microsoft Learn"
