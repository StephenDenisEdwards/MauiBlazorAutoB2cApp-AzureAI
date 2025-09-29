using MauiBlazorAutoB2bApp;
using MauiBlazorAutoB2bApp.Web.Components;
using MauiBlazorAutoB2bApp.Shared.Services;
using MauiBlazorAutoB2bApp.Web;
using MauiBlazorAutoB2bApp.Web.Services;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.Maui.Controls;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/*
	To run your app exclusively in interactive WebAssembly or Interactive Server mode rather than Interactive Auto mode,
	you need to remove any server-side or wasm registrations and ensure your Razor components and startup
	scripts use the WebAssembly or Server settings. Here are the steps:

  App.razor

		<HeadOutlet @rendermode="InteractiveAuto" />
		   ...
		   <Routes @rendermode="InteractiveAuto" />

		<HeadOutlet @rendermode="InteractiveWebAssembly" />
		   ...
		   <Routes @rendermode="InteractiveWebAssembly" />

		<HeadOutlet @rendermode="InteractiveServer" />
	      ...
		<Routes @rendermode="InteractiveAuto" />

	Look for the "<-- comment out for WASM only" and "<-- comment out for SERVER only" comments and
	do as they suggest.

*/

#if INTERACTIVE_SERVER
// Code specific to interactive server mode.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents(); // <-- comment out for WASM only
	//.AddInteractiveWebAssemblyComponents(); // <-- comment out for SERVER only
#elif INTERACTIVE_WASM
// Code specific to interactive WebAssembly mode.
builder.Services.AddRazorComponents()
	//.AddInteractiveServerComponents() // <-- comment out for WASM only
	.AddInteractiveWebAssemblyComponents(); // <-- comment out for SERVER only
#elif INTERACTIVE_AUTO
// Code specific to interactive auto mode.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents() // <-- comment out for WASM only
	.AddInteractiveWebAssemblyComponents(); // <-- comment out for SERVER only
#endif


//builder.Services.AddHttpClient<WeatherService>();
//builder.Services.AddHttpClient<WeatherService>(client =>
//{
// client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
// client.BaseAddress = new Uri("https://localhost:7250/"); // Use your API's URL
//client.BaseAddress = new Uri("https://localhost:7238/"); // Use your API's URL
//});
// Add device-specific services used by the MauiBlazorAutoB2bApp.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IAuthenticationService, MsalWebAuthenticationService>();

builder.Services.AddSingleton<INativeNavigationService, NativeNavigationService>();
builder.Services.AddSingleton<IAccelerometerService, MauiBlazorAutoB2bApp.Web.Services.AccelerometerService>();

//builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped(sp =>
	new WeatherService(new HttpClient
	{
		BaseAddress = new Uri("https://localhost:7038/")
	})
);



// MSAL configuration

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

AzureAdOptions? azureConfig = builder.Configuration.GetSection("AzureAd").Get<AzureAdOptions>();

// Register MSAL public client with login.microsoftonline.com authority
builder.Services.AddSingleton<IPublicClientApplication>(sp =>
	PublicClientApplicationBuilder
		.Create(azureConfig.ClientId)
		.WithExperimentalFeatures() // this is for upcoming logger
		//.WithAuthority(AzureCloudInstance.AzurePublic, azureConfig.TenantId)

		.WithAuthority(AzureCloudInstance.AzurePublic, azureConfig.TenantDomain, true) // Use true for debug mode to allow authority validation
		//.WithAuthority(AzureCloudInstance.AzurePublic, azureConfig.TenantId, true) // Use true for debug mode to allow authority validation

		// .WithAuthority("https://tinglercustomers.ciamlogin.com", "tinglercustomers.onmicrosoft.com")
		.WithAuthority("https://tinglercustomers.ciamlogin.com/tinglercustomers.onmicrosoft.com/tingler-app-userflow")
		.WithRedirectUri(azureConfig.RedirectUri)
		.WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false)    // This is the currently recommended way to log MSAL message. For more info refer to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging. Set Identity Logging level to Warning which is a middle ground
		.Build()
);

//builder.Services.AddMsalAuthentication(options =>
//{
//	builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
//	options.ProviderOptions.LoginMode = "redirect";
//});



builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<IParentWindowProvider, ParentWindowProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();



#if INTERACTIVE_SERVER
// Code specific to interactive server mode.
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode()  // <-- comment out for WASM only
	.AddAdditionalAssemblies(
		typeof(MauiBlazorAutoB2bApp.Shared._Imports).Assembly,
		typeof(MauiBlazorAutoB2bApp.Web.Client._Imports).Assembly);
#elif INTERACTIVE_WASM
// Code specific to interactive WebAssembly mode.
app.MapRazorComponents<App>()
	.AddInteractiveWebAssemblyRenderMode()  // <-- comment out for SERVER only
	.AddAdditionalAssemblies(
		typeof(MauiBlazorAutoB2bApp.Shared._Imports).Assembly,
		typeof(MauiBlazorAutoB2bApp.Web.Client._Imports).Assembly);
#elif INTERACTIVE_AUTO
// Code specific to interactive auto mode.
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode()  // <-- comment out for WASM only
	.AddInteractiveWebAssemblyRenderMode()  // <-- comment out for SERVER only
	.AddAdditionalAssemblies(
		typeof(MauiBlazorAutoB2bApp.Shared._Imports).Assembly,
		typeof(MauiBlazorAutoB2bApp.Web.Client._Imports).Assembly);
#endif


app.Run();
