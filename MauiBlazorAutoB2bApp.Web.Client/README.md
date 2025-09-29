# Markdown File



I still get the error message : blazor.web.js:1  crit: Microsoft.AspNetCore.Components.WebAssembly.Rendering.WebAssemblyRenderer[100]
      Unhandled exception rendering component: Could not find 'AuthenticationService.init' ('AuthenticationService' was undefined).
      Error: Could not find 'AuthenticationService.init' ('AuthenticationService' was undefined).
          at https://localhost:7250/_framework/blazor.web.js:1:384
          at Array.forEach (<anonymous>)
          at l.findFunction (https://localhost:7250/_framework/blazor.web.js:1:352)
          at _ (https://localhost:7250/_framework/blazor.web.js:1:5101)
          at https://localhost:7250/_framework/blazor.web.js:1:2894
          at new Promise (<anonymous>)
          at w.beginInvokeJSFromDotNet (https://localhost:7250/_framework/blazor.web.js:1:2857)
          at Object.jr [as invokeJSJson] (https://localhost:7250/_framework/blazor.web.js:1:165356)
          at https://localhost:7250/_framework/dotnet.runtime.5nhp1wfg9b.js:3:33879
          at Fc (https://localhost:7250/_framework/dotnet.runtime.5nhp1wfg9b.js:3:172343)
Microsoft.JSInterop.JSException: Could not find 'AuthenticationService.init' ('AuthenticationService' was undefined).
Error: Could not find 'AuthenticationService.init' ('AuthenticationService' was undefined).
    at https://localhost:7250/_framework/blazor.web.js:1:384
    at Array.forEach (<anonymous>)
    at l.findFunction (https://localhost:7250/_framework/blazor.web.js:1:352)
    at _ (https://localhost:7250/_framework/blazor.web.js:1:5101)
    at https://localhost:7250/_framework/blazor.web.js:1:2894
    at new Promise (<anonymous>)
    at w.beginInvokeJSFromDotNet (https://localhost:7250/_framework/blazor.web.js:1:2857)
    at Object.jr [as invokeJSJson] (https://localhost:7250/_framework/blazor.web.js:1:165356)
    at https://localhost:7250/_framework/dotnet.runtime.5nhp1wfg9b.js:3:33879
    at Fc (https://localhost:7250/_framework/dotnet.runtime.5nhp1wfg9b.js:3:172343)
   at Microsoft.JSInterop.JSRuntime.<InvokeAsync>d__16`1[[Microsoft.JSInterop.Infrastructure.IJSVoidResult, Microsoft.JSInterop, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60]].MoveNext()
   at Microsoft.JSInterop.JSRuntimeExtensions.InvokeVoidAsync(IJSRuntime jsRuntime, String identifier, Object[] args)
   at Microsoft.AspNetCore.Components.WebAssembly.Authentication.RemoteAuthenticationService`3.<EnsureAuthService>d__30[[Microsoft.AspNetCore.Components.WebAssembly.Authentication.RemoteAuthenticationState, Microsoft.AspNetCore.Components.WebAssembly.Authentication, Version=9.0.5.0, Culture=neutral, PublicKeyToken=adb9793829ddae60],[Microsoft.AspNetCore.Components.WebAssembly.Authentication.RemoteUserAccount, Microsoft.AspNetCore.Components.WebAssembly.Authentication, Version=9.0.5.0, Culture=neutral, PublicKeyToken=adb9793829ddae60],[Microsoft.Authentication.WebAssembly.Msal.Models.MsalProviderOptions, Microsoft.Authentication.WebAssembly.Msal, Version=9.0.5.0, Culture=neutral, PublicKeyToken=adb9793829ddae60]].MoveNext()
   at Microsoft.AspNetCore.Components.WebAssembly.Authentication.RemoteAuthenticationService`3.<SignInAsync>d__20[[Microsoft.AspNetCore.Components.WebAssembly.Authentication.RemoteAuthenticationState, Microsoft.AspNetCore.Components.WebAssembly.Authentication, Version=9.0.5.0, Culture=neutral, PublicKeyToken=adb9793829ddae60],[Microsoft.AspNetCore.Components.WebAssembly.Authentication.RemoteUserAccount, Microsoft.AspNetCore.Components.WebAssembly.Authentication, Version=9.0.5.0, Culture=neutral, PublicKeyToken=adb9793829ddae60],[Microsoft.Authentication.WebAssembly.Msal.Models.MsalProviderOptions, Microsoft.Authentication.WebAssembly.Msal, Version=9.0.5.0, Culture=neutral, PublicKeyToken=adb9793829ddae60]].MoveNext()
   at MauiBlazorAutoB2bApp.Web.Client.Services.MsalWebAssemblyAuthenticationService.SignInAsync() in C:\Users\steph\source\repos\Maui\B2C\MauiBlazorAutoB2bApp\MauiBlazorAutoB2bApp.Web.Client\Services\MsalWebAssemblyAuthenticationService.cs:line 90
   at MauiBlazorAutoB2bApp.Shared.Components.LoginDisplay.HandleSignIn() in C:\Users\steph\source\repos\Maui\B2C\MauiBlazorAutoB2bApp\MauiBlazorAutoB2bApp.Shared\Components\LoginDisplay.razor:line 74
   at Microsoft.AspNetCore.Components.ComponentBase.CallStateHasChangedOnAsyncCompletion(Task task)
   at Microsoft.AspNetCore.Components.RenderTree.Renderer.GetErrorHandledTask(Task taskToHandle, ComponentState owningComponentState)




https://localhost:7250/_content/Microsoft.Authentication.WebAssembly.Msal/AuthenticationService.js
