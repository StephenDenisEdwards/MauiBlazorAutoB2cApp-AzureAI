using MauiBlazorAutoB2cApp.Shared.Services;
using MauiBlazorAutoB2cApp.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AccelerometerService = MauiBlazorAutoB2cApp.Web.Client.Services.AccelerometerService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.Services.AddHttpClient<WeatherService>(client =>
//{
//	// client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
//	// client.BaseAddress = new Uri("https://localhost:7250/"); // Use your API's URL
//	client.BaseAddress = new Uri("https://localhost:7238/"); // Use your API's URL
//});

// Add device-specific services used by the MauiBlazorAutoB2bApp.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
//builder.Services.AddSingleton<MauiBlazorAutoB2bApp.Shared.Services.IAuthenticationService, MsalWebAssemblyAuthenticationService>();
builder.Services.AddScoped<IAuthenticationService, MsalWebAssemblyAuthenticationService>();
//builder.Services.AddScoped<WeatherService>();


builder.Services.AddSingleton<INativeNavigationService, NativeNavigationService>();
builder.Services.AddSingleton<IAccelerometerService, AccelerometerService>();


builder.Services.AddScoped(sp =>
	new WeatherService(new HttpClient
	{
		BaseAddress = new Uri("https://localhost:7038/")
	})
);

builder.Services.AddMsalAuthentication(options =>
{
	builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
	//options.ProviderOptions.DefaultAccessTokenScopes.Add("api://api.id.uri/access_as_user");
});

await builder.Build().RunAsync();