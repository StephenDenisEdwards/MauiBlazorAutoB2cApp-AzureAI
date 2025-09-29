//using MauiBlazorAutoB2bApp.MSALClient;

//using MauiBlazorAutoB2bApp.MSALClient;

//using MauiBlazorAutoB2bApp.Platforms.Android.Handlers;

//using MauiBlazorAutoB2bApp.Platforms.Android.Handlers;
using MauiBlazorAutoB2bApp.Services;
using MauiBlazorAutoB2bApp.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.IdentityModel.Abstractions;

namespace MauiBlazorAutoB2bApp;

public static class MauiProgram
{
	public static async Task<MauiApp> CreateMauiAppAsync()
	//public static MauiApp CreateMauiApp()
	{
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

		// Load config
		//builder.Configuration.AddJsonStream(
		// new MemoryStream(
		//  Encoding.UTF8.GetBytes(
		//   File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "appsettings.json"))
		//  )
		// )
		//);

		// Register the native navigation service.
		builder.Services.AddSingleton<INativeNavigationService, NativeNavigationService>();


		if (DeviceInfo.Current.Platform == DevicePlatform.Android)
		{
			using var stream = await FileSystem.OpenAppPackageFileAsync("appsettings-android.json");
			builder.Configuration.AddJsonStream(stream);
		}
		else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
		{
			using var stream = await FileSystem.OpenAppPackageFileAsync("appsettings-ios.json");
			builder.Configuration.AddJsonStream(stream);
		}
		else if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
		{
			using var stream = await FileSystem.OpenAppPackageFileAsync("appsettings-windows.json");
			builder.Configuration.AddJsonStream(stream);
		}

		AzureAdOptions? azureConfig = builder.Configuration.GetSection("AzureAd").Get<AzureAdOptions>();


#if ANDROID
		builder.ConfigureMauiHandlers(handlers =>
		{
		//	handlers.AddHandler<NativeLabel, AndroidNativeLabelHandler>();
		});
#endif


		/*
			"CacheFileName": "msal_cache.txt",
		    "CacheDir": "C:/temp"
		*/

		//var cacheDir = Path.Combine(FileSystem.AppDataDirectory, "msal_cache");
		//var storageProperties = new StorageCreationPropertiesBuilder("msalcache.dat", cacheDir)
		//	.Build();
		//var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);


		if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
		{

			var storageProperties = new StorageCreationPropertiesBuilder("msal_cache.txt", "C:/temp")
				.Build();

			MsalCacheHelper? cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);


			// Register the cache helper as a singleton.
			builder.Services.AddSingleton(cacheHelper);
		}
		else if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
				DeviceInfo.Current.Platform == DevicePlatform.iOS)
		{
			// For these platforms, file cache is not supported; use the built-in in-memory cache.
			// Optionally, you can implement a custom token cache serialization using Secure Storage.
			//builder.Logging.l?.LogWarning("File cache is not supported on this platform; using in-memory token caching.");
		}

		// ?????
		// Set up the secure token cache(for Android / iOS)
		//if (DeviceInfo.Current.Platform != DevicePlatform.Android &&
		//    DeviceInfo.Current.Platform != DevicePlatform.iOS)
		//{
		//	//_ = new SecureTokenCacheHelper(publicClient);
		//	builder.Services.AddSingleton<SecureTokenCacheHelper>();
		//}

		// Register MSAL public client with login.microsoftonline.com authority

		var userFlow = "tingler-app-userflow";
		var authority = $"https://{azureConfig.TenantId}.ciamlogin.com/{azureConfig.TenantId}.onmicrosoft.com/{userFlow}";

		builder.Services.AddSingleton<IPublicClientApplication>(sp =>
				PublicClientApplicationBuilder
					.Create(azureConfig.ClientId)
					.WithExperimentalFeatures() // this is for upcoming logger
					// ✅ Rule of thumb for validateAuthority
					//	AAD(work / school / Microsoft accounts) → validateAuthority: true(default).
					//	AAD B2C / Entra External ID(CIAM) → validateAuthority: false, because the authority includes a user flow (policy).
					.WithAuthority(authority, validateAuthority:false) // Use true for debug mode to allow authority validation
					.WithRedirectUri(azureConfig.RedirectUri)
					.WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false)    // This is the currently recommended way to log MSAL message. For more info refer to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging. Set Identity Logging level to Warning which is a middle ground
					.Build()
			);

		
		builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();


		//builder.Services.AddHttpClient<WeatherService>(client =>
		//{
		// // client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
		// // client.BaseAddress = new Uri("https://localhost:7250/"); // Use your API's URL
		// client.BaseAddress = new Uri("https://localhost:7238/"); // Use your API's URL
		//});

		// Add device-specific services used by the MauiBlazorAutoB2bApp.Shared project
		builder.Services.AddSingleton<IFormFactor, FormFactor>();
		// Register WeatherService for dependency injection
		builder.Services.AddScoped<WeatherService>();
		//		builder.Services.AddSingleton<IAuthService, AuthService>();
		//builder.Services.AddScoped(sp =>
		//	new WeatherService(new HttpClient
		//	{
		//		BaseAddress = new Uri("https://localhost:7038/")
		//	})
		//);

		// MauiProgram.cs
		//		builder.Services.AddHttpClient("MyApi", client =>
		//		{
		//#if ANDROID
		//			client.BaseAddress = new Uri("http://10.0.2.2:7038/");
		//#elif IOS && !TARGET_IPHONE_SIMULATOR
		//		        client.BaseAddress = new Uri("http://192.168.65.165:7038/");
		//#else
		//		        client.BaseAddress = new Uri("http://localhost:7038/");
		//#endif
		//		});


		//platform == DevicePlatform.Android

		builder.Services.AddSingleton<IAccelerometerService, AccelerometerService>();


		builder.Services.AddHttpClient<WeatherService>(client =>
		{
			// same BaseAddress logic you already have…
			// client.BaseAddress = new Uri("http://localhost:7038/");
			//DevicePlatform pf = DeviceInfo.Current.Platform;
			//bool isSim = DeviceInfo.Current.DeviceType == DeviceType.Virtual;


#if ANDROID
			client.BaseAddress = new Uri("https://10.0.2.2:7038/");
			// client.BaseAddress = new Uri("http://10.0.2.2:5246/");
			
#elif IOS && !TARGET_IPHONE_SIMULATOR
		        client.BaseAddress = new Uri("https://192.168.65.165:7038/");
#else
		        client.BaseAddress = new Uri("https://localhost:7038/");
#endif

			//client.BaseAddress = new Uri(baseUrl);

			//string baseUrl = DeviceInfo.Platform switch
			//{
			//	DevicePlatform.Android => "http://10.0.2.2:7038/",
			//	DevicePlatform.iOS when !DeviceInfo.Idiom.Equals(DeviceIdiom.Simulator)
			//		=> "http://192.168.65.165:7038/",
			//	_ => "http://localhost:7038/"
			//};

			//client.BaseAddress = new Uri(baseUrl);


		}).ConfigurePrimaryHttpMessageHandler(() =>
			// WARNING: This handler accepts any SSL certificate. Use only for development purposes.

			new HttpClientHandler
			{
				// accept any cert (dev only!)
				ServerCertificateCustomValidationCallback =
					HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
			});



		//builder.Services.AddScoped(sp =>
		//	new WeatherService(new HttpClient
		//	{
		//		BaseAddress = new Uri("https://localhost:7038/")
		//	})
		//);


		builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif


#if ANDROID
	   builder.Services.AddSingleton<IParentWindowProvider, MauiBlazorAutoB2bApp.Android.ParentWindowProvider>();
#elif IOS
    builder.Services.AddSingleton<IParentWindowProvider, MauiBlazorAutoB2bApp.iOS.ParentWindowProvider>();
#elif WINUI
    builder.Services.AddSingleton<IParentWindowProvider, MauiBlazorAutoB2bApp.Platforms.Windows.ParentWindowProvider>();
#elif WINDOWS
    builder.Services.AddSingleton<IParentWindowProvider, MauiBlazorAutoB2bApp.Platforms.Windows.ParentWindowProvider>();
#endif

		//if (DeviceInfo.Current.Platform == DevicePlatform.Android)
		//{
		//	builder.Services.AddSingleton<IParentWindowProvider, MauiBlazorAutoB2bApp.Android.ParentWindowProvider>();
		//}
		//else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
		//{
		//	builder.Services.AddSingleton<IParentWindowProvider, MauiBlazorAutoB2bApp.iOS.ParentWindowProvider>();
		//}
		//else if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
		//{
		//builder.Services.AddSingleton<IParentWindowProvider, MauiBlazorAutoB2bApp.WinUi.ParentWindowProvider>();
		//}


		// Register MSALClientHelper with Entra External ID B2C config
		//builder.Services.AddSingleton<MSALClientHelper>(sp =>
		//{
		// var config = new AzureAdConfig
		// {
		//  ClientId = "your-client-id",
		//  Authority = "https://yourtenant.b2clogin.com/tfp/yourtenant/B2C_1_signupsignin",
		//  TenantId = "your-tenant-id",
		//  CacheFileName = "msalcache.dat",
		//  CacheDir = FileSystem.AppDataDirectory
		// };
		// return new MSALClientHelper(config);
		//});


		return builder.Build();
    }
}
